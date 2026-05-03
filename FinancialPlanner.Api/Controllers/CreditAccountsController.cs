using FinancialPlanner.Application.Interfaces;
using FinancialPlanner.Application.Requests;
using FinancialPlanner.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.Api.Controllers;

/// <summary>
/// <para>Provides CRUD endpoints for credit accounts.</para>
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CreditAccountsController : ControllerBase
{
    private readonly ICreditAccountService _service;

    public CreditAccountsController(ICreditAccountService service)
    {
        _service = service;
    }

    [HttpGet("loan-payments-for-month")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLoanPaymentsForMonthAsync([FromQuery] int year, [FromQuery] int month, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var payments = await _service.GetLoanPaymentsForMonthAsync(year, month, userId.Value, cancellationToken);
        return Ok(payments);
    }

    [HttpPost("{id:guid}/confirm-loan-payment")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmLoanPaymentAsync(Guid id, [FromQuery] int year, [FromQuery] int month, [FromQuery] int day = 1, [FromQuery] decimal? amount = null, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        try
        {
            await _service.ConfirmLoanPaymentAsync(id, year, month, day, amount, userId.Value, cancellationToken);
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var items = await _service.GetAllAsync(userId.Value, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var item = await _service.GetByIdAsync(id, userId.Value, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateCreditAccountRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var item = await _service.CreateAsync(request, userId.Value, cancellationToken);
        return Created($"api/creditaccounts/{item.Id}", item);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateCreditAccountRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var item = await _service.UpdateAsync(id, request, userId.Value, cancellationToken);
        return Ok(item);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        await _service.DeleteAsync(id, userId.Value, cancellationToken);
        return NoContent();
    }
}

