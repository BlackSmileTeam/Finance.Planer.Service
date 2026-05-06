using System.Security.Claims;
using System.Text.Json;
using FinancialPlanner.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FinancialPlanner.Infrastructure.Persistence;

/// <summary>
/// Builds <see cref="AuditLog"/> rows from EF Core change tracker entries (scalar properties only).
/// </summary>
internal static class ChangeTrackerAuditExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private static readonly HashSet<string> SensitivePropertyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "PasswordHash",
    };

    public static Guid? TryGetCurrentUserId(IHttpContextAccessor? http, AuditActorContext? auditActor = null)
    {
        if (auditActor?.ActingUserId is Guid acting)
            return acting;

        var sub = http?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return sub != null && Guid.TryParse(sub, out var id) ? id : null;
    }

    public static IReadOnlyList<AuditLog> BuildAuditLogs(ChangeTracker tracker, Guid? userId)
    {
        var list = new List<AuditLog>();
        var entries = tracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => e.Entity is not AuditLog)
            .ToList();

        foreach (var entry in entries)
        {
            var entityType = entry.Metadata.ClrType.Name;
            var action = entry.State switch
            {
                EntityState.Added => "Insert",
                EntityState.Modified => "Update",
                EntityState.Deleted => "Delete",
                _ => "Unknown",
            };

            var pk = entry.Metadata.FindPrimaryKey();
            if (pk == null)
                continue;

            var idDict = new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (var prop in pk.Properties)
            {
                var pe = entry.Property(prop.Name);
                idDict[prop.Name] = pe.CurrentValue;
            }

            string? before = null;
            string? after = null;

            if (entry.State is EntityState.Modified or EntityState.Deleted)
                before = SerializeScalars(entry, useOriginal: true);

            if (entry.State is EntityState.Added or EntityState.Modified)
                after = SerializeScalars(entry, useOriginal: false);

            list.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityIdJson = JsonSerializer.Serialize(idDict, JsonOptions),
                StateBeforeJson = before,
                StateAfterJson = after,
                CreatedAtUtc = DateTime.UtcNow,
            });
        }

        return list;
    }

    private static string SerializeScalars(EntityEntry entry, bool useOriginal)
    {
        var dict = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var prop in entry.Properties)
        {
            if (prop.Metadata.IsPrimaryKey())
                continue;
            if (SensitivePropertyNames.Contains(prop.Metadata.Name))
                continue;

            object? value;
            try
            {
                value = useOriginal ? prop.OriginalValue : prop.CurrentValue;
            }
            catch
            {
                continue;
            }

            dict[prop.Metadata.Name] = value;
        }

        return JsonSerializer.Serialize(dict, JsonOptions);
    }
}
