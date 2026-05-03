namespace FinancialPlanner.Application.DTOs;

/// <summary>
/// <para>Represents a credit transaction DTO.</para>
/// </summary>
public sealed record CreditTransactionDto
{
    public Guid Id { get; init; }
    public Guid CreditAccountId { get; init; }
    public string CreditAccountName { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public Guid? SubcategoryId { get; init; }
    public string? SubcategoryName { get; init; }
    public DateOnly TransactionDate { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
    public bool IsIncomeRecorded { get; init; }
    public IReadOnlyCollection<CreditPaymentScheduleDto> PaymentSchedule { get; init; } = Array.Empty<CreditPaymentScheduleDto>();
}

/// <summary>
/// <para>Represents a credit payment schedule DTO.</para>
/// </summary>
public sealed record CreditPaymentScheduleDto
{
    public Guid Id { get; init; }
    public int ScheduledYear { get; init; }
    public int ScheduledMonth { get; init; }
    public decimal PaymentAmount { get; init; }
    public bool IsPaid { get; init; }
    public DateOnly? PaidDate { get; init; }
}

