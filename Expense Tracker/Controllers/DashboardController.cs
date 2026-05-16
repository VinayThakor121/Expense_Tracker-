using Expense_Tracker.DTOs;
using Expense_Tracker.Models;
using Expense_Tracker.Services;
using Expense_Tracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAiSummaryService _aiSummaryService;

        public DashboardController(ApplicationDbContext context, IAiSummaryService aiSummaryService)
        {
            _context = context;
            _aiSummaryService = aiSummaryService;
        }

        public async Task<ActionResult> Index()
        {
            int totalIncome = await _context.Transactions
                .Include(x => x.Category)
                .Where(i => i.Category != null && i.Category.Type == "Income")
                .SumAsync(i => i.Amount);

            int totalExpense = await _context.Transactions
                .Include(x => x.Category)
                .Where(i => i.Category != null && i.Category.Type == "Expense")
                .SumAsync(i => i.Amount);

            int balance = totalIncome - totalExpense;

            var recentTransactions = await _context.Transactions
                .Include(i => i.Category)
                .OrderByDescending(i => i.Date)
                .Take(5)
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                TotalIncome = totalIncome.ToString("C0"),
                TotalExpense = totalExpense.ToString("C0"),
                Balance = balance.ToString("C0"),
                RecentTransactions = recentTransactions
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateAiMonthlySummary(CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTime.Now;
                var monthStart = new DateTime(now.Year, now.Month, 1);
                var monthEnd = monthStart.AddMonths(1);

                var monthlyTransactions = await _context.Transactions
                    .Include(t => t.Category)
                    .Where(t => t.Date >= monthStart && t.Date < monthEnd)
                    .ToListAsync(cancellationToken);

                int totalIncome = monthlyTransactions
                    .Where(t => t.Category?.Type == "Income")
                    .Sum(t => t.Amount);

                int totalExpenses = monthlyTransactions
                    .Where(t => t.Category?.Type == "Expense")
                    .Sum(t => t.Amount);

                int balance = totalIncome - totalExpenses;
                var categoryBreakdown = monthlyTransactions
                    .Where(t => t.Category?.Type == "Expense")
                    .GroupBy(t => string.IsNullOrWhiteSpace(t.Category!.Title) ? "Uncategorized" : t.Category.Title)
                    .Select(group => new ExpenseCategoryBreakdownDto
                    {
                        CategoryName = group.Key,
                        Amount = group.Sum(t => t.Amount),
                    })
                    .OrderByDescending(x => x.Amount)
                    .ToList();

                foreach (var breakdown in categoryBreakdown)
                {
                    breakdown.Percentage = totalExpenses == 0
                        ? 0
                        : Math.Round((breakdown.Amount * 100m) / totalExpenses, 2);
                }

                var highestSpendingCategory = categoryBreakdown.FirstOrDefault()?.CategoryName ?? "N/A";
                var expenseToIncomeRatio = totalIncome == 0 ? (totalExpenses > 0 ? 100m : 0m) : (totalExpenses * 100m) / totalIncome;

                var monthlySummary = new MonthlyFinancialSummaryDto
                {
                    TotalIncome = totalIncome,
                    TotalExpenses = totalExpenses,
                    Balance = balance,
                    HighestSpendingCategory = highestSpendingCategory,
                    CategoryBreakdown = categoryBreakdown,
                    MonthlyFinancialBehavior =
                        $"Spending-to-income ratio is {Math.Round(expenseToIncomeRatio, 2)}%. Balance is {(balance >= 0 ? "positive" : "negative")}."
                };

                var summary = await _aiSummaryService.GenerateMonthlySummaryAsync(monthlySummary, cancellationToken);

                return Ok(new AiMonthlySummaryResultViewModel
                {
                    Summary = summary,
                    HighestSpendingCategory = highestSpendingCategory,
                    ExpenseToIncomeRatio = Math.Round(expenseToIncomeRatio, 2),
                    IsHighSpending = expenseToIncomeRatio > 80 || balance < 0
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { message = "Unable to generate AI summary right now. Please try again shortly." });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An unexpected error occurred while generating your monthly summary." });
            }
        }
    }
}
