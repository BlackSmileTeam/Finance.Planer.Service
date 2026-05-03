using FinancialPlanner.Application.DTOs;

namespace FinancialPlanner.Application.Interfaces;

/// <summary>
/// <para>Provides reporting data for UI layers.</para>
/// </summary>
public interface IMonthlySummaryService
{
    /// <summary>
    /// <para>Returns summaries for a specific year. Optional startDay/endDay filter data by day of month (e.g. 1–15 or 16–31).</para>
    /// </summary>
    Task<IReadOnlyCollection<MonthlySummaryDto>> GetSummariesAsync(int year, Guid userId, int? startDay = null, int? endDay = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// <para>Returns a forecast horizon starting from the specified month for the specified number of months.</para>
    /// </summary>
    Task<ForecastHorizonDto> GetForecastHorizonAsync(int startYear, int startMonth, int monthsCount, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// <para>Returns category statistics for the specified period.</para>
    /// </summary>
    Task<CategoryStatisticsResponseDto> GetCategoryStatisticsAsync(DateOnly startDate, DateOnly endDate, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// <para>Returns available funds forecast for the specified period.</para>
    /// </summary>
    Task<IReadOnlyCollection<AvailableFundsDto>> GetAvailableFundsForecastAsync(DateOnly startDate, DateOnly endDate, Guid userId, CancellationToken cancellationToken);
}

