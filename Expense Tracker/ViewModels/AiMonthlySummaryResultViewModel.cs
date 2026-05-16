namespace Expense_Tracker.ViewModels;

public class AiMonthlySummaryResultViewModel
{
    public string Summary { get; set; } = string.Empty;
    public string HighestSpendingCategory { get; set; } = "N/A";
    public decimal ExpenseToIncomeRatio { get; set; }
    public bool IsHighSpending { get; set; }
}
