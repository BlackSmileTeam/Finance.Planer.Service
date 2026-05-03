namespace FinancialPlanner.Application.DTOs;

/// <summary>
/// <para>Represents an expense enriched with category metadata.</para>
/// </summary>
public sealed record ExpenseDto
{
    /// <summary>
    /// <para>Gets or sets the identifier.</para>
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// <para>Gets or sets the related category identifier.</para>
    /// </summary>
    public Guid CategoryId { get; init; }

    /// <summary>
    /// <para>Gets or sets the category name.</para>
    /// </summary>
    public required string CategoryName { get; init; }

    /// <summary>
    /// <para>Gets or sets the subcategory identifier.</para>
    /// </summary>
    public Guid? SubcategoryId { get; init; }

    /// <summary>
    /// <para>Gets or sets the subcategory name.</para>
    /// </summary>
    public string? SubcategoryName { get; init; }

    /// <summary>
    /// <para>Gets or sets the expense date.</para>
    /// </summary>
    public DateOnly ExpenseDate { get; init; }

    /// <summary>
    /// <para>Gets or sets the amount of the expense.</para>
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// <para>Gets or sets the optional description.</para>
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// <para>Gets or sets the optional planned budget identifier.</para>
    /// </summary>
    public Guid? PlannedBudgetId { get; init; }

    /// <summary>
    /// <para>Gets or sets the account identifier from which this expense is made.</para>
    /// </summary>
    public Guid? AccountId { get; init; }

    /// <summary>
    /// <para>Gets or sets a value indicating whether this expense is planned (not yet actual).</para>
    /// </summary>
    public bool IsPlanned { get; init; }

    /// <summary>
    /// <para>Gets or sets the optional credit payment schedule identifier when this expense was created by confirming a credit payment.</para>
    /// </summary>
    public Guid? CreditPaymentScheduleId { get; init; }

    /// <summary>
    /// <para>Gets or sets the optional credit account identifier when this expense is a confirmed credit payment.</para>
    /// </summary>
    public Guid? CreditAccountId { get; init; }

    /// <summary>
    /// <para>Gets or sets the currency code (e.g., RUB, USD). From account when set; otherwise default RUB.</para>
    /// </summary>
    public string? Currency { get; init; }
}

