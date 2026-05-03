namespace FinancialPlanner.Application.DTOs;

/// <summary>
/// <para>Represents a credit account DTO.</para>
/// </summary>
public sealed record CreditAccountDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string AccountType { get; init; } = "CreditCard";
    public decimal? CreditLimit { get; init; }
    public decimal? MonthlyPayment { get; init; }
    public decimal? TotalAmount { get; init; }
    public int? TermMonths { get; init; }
    public DateOnly? PaymentStartDate { get; init; }
    public decimal CurrentBalance { get; init; }
    public decimal? InterestRate { get; init; }
    public bool IsActive { get; init; }
    public string? Notes { get; init; }
}

