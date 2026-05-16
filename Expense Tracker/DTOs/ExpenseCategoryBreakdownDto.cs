namespace Expense_Tracker.DTOs;

public class ExpenseCategoryBreakdownDto
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}
