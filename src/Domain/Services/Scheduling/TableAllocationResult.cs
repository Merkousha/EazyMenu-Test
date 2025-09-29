using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Services.Scheduling;

public sealed record TableAllocationResult(bool IsSuccessful, TableId? TableId, string? FailureReason)
{
    public static TableAllocationResult Success(TableId tableId) => new(true, tableId, null);

    public static TableAllocationResult Failure(string reason) => new(false, null, reason);
}
