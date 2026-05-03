namespace FinancialPlanner.Application.Requests;

/// <summary>
/// <para>Defines a payload for creating real expenses.</para>
/// </summary>
public sealed class CreateExpenseRequest
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
    /// <para>Gets or sets the date of the expense.</para>
    /// </summary>
    public DateOnly ExpenseDate { get; init; }

    /// <summary>
    /// <para>Gets or sets the spent amount.</para>
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// <para>Gets or sets optional description.</para>
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
    public bool IsPlanned { get; init; } = false;

    /// <summary>
    /// <para>Gets or sets the optional credit account (e.g. credit card) identifier. When set, a credit transaction is created and this expense is not counted as actual cash expense.</para>
    /// </summary>
    public Guid? CreditAccountId { get; init; }

    /// <summary>
    /// <para>When CreditAccountId is set, gets or sets the number of months to spread the payment (default 6).</para>
    /// </summary>
    public int? PaymentMonths { get; init; }
}

