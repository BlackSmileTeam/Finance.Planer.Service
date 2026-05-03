namespace FinancialPlanner.Application.DTOs;

/// <summary>
/// <para>Represents a pending credit payment that needs to be confirmed.</para>
/// </summary>
public sealed record PendingCreditPaymentDto
{
    public Guid PaymentScheduleId { get; init; }
    public Guid CreditTransactionId { get; init; }
    public string CreditAccountName { get; init; } = string.Empty;
    /// <summary>CreditCard or Loan — для различения «Платеж по кредитной карте» и «Платеж по кредиту».</summary>
    public string CreditAccountType { get; init; } = "CreditCard";
    public int ScheduledYear { get; init; }
    public int ScheduledMonth { get; init; }
    /// <summary>Day of month when the payment is due (from transaction date for credit cards).</summary>
    public int ScheduledDay { get; init; }
    public decimal PaymentAmount { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public Guid? SubcategoryId { get; init; }
    public string? SubcategoryName { get; init; }
}
