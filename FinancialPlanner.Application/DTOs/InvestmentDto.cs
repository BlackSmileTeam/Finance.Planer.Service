namespace FinancialPlanner.Application.DTOs;

/// <summary>
/// <para>Represents an investment DTO.</para>
/// </summary>
public sealed record InvestmentDto
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public string InvestmentType { get; init; } = "Stock";
    public decimal Amount { get; init; }
    public DateOnly PurchaseDate { get; init; }
    public decimal? CurrentValue { get; init; }
    public string? Notes { get; init; }
}

