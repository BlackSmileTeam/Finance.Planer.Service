namespace FinancialPlanner.Domain.Entities;

/// <summary>
/// <para>
/// Represents a scheduled payment for a credit transaction, split over multiple months
/// (maximum 6 months).
/// </para>
/// </summary>
public sealed class CreditPaymentSchedule
{
    /// <summary>
    /// <para>Gets or sets the unique identifier of the payment schedule entry.</para>
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// <para>Gets or sets the identifier of the credit transaction.</para>
    /// </summary>
    public Guid CreditTransactionId { get; set; }

    /// <summary>
    /// <para>Gets or sets the identifier of the user who owns this payment.</para>
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// <para>Gets or sets the year when the payment is scheduled.</para>
    /// </summary>
    public int ScheduledYear { get; set; }

    /// <summary>
    /// <para>Gets or sets the month when the payment is scheduled.</para>
    /// </summary>
    public int ScheduledMonth { get; set; }

    /// <summary>
    /// <para>Gets or sets the amount to be paid in this scheduled period.</para>
    /// </summary>
    public decimal PaymentAmount { get; set; }

    /// <summary>
    /// <para>Gets or sets a value indicating whether the payment has been made.</para>
    /// </summary>
    public bool IsPaid { get; set; } = false;

    /// <summary>
    /// <para>Gets or sets the date when the payment was made (if paid).</para>
    /// </summary>
    public DateOnly? PaidDate { get; set; }

    /// <summary>
    /// <para>Gets or sets the creation timestamp in UTC.</para>
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// <para>Gets or sets the last update timestamp in UTC.</para>
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// <para>Navigation property to the credit transaction.</para>
    /// </summary>
    public CreditTransaction? CreditTransaction { get; set; }

    /// <summary>
    /// <para>Navigation property to the user who owns this payment.</para>
    /// </summary>
    public User? User { get; set; }
}

