namespace FinancialPlanner.Application.Requests;

/// <summary>
/// <para>Defines payload to update an expense.</para>
/// </summary>
public sealed class UpdateExpenseRequest
{
    /// <summary>
    /// <para>Gets or sets the category identifier.</para>
    /// </summary>
    public Guid CategoryId { get; init; }

    /// <summary>
    /// <para>Gets or sets the optional subcategory identifier.</para>
    /// </summary>
    public Guid? SubcategoryId { get; init; }

    /// <summary>
    /// <para>Gets or sets the date.</para>
    /// </summary>
    public DateOnly ExpenseDate { get; init; }

    /// <summary>
    /// <para>Gets or sets the amount.</para>
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// <para>Gets or sets optional description.</para>
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// <para>Gets or sets optional planned budget identifier.</para>
    /// </summary>
    public Guid? PlannedBudgetId { get; init; }

    /// <summary>
    /// <para>Gets or sets the account identifier from which this expense is made.</para>
    /// </summary>
    public Guid? AccountId { get; init; }

    /// <summary>
    /// <para>Gets or sets the optional credit account (e.g. credit card) identifier. When set, expense is paid by card; account balance is not changed.</para>
    /// </summary>
    public Guid? CreditAccountId { get; init; }

    /// <summary>
    /// <para>Gets or sets the currency code (e.g., RUB, USD).</para>
    /// </summary>
    public string? Currency { get; init; }

    /// <summary>
    /// <para>Gets or sets a value indicating whether this expense is planned (not yet actual).</para>
    /// </summary>
    public bool IsPlanned { get; init; } = false;
}

