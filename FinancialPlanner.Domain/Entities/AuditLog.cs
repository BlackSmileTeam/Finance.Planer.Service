namespace FinancialPlanner.Domain.Entities;

/// <summary>
/// Immutable record of a database change for auditing and potential future rollback.
/// </summary>
public sealed class AuditLog
{
    public Guid Id { get; set; }

    /// <summary>User who triggered the change; null if unknown (e.g. design-time tools).</summary>
    public Guid? UserId { get; set; }

    /// <summary>Insert, Update, or Delete.</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>CLR entity type name (e.g. Expense).</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>JSON object with primary key property names and values.</summary>
    public string? EntityIdJson { get; set; }

    /// <summary>JSON snapshot before change (null for Insert).</summary>
    public string? StateBeforeJson { get; set; }

    /// <summary>JSON snapshot after change (null for Delete).</summary>
    public string? StateAfterJson { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
