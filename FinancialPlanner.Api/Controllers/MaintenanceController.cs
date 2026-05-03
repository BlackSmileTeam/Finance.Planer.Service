using FinancialPlanner.Api.Extensions;
using FinancialPlanner.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.Api.Controllers;

/// <summary>
/// <para>Provides maintenance and data reset operations for the current user.</para>
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class MaintenanceController : ControllerBase
{
    private readonly IDataResetService _dataResetService;

    public MaintenanceController(IDataResetService dataResetService)
    {
        _dataResetService = dataResetService;
    }

    /// <summary>
    /// <para>
    /// Resets actual income and expense data for the current user:
    /// - deletes all non-planned income and expense history;
    /// - keeps planned and recurring items, shifting their dates so they are not earlier than today.
    /// </para>
    /// </summary>
    [HttpPost("reset-actual-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResetActualDataAsync(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        await _dataResetService.ResetActualDataAsync(userId.Value, cancellationToken);
        return NoContent();
    }
}

