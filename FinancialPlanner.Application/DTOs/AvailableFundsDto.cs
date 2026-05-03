namespace FinancialPlanner.Application.DTOs;

/// <summary>
/// <para>Represents available funds forecast DTO.</para>
/// </summary>
public sealed record AvailableFundsDto
{
    public DateOnly Date { get; init; }
    public decimal AvailableAmount { get; init; }
    public decimal PlannedIncome { get; init; }
    public decimal PlannedExpenses { get; init; }
    public decimal CreditPayments { get; init; }
    public decimal RecurringExpenses { get; init; }
}

