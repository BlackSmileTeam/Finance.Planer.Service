using FinancialPlanner.Api.Extensions;
using FinancialPlanner.Application.Interfaces;
using FinancialPlanner.Application.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.Api.Controllers;

/// <summary>
/// <para>Manages monthly plans and planned budgets.</para>
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class MonthlyPlansController : ControllerBase
{
    private readonly IMonthlyPlanService _planService;

    /// <summary>
    /// <para>Initializes the controller.</para>
    /// </summary>
    public MonthlyPlansController(IMonthlyPlanService planService)
    {
        _planService = planService;
    }

    /// <summary>
    /// <para>Returns the plan for the specified month.</para>
    /// </summary>
    [HttpGet("{year:int}/{month:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAsync(int year, int month, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var plan = await _planService.GetAsync(year, month, userId.Value, cancellationToken);
        // Return 200 OK with null instead of 404 to avoid console errors on frontend
        return Ok(plan);
    }

    /// <summary>
    /// <para>Returns all plans for a year.</para>
    /// </summary>
    [HttpGet("year/{year:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByYearAsync(int year, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var plans = await _planService.GetByYearAsync(year, userId.Value, cancellationToken);
        return Ok(plans);
    }

    /// <summary>
    /// <para>Creates a new monthly plan.</para>
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateMonthlyPlanRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var plan = await _planService.CreateAsync(request, userId.Value, cancellationToken);
        return CreatedAtAction(nameof(GetAsync), new { year = plan.PlanYear, month = plan.PlanMonth }, plan);
    }

    /// <summary>
    /// <para>Updates an existing plan.</para>
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateMonthlyPlanRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var plan = await _planService.UpdateAsync(id, request, userId.Value, cancellationToken);
        return Ok(plan);
    }

    /// <summary>
    /// <para>Deletes a plan.</para>
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        await _planService.DeleteAsync(id, userId.Value, cancellationToken);
        return NoContent();
    }
}

