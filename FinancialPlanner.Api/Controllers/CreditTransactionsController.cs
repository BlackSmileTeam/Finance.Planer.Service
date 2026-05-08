using FinancialPlanner.Application.Interfaces;
using FinancialPlanner.Application.Requests;
using FinancialPlanner.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.Api.Controllers;

/// <summary>
/// <para>Provides CRUD endpoints for credit transactions.</para>
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CreditTransactionsController : ControllerBase
{
    private readonly ICreditTransactionService _service;

    public CreditTransactionsController(ICreditTransactionService service)
    {
        _service = service;
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
        var item = await _service.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateCreditTransactionRequest request, CancellationToken cancellationToken)
    {
        var item = await _service.CreateAsync(request, cancellationToken);
        // CreatedAtAction иногда выбрасывает InvalidOperationException
        // \"No route matches the supplied values.\" из-за особенностей маршрутизации.
        // Нам достаточно вернуть созданный объект без ссылки.
        return StatusCode(StatusCodes.Status201Created, item);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/record-income")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RecordAsIncomeAsync(Guid id, CancellationToken cancellationToken)
    {
        await _service.RecordAsIncomeAsync(id, cancellationToken);
        return Ok();
    }

    [HttpGet("pending-payments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingPaymentsAsync(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var pendingPayments = await _service.GetPendingPaymentsAsync(userId.Value, cancellationToken);
        return Ok(pendingPayments);
    }

    [HttpGet("payments-for-month")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCreditPaymentsForMonthAsync([FromQuery] int year, [FromQuery] int month, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var payments = await _service.GetCreditPaymentsForMonthAsync(year, month, userId.Value, cancellationToken);
        return Ok(payments);
    }

    [HttpPost("confirm-payment/{paymentScheduleId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmPaymentAsync(Guid paymentScheduleId, [FromQuery] decimal? amount, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        try
        {
            await _service.ConfirmPaymentAsync(paymentScheduleId, userId.Value, amount, cancellationToken);
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}

