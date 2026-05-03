using FinancialPlanner.Domain.Enumerations;

namespace FinancialPlanner.Domain.Entities;

/// <summary>
/// <para>
/// Represents a recurring expense that repeats at a specified interval
/// (weekly, bi-weekly, monthly, quarterly, yearly).
/// </para>
/// </summary>
public sealed class RecurringExpense
{
    /// <summary>
    /// <para>Gets or sets the unique identifier of the recurring expense.</para>
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// <para>Gets or sets the identifier of the user who owns this recurring expense.</para>
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// <para>Gets or sets the identifier of the related category.</para>
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// <para>Gets or sets the optional identifier of the related subcategory.</para>
    /// </summary>
    public Guid? SubcategoryId { get; set; }

    /// <summary>
    /// <para>Gets or sets the friendly name of the recurring expense.</para>
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// <para>Gets or sets the amount of the recurring expense.</para>
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// <para>Gets or sets the start date when the recurring expense begins.</para>
    /// </summary>
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// <para>Gets or sets the optional end date when the recurring expense stops.</para>
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// <para>Gets or sets how often the expense recurs.</para>
    /// </summary>
    public IncomeFrequency Frequency { get; set; } = IncomeFrequency.Monthly;

    /// <summary>
    /// <para>Gets or sets a value indicating whether the recurring expense is active.</para>
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// <para>Gets or sets a value indicating whether this recurring expense is planned (not yet actual).</para>
    /// </summary>
    public bool IsPlanned { get; set; } = false;

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
    /// <para>Navigation property to the related category.</para>
    /// </summary>
    public Category? Category { get; set; }

    /// <summary>
    /// <para>Navigation property to the related subcategory.</para>
    /// </summary>
    public Category? Subcategory { get; set; }

    /// <summary>
    /// <para>Navigation property to the user who owns this recurring expense.</para>
    /// </summary>
    public User? User { get; set; }
}

