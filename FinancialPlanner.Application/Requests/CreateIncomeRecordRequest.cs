namespace FinancialPlanner.Application.Requests;

/// <summary>
/// <para>Defines the payload needed to create an income record.</para>
/// </summary>
public sealed class CreateIncomeRecordRequest
{
    public Guid? IncomeCycleId { get; init; }
    public required string Title { get; init; }
    public decimal Amount { get; init; }
    public DateOnly ReceivedDate { get; init; }
    public string? Notes { get; init; }
    public bool IsPlanned { get; init; } = false;
}

