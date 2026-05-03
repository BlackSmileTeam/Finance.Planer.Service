namespace FinancialPlanner.Domain.Enumerations;

/// <summary>
/// <para>
/// Represents how frequently a particular income cycle recurs, enabling
/// downstream services to forecast monthly cash flow with higher precision.
/// </para>
/// </summary>
public enum IncomeFrequency
{
    /// <summary>
    /// <para>Income that occurs once a week.</para>
    /// </summary>
    Weekly = 0,

    /// <summary>
    /// <para>Income that arrives every two weeks.</para>
    /// </summary>
    BiWeekly = 1,

    /// <summary>
    /// <para>Income scheduled for delivery each calendar month.</para>
    /// </summary>
    Monthly = 2,

    /// <summary>
    /// <para>Income received once per quarter.</para>
    /// </summary>
    Quarterly = 3,

    /// <summary>
    /// <para>Income that is paid once per year.</para>
    /// </summary>
    Yearly = 4
}

