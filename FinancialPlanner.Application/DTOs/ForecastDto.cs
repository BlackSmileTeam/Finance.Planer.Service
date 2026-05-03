namespace FinancialPlanner.Application.DTOs;

/// <summary>
/// <para>Represents a monthly forecast for financial planning.</para>
/// </summary>
public sealed class ForecastDto
{
    /// <summary>
    /// <para>Gets or sets the year.</para>
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// <para>Gets or sets the month (1-12).</para>
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// <para>Gets or sets the planned income.</para>
    /// </summary>
    public decimal PlannedIncome { get; set; }

    /// <summary>
    /// <para>Gets or sets the planned expense.</para>
    /// </summary>
    public decimal PlannedExpense { get; set; }

    /// <summary>
    /// <para>Gets or sets the carry over from previous month.</para>
    /// </summary>
    public decimal CarryOver { get; set; }

    /// <summary>
    /// <para>Gets or sets the actual income (if available).</para>
    /// </summary>
    public decimal ActualIncome { get; set; }

    /// <summary>
    /// <para>Gets or sets the actual expense (if available).</para>
    /// </summary>
    public decimal ActualExpense { get; set; }

    /// <summary>
    /// <para>Gets or sets the closing balance.</para>
    /// </summary>
    public decimal ClosingBalance { get; set; }

    /// <summary>
    /// <para>Gets or sets the projected balance (planned).</para>
    /// </summary>
    public decimal ProjectedBalance { get; set; }

    /// <summary>
    /// <para>Gets or sets a value indicating whether this month has issues (negative balance or high expenses).</para>
    /// </summary>
    public bool IsProblematic { get; set; }

    /// <summary>
    /// <para>Gets or sets the alert level: 0 = OK, 1 = Warning, 2 = Critical.</para>
    /// </summary>
    public int AlertLevel { get; set; }

    /// <summary>
    /// <para>Gets or sets the alert color for UI.</para>
    /// </summary>
    public string AlertColor { get; set; } = "#10B981"; // Green by default

    /// <summary>
    /// <para>Gets or sets the free funds available (projected balance).</para>
    /// </summary>
    public decimal FreeFunds { get; set; }
}

/// <summary>
/// <para>Represents a forecast horizon response.</para>
/// </summary>
public sealed class ForecastHorizonDto
{
    /// <summary>
    /// <para>Gets or sets the start month of the forecast.</para>
    /// </summary>
    public int StartMonth { get; set; }

    /// <summary>
    /// <para>Gets or sets the start year of the forecast.</para>
    /// </summary>
    public int StartYear { get; set; }

    /// <summary>
    /// <para>Gets or sets the forecast months.</para>
    /// </summary>
    public List<ForecastDto> Forecasts { get; set; } = new();

    /// <summary>
    /// <para>Gets or sets the total problematic months count.</para>
    /// </summary>
    public int ProblematicMonthsCount { get; set; }

    /// <summary>
    /// <para>Gets or sets the minimum free funds across all months.</para>
    /// </summary>
    public decimal MinimumFreeFunds { get; set; }

    /// <summary>
    /// <para>Gets or sets the average free funds.</para>
    /// </summary>
    public decimal AverageFreeFunds { get; set; }
}

