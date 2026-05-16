namespace Expense_Tracker.DTOs;

public class ExpenseCategoryBreakdownDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int Amount { get; set; }
    public decimal Percentage { get; set; }
}
