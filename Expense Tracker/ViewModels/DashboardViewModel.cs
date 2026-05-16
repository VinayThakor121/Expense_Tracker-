using Expense_Tracker.Models;

namespace Expense_Tracker.ViewModels;

public class DashboardViewModel
{
    public string TotalIncome { get; set; } = "₹0";
    public string TotalExpense { get; set; } = "₹0";
    public string Balance { get; set; } = "₹0";
    public List<Transaction> RecentTransactions { get; set; } = [];
}
