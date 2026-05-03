namespace FinancialPlanner.Domain.Entities;

/// <summary>
/// <para>
/// Represents a real expense that happened on a specific day and can be mapped
/// to both a budgeting category and an optional planned budget entry.
/// </para>
/// </summary>
public sealed class Expense
{
    /// <summary>
    /// <para>Gets or sets the unique identifier of the expense.</para>
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// <para>Gets or sets the identifier of the user who owns this expense.</para>
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// <para>Gets or sets the identifier of the account from which this expense is made.</para>
    /// </summary>
    public Guid? AccountId { get; set; }

    /// <summary>
    /// <para>Gets or sets the identifier of the related category.</para>
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// <para>Gets or sets the optional identifier of the related subcategory.</para>
    /// </summary>
    public Guid? SubcategoryId { get; set; }

    /// <summary>
    /// <para>Gets or sets the calendar date when the spending happened.</para>
    /// </summary>
    public DateOnly ExpenseDate { get; set; }

    /// <summary>
    /// <para>Gets or sets the amount of money spent.</para>
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// <para>Gets or sets the free form description supplied by the user.</para>
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// <para>Gets or sets the optional identifier of the planned budget entry.</para>
    /// </summary>
    public Guid? PlannedBudgetId { get; set; }

    /// <summary>
    /// <para>Gets or sets the optional identifier of the credit payment schedule when this expense was created by confirming a credit payment.</para>
    /// </summary>
    public Guid? CreditPaymentScheduleId { get; set; }

    /// <summary>
    /// <para>Gets or sets the optional identifier of the credit account when this expense was created by confirming a loan payment.</para>
    /// </summary>
    public Guid? CreditAccountId { get; set; }

    /// <summary>
    /// <para>Gets or sets a value indicating whether this expense is planned (not yet actual).</para>
    /// </summary>
    public bool IsPlanned { get; set; } = false;

    /// <summary>
    /// <para>Gets or sets the creation timestamp in UTC.</para>
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// <para>Gets or sets the last update timestamp in UTC.</para>
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// <para>Navigation property to the related category.</para>
    /// </summary>
    public Category? Category { get; set; }

    /// <summary>
    /// <para>Navigation property to the related subcategory.</para>
    /// </summary>
    public Category? Subcategory { get; set; }

    /// <summary>
    /// <para>Navigation property to the linked planned budget.</para>
    /// </summary>
    public PlannedBudget? PlannedBudget { get; set; }

    /// <summary>
    /// <para>Navigation property to the user who owns this expense.</para>
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// <para>Navigation property to the account from which this expense is made.</para>
    /// </summary>
    public Account? Account { get; set; }

    /// <summary>
    /// <para>Navigation property to the credit payment schedule when this expense was created by confirming a credit payment.</para>
    /// </summary>
    public CreditPaymentSchedule? CreditPaymentSchedule { get; set; }

    /// <summary>
    /// <para>Navigation property to the credit account when this expense was created by confirming a loan payment.</para>
    /// </summary>
    public CreditAccount? CreditAccount { get; set; }
}

