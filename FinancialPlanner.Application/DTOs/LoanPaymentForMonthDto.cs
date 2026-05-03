namespace FinancialPlanner.Application.DTOs;

/// <summary>
/// <para>Плановый платёж по кредиту (Loan) за месяц — для отображения в списке расходов.</para>
/// </summary>
public sealed record LoanPaymentForMonthDto
{
    public Guid CreditAccountId { get; init; }
    public string CreditAccountName { get; init; } = string.Empty;
    public int ScheduledYear { get; init; }
    public int ScheduledMonth { get; init; }
    /// <summary>Day of month when the payment is due (from PaymentStartDate).</summary>
    public int ScheduledDay { get; init; }
    public decimal PaymentAmount { get; init; }
}
