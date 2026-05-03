namespace FinancialPlanner.Application.Requests;

/// <summary>
/// <para>Defines the payload needed to update an investment.</para>
/// </summary>
public sealed class UpdateInvestmentRequest
{
    public required string Title { get; init; }
    public string InvestmentType { get; init; } = "Stock";
    public decimal Amount { get; init; }
    public DateOnly PurchaseDate { get; init; }
    public decimal? CurrentValue { get; init; }
    public string? Notes { get; init; }
}

