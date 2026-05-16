namespace Expense_Tracker.DTOs;

public class MonthlyFinancialSummaryDto
{
    public int TotalIncome { get; set; }
    public int TotalExpenses { get; set; }
    public int Balance { get; set; }
    public string HighestSpendingCategory { get; set; } = "N/A";
    public string MonthlyFinancialBehavior { get; set; } = string.Empty;
    public List<ExpenseCategoryBreakdownDto> CategoryBreakdown { get; set; } = [];
}
