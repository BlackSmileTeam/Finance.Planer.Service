namespace FinancialPlanner.Domain.Entities;

/// <summary>
/// <para>
/// Represents a transaction made using a credit card or loan account.
/// When money is withdrawn, it creates future debt obligations.
/// </para>
/// </summary>
public sealed class CreditTransaction
{
    /// <summary>
    /// <para>Gets or sets the unique identifier of the credit transaction.</para>
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// <para>Gets or sets the identifier of the user who owns this transaction.</para>
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// <para>Gets or sets the identifier of the credit account used.</para>
    /// </summary>
    public Guid CreditAccountId { get; set; }

    /// <summary>
    /// <para>Gets or sets the identifier of the related category.</para>
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// <para>Gets or sets the optional identifier of the related subcategory.</para>
    /// </summary>
    public Guid? SubcategoryId { get; set; }

    /// <summary>
    /// <para>Gets or sets the date when the transaction occurred.</para>
    /// </summary>
    public DateOnly TransactionDate { get; set; }

    /// <summary>
    /// <para>Gets or sets the amount of the transaction.</para>
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// <para>Gets or sets the free form description supplied by the user.</para>
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// <para>Gets or sets a value indicating whether this transaction was recorded as income in the current month.</para>
    /// </summary>
    public bool IsIncomeRecorded { get; set; } = false;

    /// <summary>
    /// <para>Gets or sets the creation timestamp in UTC.</para>
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// <para>Gets or sets the last update timestamp in UTC.</para>
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// <para>Navigation property to the credit account.</para>
    /// </summary>
    public CreditAccount? CreditAccount { get; set; }

    /// <summary>
    /// <para>Navigation property to the related category.</para>
    /// </summary>
    public Category? Category { get; set; }

    /// <summary>
    /// <para>Navigation property to the related subcategory.</para>
    /// </summary>
    public Category? Subcategory { get; set; }

    /// <summary>
    /// <para>Navigation property to the payment schedule.</para>
    /// </summary>
    public ICollection<CreditPaymentSchedule> PaymentSchedule { get; init; } = new HashSet<CreditPaymentSchedule>();

    /// <summary>
    /// <para>Navigation property to the income record if created.</para>
    /// </summary>
    public IncomeRecord? IncomeRecord { get; set; }

    /// <summary>
    /// <para>Navigation property to the user who owns this transaction.</para>
    /// </summary>
    public User? User { get; set; }
}

