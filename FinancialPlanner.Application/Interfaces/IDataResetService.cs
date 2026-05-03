using System;
using System.Threading;
using System.Threading.Tasks;

namespace FinancialPlanner.Application.Interfaces;

/// <summary>
/// <para>
/// Provides operations to reset user data so that actual income/expense
/// history can be cleared while preserving planned and recurring items.
/// </para>
/// </summary>
public interface IDataResetService
{
    /// <summary>
    /// <para>
    /// Deletes all actual (non-planned) income and expense data for the user and
    /// shifts planned/recurring items so that their dates are not earlier than today.
    /// </para>
    /// </summary>
    /// <param name="userId">The identifier of the user whose data will be reset.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ResetActualDataAsync(Guid userId, CancellationToken cancellationToken);
}

