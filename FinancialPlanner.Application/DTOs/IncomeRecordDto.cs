namespace FinancialPlanner.Application.DTOs;

/// <summary>
/// <para>Represents an income record DTO.</para>
/// </summary>
public sealed record IncomeRecordDto
{
    public Guid Id { get; init; }
    public Guid? IncomeCycleId { get; init; }
    public required string Title { get; init; }
    public decimal Amount { get; init; }
    public DateOnly ReceivedDate { get; init; }
    public bool IsFromCredit { get; init; }
    public string? Notes { get; init; }
    public bool IsPlanned { get; init; }
}

