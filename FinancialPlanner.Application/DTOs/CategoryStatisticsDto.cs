namespace FinancialPlanner.Application.DTOs;

/// <summary>
/// <para>Represents statistics for a category.</para>
/// </summary>
public sealed class CategoryStatisticsDto
{
    /// <summary>
    /// <para>Gets or sets the category identifier.</para>
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// <para>Gets or sets the category name.</para>
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// <para>Gets or sets the category color.</para>
    /// </summary>
    public string CategoryColor { get; set; } = "#3B82F6";

    /// <summary>
    /// <para>Gets or sets the total amount spent in this category.</para>
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// <para>Gets or sets the count of expenses in this category.</para>
    /// </summary>
    public int ExpenseCount { get; set; }

    /// <summary>
    /// <para>Gets or sets the average expense amount.</para>
    /// </summary>
    public decimal AverageAmount { get; set; }

    /// <summary>
    /// <para>Gets or sets the percentage of total expenses.</para>
    /// </summary>
    public decimal PercentageOfTotal { get; set; }

    /// <summary>
    /// <para>Gets or sets the planned budget amount for this category (if any).</para>
    /// </summary>
    public decimal PlannedAmount { get; set; }

    /// <summary>
    /// <para>Gets or sets the difference between planned and actual.</para>
    /// </summary>
    public decimal DifferenceFromPlan { get; set; }
}

/// <summary>
/// <para>Represents category statistics response.</para>
/// </summary>
public sealed class CategoryStatisticsResponseDto
{
    /// <summary>
    /// <para>Gets or sets the start date of the period.</para>
    /// </summary>
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// <para>Gets or sets the end date of the period.</para>
    /// </summary>
    public DateOnly EndDate { get; set; }

    /// <summary>
    /// <para>Gets or sets the total expenses across all categories.</para>
    /// </summary>
    public decimal TotalExpenses { get; set; }

    /// <summary>
    /// <para>Gets or sets the category statistics.</para>
    /// </summary>
    public List<CategoryStatisticsDto> Categories { get; set; } = new();
}

