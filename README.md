# Expense Tracker (ASP.NET MVC + AI Insights)

Expense Tracker is a full-stack ASP.NET MVC personal finance application for managing incomes, expenses, and categories with actionable monthly insights.  
It now includes an **AI-powered Monthly Expense Summary** feature built with **Google Gemini API**.

## Project Description

This project helps users:
- Record and manage income/expense transactions
- Organize transactions by category
- Monitor overall financial position with dashboard statistics
- Generate AI-driven monthly financial analysis and savings suggestions

## Feature List

- Category management (create, edit, delete, view)
- Transaction management (income and expense entries)
- Dashboard cards for total income, expense, and balance
- Recent transaction history
- **AI Monthly Expense Summary (Gemini-powered)**
  - Monthly spending overview
  - Financial behavior analysis
  - Saving suggestions
  - Unusual spending observations
  - Personalized finance advice

## AI Monthly Expense Summary

The dashboard includes a **Generate AI Summary** button that:
1. Fetches the current month’s transactions
2. Calculates:
   - total income
   - total expenses
   - remaining balance
   - highest spending category
   - category-wise expense breakdown
   - percentage spending by category
3. Sends structured data to Google Gemini
4. Displays concise and personalized AI insights in a responsive summary card

The UI includes:
- Loading spinner while AI summary is generated
- Warning badge for high-spending months
- Graceful error handling with user-friendly messages

## Technologies Used

- ASP.NET Core MVC
- Entity Framework Core
- MySQL (Pomelo EF provider)
- Razor Views
- Bootstrap
- Google Gemini API (HttpClient integration)

## Gemini API Setup

Add your Gemini API key in `Expense Tracker/appsettings.json` (or `appsettings.Development.json`):

```json
"GeminiApi": {
  "ApiKey": "YOUR_GEMINI_API_KEY",
  "Model": "gemini-1.5-flash",
  "BaseUrl": "https://generativelanguage.googleapis.com"
}
```

> Recommended for production: store secrets using environment variables or a secure secret manager.

## How AI Summary Works

- A service layer (`IAiSummaryService` / `AiSummaryService`) builds a structured financial prompt.
- The prompt includes totals, balance, top category, monthly behavior note, and category percentages.
- Gemini returns a concise professional summary in defined sections.
- The dashboard renders the summary with spending indicators for quick decision-making.

## Prerequisites

- .NET SDK 8.0+
- Visual Studio 2022 (ASP.NET workload) or VS Code
- MySQL Server
- Git

## Running the Project

1. Clone the repository:
   ```bash
   git clone https://github.com/VinayThakor121/Expense_Tracker-.git
   cd Expense_Tracker-
   ```

2. Update database connection string in `Expense Tracker/appsettings.json`.

3. Apply EF migrations:
   ```bash
   dotnet ef database update --project "Expense Tracker/Expense Tracker.csproj"
   ```

4. Run the application:
   ```bash
   dotnet run --project "Expense Tracker/Expense Tracker.csproj"
   ```

## Screenshots

- Dashboard Overview: *(add screenshot here)*
- AI Monthly Summary Card: *(add screenshot here)*
- AI Warning Indicator Example: *(add screenshot here)*

## Future Enhancements

- Trend comparison vs previous months
- Budget target tracking with AI variance analysis
- Export AI summaries to PDF/email
- Multi-language AI summaries
- User authentication with personalized financial history
