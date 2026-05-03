using FinancialPlanner.Application.Interfaces;
using FinancialPlanner.Application.Requests;
using FinancialPlanner.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.Api.Controllers;

/// <summary>
/// <para>Provides CRUD endpoints for accounts.</para>
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    /// <summary>
    /// <para>Initializes the controller.</para>
    /// </summary>
    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    /// <summary>
    /// <para>Returns all accounts for the current user.</para>
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var accounts = await _accountService.GetAllAsync(userId.Value, cancellationToken);
        return Ok(accounts);
    }

    /// <summary>
    /// <para>Returns an account by identifier.</para>
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var account = await _accountService.GetByIdAsync(id, userId.Value, cancellationToken);
        return account is null ? NotFound() : Ok(account);
    }

    /// <summary>
    /// <para>Creates a new account.</para>
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateAccountRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var account = await _accountService.CreateAsync(request, userId.Value, cancellationToken);
        return Created($"api/accounts/{account.Id}", account);
    }

    /// <summary>
    /// <para>Updates an account.</para>
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateAccountRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var account = await _accountService.UpdateAsync(id, request, userId.Value, cancellationToken);
        return Ok(account);
    }

    /// <summary>
    /// <para>Deletes an account.</para>
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        await _accountService.DeleteAsync(id, userId.Value, cancellationToken);
        return NoContent();
    }
}

