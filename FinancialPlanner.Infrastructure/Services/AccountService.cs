using FinancialPlanner.Application.DTOs;
using FinancialPlanner.Application.Interfaces;
using FinancialPlanner.Application.Requests;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace FinancialPlanner.Infrastructure.Services;

/// <summary>
/// <para>Provides account management services.</para>
/// </summary>
public sealed class AccountService : IAccountService
{
    private readonly FinancialPlannerDbContext _context;

    /// <summary>
    /// <para>Initializes a new instance of the account service.</para>
    /// </summary>
    public AccountService(FinancialPlannerDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<AccountDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Accounts
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.Name)
                .Select(a => new AccountDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    AccountNumber = a.AccountNumber,
                    AccountType = a.AccountType,
                    Balance = a.Balance,
                    ExpiryDate = a.ExpiryDate,
                    Color = a.Color,
                    Currency = a.Currency,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync(cancellationToken);
        }
        catch (MySqlException ex) when (ex.Message.Contains("card_holder_name", StringComparison.OrdinalIgnoreCase))
        {
            // Backward-compatible fallback: if runtime model still expects dropped column,
            // query accounts via raw SQL with the actual schema.
            return await _context.Database.SqlQueryRaw<AccountDto>(
                """
                SELECT
                    id AS Id,
                    name AS Name,
                    account_number AS AccountNumber,
                    account_type AS AccountType,
                    balance AS Balance,
                    expiry_date AS ExpiryDate,
                    color AS Color,
                    currency AS Currency,
                    is_active AS IsActive,
                    created_at AS CreatedAt
                FROM accounts
                WHERE user_id = {0}
                ORDER BY name
                """,
                userId)
                .ToListAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task<AccountDto?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .AsNoTracking()
            .Where(a => a.Id == id && a.UserId == userId)
            .Select(a => new AccountDto
            {
                Id = a.Id,
                Name = a.Name,
                AccountNumber = a.AccountNumber,
                AccountType = a.AccountType,
                Balance = a.Balance,
                ExpiryDate = a.ExpiryDate,
                Color = a.Color,
                Currency = a.Currency,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<AccountDto> CreateAsync(CreateAccountRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        // Проверка на дубликат названия счета
        if (await _context.Accounts.AnyAsync(a => a.UserId == userId && a.Name.ToLower() == request.Name.ToLower(), cancellationToken))
        {
            throw new InvalidOperationException("Счет с таким названием уже существует.");
        }

        // Проверка на дубликат номера карты (если указан)
        if (!string.IsNullOrWhiteSpace(request.AccountNumber))
        {
            if (await _context.Accounts.AnyAsync(a => a.UserId == userId && a.AccountNumber != null && a.AccountNumber.ToLower() == request.AccountNumber.ToLower(), cancellationToken))
            {
                throw new InvalidOperationException("Карта с таким номером уже существует.");
            }
        }

        var entity = new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name,
            AccountNumber = request.AccountNumber,
            AccountType = request.AccountType,
            Balance = request.Balance,
            ExpiryDate = request.ExpiryDate,
            Color = request.Color,
            Currency = request.Currency,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Accounts.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new AccountDto
        {
            Id = entity.Id,
            Name = entity.Name,
            AccountNumber = entity.AccountNumber,
            AccountType = entity.AccountType,
            Balance = entity.Balance,
            ExpiryDate = entity.ExpiryDate,
            Color = entity.Color,
            Currency = entity.Currency,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt
        };
    }

    /// <inheritdoc/>
    public async Task<AccountDto> UpdateAsync(Guid id, UpdateAccountRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException("Account was not found.");
        }

        // Проверка на дубликат названия счета (исключая текущий счет)
        if (await _context.Accounts.AnyAsync(a => a.UserId == userId && a.Id != id && a.Name.ToLower() == request.Name.ToLower(), cancellationToken))
        {
            throw new InvalidOperationException("Счет с таким названием уже существует.");
        }

        // Проверка на дубликат номера карты (если указан, исключая текущий счет)
        if (!string.IsNullOrWhiteSpace(request.AccountNumber))
        {
            if (await _context.Accounts.AnyAsync(a => a.UserId == userId && a.Id != id && a.AccountNumber != null && a.AccountNumber.ToLower() == request.AccountNumber.ToLower(), cancellationToken))
            {
                throw new InvalidOperationException("Карта с таким номером уже существует.");
            }
        }

        entity.Name = request.Name;
        entity.AccountNumber = request.AccountNumber;
        entity.AccountType = request.AccountType;
        entity.Balance = request.Balance;
        entity.ExpiryDate = request.ExpiryDate;
        entity.Color = request.Color;
        entity.Currency = request.Currency;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new AccountDto
        {
            Id = entity.Id,
            Name = entity.Name,
            AccountNumber = entity.AccountNumber,
            AccountType = entity.AccountType,
            Balance = entity.Balance,
            ExpiryDate = entity.ExpiryDate,
            Color = entity.Color,
            Currency = entity.Currency,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt
        };
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException("Account was not found.");
        }

        _context.Accounts.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

