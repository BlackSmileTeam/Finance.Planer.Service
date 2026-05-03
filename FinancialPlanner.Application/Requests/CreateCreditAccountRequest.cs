namespace FinancialPlanner.Application.Requests;

/// <summary>
/// <para>Defines the payload needed to create a credit account.</para>
/// </summary>
public sealed class CreateCreditAccountRequest
{
    public required string Name { get; init; }
    public string AccountType { get; init; } = "CreditCard";
    public decimal? CreditLimit { get; init; }
    public decimal? MonthlyPayment { get; init; }
    public decimal? TotalAmount { get; init; }
    public int? TermMonths { get; init; }
    public DateOnly? PaymentStartDate { get; init; }
    /// <summary>Optional initial/current balance (e.g. to fix discrepancies and start calculations from now).</summary>
    public decimal? CurrentBalance { get; init; }
    public decimal? InterestRate { get; init; }
    public string? Notes { get; init; }
}

