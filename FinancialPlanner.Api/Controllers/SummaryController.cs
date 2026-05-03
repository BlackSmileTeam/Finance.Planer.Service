using FinancialPlanner.Api.Extensions;
using FinancialPlanner.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FinancialPlanner.Api.Controllers;

/// <summary>
/// <para>Provides read-only endpoints for monthly summaries.</para>
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class SummaryController : ControllerBase
{
    private readonly IMonthlySummaryService _summaryService;

    /// <summary>
    /// <para>Initializes the controller.</para>
    /// </summary>
    public SummaryController(IMonthlySummaryService summaryService)
    {
        _summaryService = summaryService;
    }

    /// <summary>
    /// <para>Returns monthly summaries for the requested year. Optional startDay/endDay filter by day of month (e.g. 1–15 or 16–31).</para>
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAsync([FromQuery] int? year, [FromQuery] int? startDay, [FromQuery] int? endDay, CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var targetYear = year ?? DateTime.UtcNow.Year;
            var summaries = await _summaryService.GetSummariesAsync(targetYear, userId.Value, startDay, endDay, cancellationToken);
            return Ok(summaries);
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Error in GetAsync: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            return BadRequest(new { message = ex.Message, details = ex.InnerException?.Message });
        }
    }

    /// <summary>
    /// <para>Returns a forecast horizon starting from the specified month for the specified number of months (minimum 3).</para>
    /// </summary>
    [HttpGet("forecast")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetForecastHorizonAsync(
        [FromQuery] int? startYear,
        [FromQuery] int? startMonth,
        [FromQuery] int? monthsCount,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var now = DateTime.UtcNow;
            var targetStartYear = startYear ?? now.Year;
            var targetStartMonth = startMonth ?? now.Month;
            var targetMonthsCount = monthsCount ?? 3;
            
            if (targetMonthsCount < 3)
                targetMonthsCount = 3;

            var forecast = await _summaryService.GetForecastHorizonAsync(
                targetStartYear,
                targetStartMonth,
                targetMonthsCount,
                userId.Value,
                cancellationToken);

            return Ok(forecast);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// <para>Returns category statistics for the specified period.</para>
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategoryStatisticsAsync(
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery] int? monthsBack,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var now = DateTime.UtcNow;
            var targetYear = year ?? now.Year;
            var targetMonth = month ?? now.Month;
            var monthsToLookBack = monthsBack ?? 3;

            var startDate = new DateOnly(targetYear, targetMonth, 1).AddMonths(-monthsToLookBack + 1);
            var endDate = new DateOnly(targetYear, targetMonth, DateTime.DaysInMonth(targetYear, targetMonth));

            var statistics = await _summaryService.GetCategoryStatisticsAsync(
                startDate,
                endDate,
                userId.Value,
                cancellationToken);

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// <para>Returns available funds forecast for the specified period.</para>
    /// </summary>
    [HttpGet("available-funds")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableFundsForecastAsync(
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var now = DateOnly.FromDateTime(DateTime.UtcNow);
            var targetStartDate = startDate ?? now;
            var targetEndDate = endDate ?? now.AddMonths(3);

            var forecast = await _summaryService.GetAvailableFundsForecastAsync(
                targetStartDate,
                targetEndDate,
                userId.Value,
                cancellationToken);

            return Ok(forecast);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return BadRequest(ex.Message);
        }
    }
}

