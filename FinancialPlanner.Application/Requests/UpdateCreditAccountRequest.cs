namespace FinancialPlanner.Application.Requests;

/// <summary>
/// <para>Defines the payload needed to update a credit account.</para>
/// </summary>
public sealed class UpdateCreditAccountRequest
{
    public required string Name { get; init; }
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

