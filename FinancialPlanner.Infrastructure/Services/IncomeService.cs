using FinancialPlanner.Application.DTOs;
using FinancialPlanner.Application.Interfaces;
using FinancialPlanner.Application.Requests;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Enumerations;
using FinancialPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq;

namespace FinancialPlanner.Infrastructure.Services;

/// <summary>
/// <para>EF Core implementation of <see cref="IIncomeService"/>.</para>
/// </summary>
public sealed class IncomeService : IIncomeService
{
    private readonly FinancialPlannerDbContext _context;

    /// <summary>
    /// <para>Initializes the service.</para>
    /// </summary>
    public IncomeService(FinancialPlannerDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<IncomeCycleDto>> GetByYearAsync(int year, Guid userId, CancellationToken cancellationToken)
    {
        var start = new DateOnly(year, 1, 1);
        var end = new DateOnly(year, 12, 31);

        return await _context.IncomeCycles
            .AsNoTracking()
            .Where(i => i.UserId == userId && i.StartDate <= end && (i.EndDate == null || i.EndDate >= start))
            .OrderBy(i => i.StartDate)
            .Select(i => new IncomeCycleDto
            {
                Id = i.Id,
                Title = i.Title,
                ReceivedDate = i.ReceivedDate,
                StartDate = i.StartDate,
                EndDate = i.EndDate,
                Amount = i.Amount,
                Frequency = i.Frequency,
                Notes = i.Notes,
                AccountId = i.AccountId,
                IsPlanned = i.IsPlanned
            })
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IncomeCycleDto> CreateAsync(CreateIncomeRequest request, Guid userId, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<IncomeFrequency>(request.Frequency, out var frequency))
        {
            frequency = IncomeFrequency.Monthly;
        }

        var entity = new IncomeCycle
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AccountId = request.AccountId,
            Title = request.Title,
            Amount = request.Amount,
            ReceivedDate = request.ReceivedDate,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Frequency = frequency,
            Notes = request.Notes,
            IsPlanned = request.IsPlanned,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.IncomeCycles.Add(entity);
        
        // For Weekly and BiWeekly frequencies, create individual income records
        if (frequency == IncomeFrequency.Weekly || frequency == IncomeFrequency.BiWeekly)
        {
            var endDate = request.EndDate ?? request.StartDate.AddYears(1); // Limit to 1 year if no end date
            var currentDate = request.StartDate;
            var daysToAdd = frequency == IncomeFrequency.Weekly ? 7 : 14;
            var totalAmount = 0m;
            var recordCount = 0;
            
            while (currentDate <= endDate)
            {
                var incomeRecord = new IncomeRecord
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    IncomeCycleId = entity.Id,
                    Title = entity.Title,
                    Amount = entity.Amount,
                    ReceivedDate = currentDate,
                    IsFromCredit = false,
                    Notes = entity.Notes,
                    IsPlanned = entity.IsPlanned,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _context.IncomeRecords.Add(incomeRecord);
                totalAmount += entity.Amount;
                recordCount++;
                currentDate = currentDate.AddDays(daysToAdd);
            }
            
            // Update account balance once for all records if account is specified and income is not planned
            // Planned income doesn't affect balance until confirmed
            if (request.AccountId.HasValue && !request.IsPlanned && recordCount > 0)
            {
                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Id == request.AccountId.Value && a.UserId == userId, cancellationToken);
                if (account == null)
                {
                    throw new KeyNotFoundException("Account was not found.");
                }
                account.Balance += totalAmount;
                account.UpdatedAt = DateTime.UtcNow;
            }
        }
        else
        {
            // For other frequencies, update account balance once if account is specified and income is not planned
            // Planned income doesn't affect balance until confirmed
            if (request.AccountId.HasValue && !request.IsPlanned)
            {
                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Id == request.AccountId.Value && a.UserId == userId, cancellationToken);
                if (account == null)
                {
                    throw new KeyNotFoundException("Account was not found.");
                }
                account.Balance += request.Amount;
                account.UpdatedAt = DateTime.UtcNow;
            }
        }
        
        await _context.SaveChangesAsync(cancellationToken);

        return new IncomeCycleDto
        {
            Id = entity.Id,
            Title = entity.Title,
            ReceivedDate = entity.ReceivedDate,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Amount = entity.Amount,
            Frequency = entity.Frequency,
            Notes = entity.Notes,
            AccountId = entity.AccountId,
            IsPlanned = entity.IsPlanned
        };
    }

    /// <inheritdoc/>
    public async Task<IncomeCycleDto> UpdateAsync(Guid id, UpdateIncomeRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var entity = await _context.IncomeCycles.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException("Income cycle was not found.");
        }

        if (!Enum.TryParse<IncomeFrequency>(request.Frequency, out var frequency))
        {
            frequency = IncomeFrequency.Monthly;
        }

        // Handle account balance changes
        var oldAmount = entity.Amount;
        var newAmount = request.Amount;
        var amountDifference = newAmount - oldAmount;

        // Revert old account balance if account was set
        if (entity.AccountId.HasValue)
        {
            var oldAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == entity.AccountId.Value, cancellationToken);
            if (oldAccount != null)
            {
                oldAccount.Balance -= oldAmount;
                oldAccount.UpdatedAt = DateTime.UtcNow;
            }
        }

        // Update new account balance if account is specified
        if (request.AccountId.HasValue)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId.Value && a.UserId == userId, cancellationToken);
            if (account == null)
            {
                throw new KeyNotFoundException("Account was not found.");
            }
            account.Balance += newAmount;
            account.UpdatedAt = DateTime.UtcNow;
        }

        entity.Title = request.Title;
        entity.Amount = request.Amount;
        entity.ReceivedDate = request.ReceivedDate;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Frequency = frequency;
        entity.Notes = request.Notes;
        entity.AccountId = request.AccountId;
        entity.IsPlanned = request.IsPlanned;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new IncomeCycleDto
        {
            Id = entity.Id,
            Title = entity.Title,
            ReceivedDate = entity.ReceivedDate,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Amount = entity.Amount,
            Frequency = entity.Frequency,
            Notes = entity.Notes,
            AccountId = entity.AccountId,
            IsPlanned = entity.IsPlanned
        };
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var entity = await _context.IncomeCycles.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException("Income cycle was not found.");
        }

        // Revert account balance only for actual (non-planned) cycles — плановый доход никогда не добавлялся на счёт
        if (!entity.IsPlanned && entity.AccountId.HasValue)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == entity.AccountId.Value, cancellationToken);
            if (account != null)
            {
                account.Balance -= entity.Amount;
                account.UpdatedAt = DateTime.UtcNow;
            }
        }

        _context.IncomeCycles.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<PendingPlannedTransactionDto>> GetPendingPlannedIncomeAsync(Guid userId, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Get planned income cycles where start date has arrived and haven't been confirmed yet
        // FirstConfirmedDate is set when cycle is first confirmed, so if it's null, cycle hasn't been confirmed
        var pendingIncomeCycles = await _context.IncomeCycles
            .AsNoTracking()
            .Where(i => i.UserId == userId &&
                       i.IsPlanned &&
                       i.StartDate <= today &&
                       (i.EndDate == null || i.EndDate >= today) &&
                       i.FirstConfirmedDate == null) // Only show cycles that haven't been confirmed yet
            .Select(i => new PendingPlannedTransactionDto
            {
                Id = i.Id,
                Type = "Income",
                Title = i.Title,
                Amount = i.Amount,
                Date = i.StartDate,
                CategoryName = "Доход",
                CategoryId = Guid.Empty,
                SubcategoryId = null,
                SubcategoryName = null,
                Description = i.Notes,
                AccountId = i.AccountId
            })
            .ToListAsync(cancellationToken);

        // Get planned income records where received date has arrived and haven't been confirmed yet
        // FirstConfirmedDate is set when record is first confirmed, so if it's null, record hasn't been confirmed
        var pendingIncomeRecords = await _context.IncomeRecords
            .AsNoTracking()
            .Where(i => i.UserId == userId &&
                       i.IsPlanned &&
                       i.ReceivedDate <= today &&
                       i.FirstConfirmedDate == null)
            .Select(i => new PendingPlannedTransactionDto
            {
                Id = i.Id,
                Type = "IncomeRecord",
                Title = i.Title,
                Amount = i.Amount,
                Date = i.ReceivedDate,
                CategoryName = "Доход",
                CategoryId = Guid.Empty,
                SubcategoryId = null,
                SubcategoryName = null,
                Description = i.Notes,
                AccountId = null
            })
            .ToListAsync(cancellationToken);

        var result = new List<PendingPlannedTransactionDto>();
        result.AddRange(pendingIncomeCycles);
        result.AddRange(pendingIncomeRecords);

        return result;
    }

    /// <inheritdoc/>
    public async Task ConfirmPlannedIncomeAsync(Guid incomeId, Guid userId, decimal? actualAmount, CancellationToken cancellationToken)
    {
        // Try to find as IncomeCycle first
        var incomeCycle = await _context.IncomeCycles
            .FirstOrDefaultAsync(i => i.Id == incomeId && i.UserId == userId, cancellationToken);

        if (incomeCycle != null)
        {
            if (!incomeCycle.IsPlanned)
            {
                return; // Already confirmed
            }

            // Use actual amount if provided, otherwise use planned amount
            var finalAmount = actualAmount ?? incomeCycle.Amount;

            // Create a new actual IncomeRecord based on the planned IncomeCycle
            var incomeRecord = new IncomeRecord
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                IncomeCycleId = incomeCycle.Id,
                Title = incomeCycle.Title,
                Amount = finalAmount,
                ReceivedDate = incomeCycle.StartDate,
                IsFromCredit = false,
                Notes = incomeCycle.Notes,
                IsPlanned = false, // This is an actual income
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.IncomeRecords.Add(incomeRecord);

            // Mark the cycle as confirmed if not already marked
            if (!incomeCycle.FirstConfirmedDate.HasValue)
            {
                incomeCycle.FirstConfirmedDate = DateOnly.FromDateTime(DateTime.UtcNow);
                incomeCycle.UpdatedAt = DateTime.UtcNow;
            }

            // Update account balance if account is specified
            if (incomeCycle.AccountId.HasValue)
            {
                var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == incomeCycle.AccountId.Value && a.UserId == userId, cancellationToken);
                if (account != null)
                {
                    account.Balance += finalAmount;
                    account.UpdatedAt = DateTime.UtcNow;
                }
            }

            // Keep the planned IncomeCycle as is (IsPlanned = true) for plan vs actual reporting
            // Don't modify the original planned IncomeCycle

            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        // Try to find as IncomeRecord
        var incomeRecordExisting = await _context.IncomeRecords
            .FirstOrDefaultAsync(i => i.Id == incomeId && i.UserId == userId, cancellationToken);

        if (incomeRecordExisting != null)
        {
            if (!incomeRecordExisting.IsPlanned)
            {
                return; // Already confirmed
            }

            // Use actual amount if provided, otherwise use planned amount
            var finalAmount = actualAmount ?? incomeRecordExisting.Amount;

            // Create a new actual IncomeRecord based on the planned one
            var actualIncomeRecord = new IncomeRecord
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                IncomeCycleId = incomeRecordExisting.IncomeCycleId,
                Title = incomeRecordExisting.Title,
                Amount = finalAmount,
                ReceivedDate = incomeRecordExisting.ReceivedDate,
                IsFromCredit = incomeRecordExisting.IsFromCredit,
                Notes = incomeRecordExisting.Notes,
                IsPlanned = false, // This is an actual income
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.IncomeRecords.Add(actualIncomeRecord);

            // Mark the planned record as confirmed so it won't appear in pending again even if actual is deleted
            if (!incomeRecordExisting.FirstConfirmedDate.HasValue)
            {
                incomeRecordExisting.FirstConfirmedDate = DateOnly.FromDateTime(DateTime.UtcNow);
                incomeRecordExisting.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        throw new KeyNotFoundException("Income was not found.");
    }
}

