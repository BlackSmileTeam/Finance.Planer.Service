using FinancialPlanner.Application.Interfaces;
using FinancialPlanner.Application.Requests;
using FinancialPlanner.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.Api.Controllers;

/// <summary>
/// <para>Provides CRUD endpoints for expenses.</para>
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;

    /// <summary>
    /// <para>Initializes the controller.</para>
    /// </summary>
    public ExpensesController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    /// <summary>
    /// <para>Returns expenses for the provided month.</para>
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByMonthAsync([FromQuery] int? year, [FromQuery] int? month, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var targetYear = year ?? DateTime.UtcNow.Year;
        var targetMonth = month ?? DateTime.UtcNow.Month;
        var expenses = await _expenseService.GetByMonthAsync(targetYear, targetMonth, userId.Value, cancellationToken);
        return Ok(expenses);
    }

    /// <summary>
    /// <para>Returns a single expense by id.</para>
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var expense = await _expenseService.GetByIdAsync(id, userId.Value, cancellationToken);
        if (expense == null)
            return NotFound();
        return Ok(expense);
    }

    /// <summary>
    /// <para>Creates a new expense entry.</para>
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateExpenseRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var expense = await _expenseService.CreateAsync(request, userId.Value, cancellationToken);
        // CreatedAtAction может выбрасывать "No route matches the supplied values" при генерации Location.
        return StatusCode(StatusCodes.Status201Created, expense);
    }

    /// <summary>
    /// <para>Updates an expense.</para>
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateExpenseRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var expense = await _expenseService.UpdateAsync(id, request, userId.Value, cancellationToken);
        return Ok(expense);
    }

    /// <summary>
    /// <para>Deletes an expense.</para>
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        await _expenseService.DeleteAsync(id, userId.Value, cancellationToken);
        return NoContent();
    }

    [HttpGet("pending-planned")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingPlannedExpensesAsync(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var pendingExpenses = await _expenseService.GetPendingPlannedExpensesAsync(userId.Value, cancellationToken);
        return Ok(pendingExpenses);
    }

    [HttpPost("confirm-planned-recurring/{recurringExpenseId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmPlannedRecurringExpenseAsync(
        [FromRoute] Guid recurringExpenseId, 
        [FromQuery(Name = "expenseDate")] string expenseDate,
        [FromBody] ConfirmPlannedTransactionRequest? request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(expenseDate))
        {
            return BadRequest("expenseDate query parameter is required");
        }

        if (!DateOnly.TryParse(expenseDate, out var expenseDateParsed))
        {
            return BadRequest("Invalid expenseDate format. Expected format: YYYY-MM-DD");
        }

        try
        {
            await _expenseService.ConfirmPlannedRecurringExpenseAsync(recurringExpenseId, expenseDateParsed, userId.Value, request?.Amount, cancellationToken);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("confirm-planned/{expenseId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmPlannedExpenseAsync(Guid expenseId, [FromBody] ConfirmPlannedTransactionRequest? request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        try
        {
            await _expenseService.ConfirmPlannedExpenseAsync(expenseId, userId.Value, request?.Amount, cancellationToken);
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}

