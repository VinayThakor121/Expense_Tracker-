using Expense_Tracker.DTOs;

namespace Expense_Tracker.Services;

public interface IAiSummaryService
{
    Task<string> GenerateMonthlySummaryAsync(MonthlyFinancialSummaryDto monthlySummary, CancellationToken cancellationToken = default);
}
