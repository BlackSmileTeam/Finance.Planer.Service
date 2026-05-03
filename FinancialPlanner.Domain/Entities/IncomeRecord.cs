namespace FinancialPlanner.Domain.Entities;

/// <summary>
/// <para>
/// Represents an actual income record (separate from income cycles which are templates).
/// Can be linked to an income cycle or a credit transaction.
/// </para>
/// </summary>
public sealed class IncomeRecord
{
    /// <summary>
    /// <para>Gets or sets the unique identifier of the income record.</para>
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// <para>Gets or sets the identifier of the user who owns this income record.</para>
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// <para>Gets or sets the optional identifier of the related income cycle.</para>
    /// </summary>
    public Guid? IncomeCycleId { get; set; }

    /// <summary>
    /// <para>Gets or sets the friendly name of the income source.</para>
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// <para>Gets or sets the amount received.</para>
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// <para>Gets or sets the date when the income was received.</para>
    /// </summary>
    public DateOnly ReceivedDate { get; set; }

    /// <summary>
    /// <para>Gets or sets a value indicating whether this income came from a credit transaction.</para>
    /// </summary>
    public bool IsFromCredit { get; set; } = false;

    /// <summary>
    /// <para>Gets or sets the optional identifier of the related credit transaction.</para>
    /// </summary>
    public Guid? CreditTransactionId { get; set; }

    /// <summary>
    /// <para>Gets or sets a value indicating whether this income record is planned (not yet actual).</para>
    /// </summary>
    public bool IsPlanned { get; set; } = false;

    /// <summary>
    /// <para>Gets or sets the date when this planned income record was first confirmed (converted to actual income).</para>
    /// <para>Once set, this record should not appear in pending planned income list even if the actual record is deleted.</para>
    /// </summary>
    public DateOnly? FirstConfirmedDate { get; set; }

    /// <summary>
    /// <para>Gets or sets additional notes provided by the user.</para>
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// <para>Gets or sets the creation timestamp in UTC.</para>
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// <para>Gets or sets the last update timestamp in UTC.</para>
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// <para>Navigation property to the related income cycle.</para>
    /// </summary>
    public IncomeCycle? IncomeCycle { get; set; }

    /// <summary>
    /// <para>Navigation property to the related credit transaction.</para>
    /// </summary>
    public CreditTransaction? CreditTransaction { get; set; }

    /// <summary>
    /// <para>Navigation property to the user who owns this income record.</para>
    /// </summary>
    public User? User { get; set; }
}

