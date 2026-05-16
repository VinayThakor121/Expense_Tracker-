namespace Expense_Tracker.DTOs;

public class MonthlyFinancialSummaryDto
{
    public string CurrencySymbol { get; set; } = "₹";
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal Balance { get; set; }
    public string HighestSpendingCategory { get; set; } = "N/A";
    public string MonthlyFinancialBehavior { get; set; } = string.Empty;
    public List<ExpenseCategoryBreakdownDto> CategoryBreakdown { get; set; } = [];
}
