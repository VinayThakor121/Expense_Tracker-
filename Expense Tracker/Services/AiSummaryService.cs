using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Expense_Tracker.DTOs;
using Expense_Tracker.Settings;
using Microsoft.Extensions.Options;

namespace Expense_Tracker.Services;

public class AiSummaryService : IAiSummaryService
{
    private const int MaxOutputTokens = 400;
    private readonly HttpClient _httpClient;
    private readonly GeminiApiSettings _geminiApiSettings;

    public AiSummaryService(HttpClient httpClient, IOptions<GeminiApiSettings> geminiApiOptions)
    {
        _httpClient = httpClient;
        _geminiApiSettings = geminiApiOptions.Value;
    }

    public async Task<string> GenerateMonthlySummaryAsync(
        MonthlyFinancialSummaryDto monthlySummary,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_geminiApiSettings.ApiKey))
        {
            throw new InvalidOperationException("Gemini API key is not configured.");
        }

        var prompt = BuildPrompt(monthlySummary);
        var requestPayload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.4,
                maxOutputTokens = MaxOutputTokens
            }
        };

        var requestUrl =
            $"{_geminiApiSettings.BaseUrl.TrimEnd('/')}/v1beta/models/{_geminiApiSettings.Model}:generateContent?key={_geminiApiSettings.ApiKey}";

        using var response = await _httpClient.PostAsJsonAsync(requestUrl, requestPayload, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Gemini API request failed: {(int)response.StatusCode} - {responseContent}");
        }

        var summaryText = ExtractSummaryText(responseContent);
        if (string.IsNullOrWhiteSpace(summaryText))
        {
            throw new InvalidOperationException("AI summary could not be generated from Gemini response.");
        }

        return summaryText;
    }

    private static string BuildPrompt(MonthlyFinancialSummaryDto monthlySummary)
    {
        var currency = string.IsNullOrWhiteSpace(monthlySummary.CurrencySymbol) ? "₹" : monthlySummary.CurrencySymbol;
        var breakdownLines = monthlySummary.CategoryBreakdown.Count == 0
            ? "- No expense categories recorded this month."
            : string.Join(Environment.NewLine,
                monthlySummary.CategoryBreakdown.Select(x =>
                    $"- {x.CategoryName}: {currency}{x.Amount.ToString("N0", CultureInfo.InvariantCulture)} ({x.Percentage:F2}%)"));

        return $"""
                You are a professional personal finance assistant.
                Use the financial data below to generate a concise, professional, and personalized monthly expense summary for the user.

                Monthly Financial Data:
                - Total income: {currency}{monthlySummary.TotalIncome.ToString("N0", CultureInfo.InvariantCulture)}
                - Total expenses: {currency}{monthlySummary.TotalExpenses.ToString("N0", CultureInfo.InvariantCulture)}
                - Remaining balance: {currency}{monthlySummary.Balance.ToString("N0", CultureInfo.InvariantCulture)}
                - Top spending category: {monthlySummary.HighestSpendingCategory}
                - Monthly behavior note: {monthlySummary.MonthlyFinancialBehavior}
                - Category-wise expense breakdown:
                {breakdownLines}

                Return the output with exactly these sections:
                1) Spending overview
                2) Financial behavior analysis
                3) Saving suggestions
                4) Unusual spending observations
                5) Personalized finance advice

                Keep it brief (120-180 words), practical, and easy to understand.
                """;
    }

    private static string ExtractSummaryText(string responseJson)
    {
        using var document = JsonDocument.Parse(responseJson);
        var root = document.RootElement;

        if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
        {
            return string.Empty;
        }

        var textBuilder = new StringBuilder();
        var firstCandidate = candidates[0];
        if (!firstCandidate.TryGetProperty("content", out var content))
        {
            return string.Empty;
        }

        if (!content.TryGetProperty("parts", out var parts))
        {
            return string.Empty;
        }

        foreach (var part in parts.EnumerateArray())
        {
            if (part.TryGetProperty("text", out var textElement))
            {
                textBuilder.AppendLine(textElement.GetString());
            }
        }

        return textBuilder.ToString().Trim();
    }
}
