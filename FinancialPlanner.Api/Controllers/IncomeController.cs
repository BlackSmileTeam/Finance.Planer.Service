using FinancialPlanner.Application.Interfaces;
using FinancialPlanner.Application.Requests;
using FinancialPlanner.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.Api.Controllers;

/// <summary>
/// <para>Provides CRUD endpoints for income cycles.</para>
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class IncomeController : ControllerBase
{
    private readonly IIncomeService _incomeService;

    /// <summary>
    /// <para>Initializes the controller.</para>
    /// </summary>
    public IncomeController(IIncomeService incomeService)
    {
        _incomeService = incomeService;
    }

    /// <summary>
    /// <para>Returns incomes for a year.</para>
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByYearAsync([FromQuery] int? year, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var targetYear = year ?? DateTime.UtcNow.Year;
        var incomes = await _incomeService.GetByYearAsync(targetYear, userId.Value, cancellationToken);
        return Ok(incomes);
    }

    /// <summary>
    /// <para>Creates an income cycle.</para>
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateIncomeRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var income = await _incomeService.CreateAsync(request, userId.Value, cancellationToken);
        return Created($"/api/income?year={income.ReceivedDate.Year}", income);
    }

    [HttpGet("pending-planned")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingPlannedIncomeAsync(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var pendingIncome = await _incomeService.GetPendingPlannedIncomeAsync(userId.Value, cancellationToken);
        return Ok(pendingIncome);
    }

    [HttpPost("confirm-planned/{incomeId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmPlannedIncomeAsync(Guid incomeId, [FromBody] ConfirmPlannedTransactionRequest? request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        try
        {
            await _incomeService.ConfirmPlannedIncomeAsync(incomeId, userId.Value, request?.Amount, cancellationToken);
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// <para>Updates an income cycle.</para>
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateIncomeRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var income = await _incomeService.UpdateAsync(id, request, userId.Value, cancellationToken);
        return Ok(income);
    }

    /// <summary>
    /// <para>Deletes an income cycle.</para>
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        await _incomeService.DeleteAsync(id, userId.Value, cancellationToken);
        return NoContent();
    }
}

