using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Domain.Common.Guards;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Aggregates.Tenants;

public sealed class Table : Entity<TableId>
{
    private Table(TableId id, string label, int capacity, bool isOutdoor)
        : base(id)
    {
        UpdateLabel(label);
        UpdateCapacity(capacity);
        IsOutdoor = isOutdoor;
        IsOutOfService = false;
    }

    private Table()
    {
    }

    public string Label { get; private set; } = string.Empty;

    public int Capacity { get; private set; }

    public bool IsOutdoor { get; private set; }

    public bool IsOutOfService { get; private set; }

    public static Table Create(string label, int capacity, bool isOutdoor = false)
    {
        return new Table(TableId.New(), label, capacity, isOutdoor);
    }

    public void UpdateLabel(string label)
    {
        Guard.AgainstNullOrWhiteSpace(label, nameof(label));
        Label = label.Trim();
    }

    public void UpdateCapacity(int capacity)
    {
        if (capacity <= 0)
        {
            throw new DomainException("ظرفیت میز باید بزرگ‌تر از صفر باشد.");
        }

        Capacity = capacity;
    }

    public void SetOutdoor(bool isOutdoor)
    {
        IsOutdoor = isOutdoor;
    }

    public void MarkOutOfService()
    {
        IsOutOfService = true;
    }

    public void RestoreToService()
    {
        IsOutOfService = false;
    }
}
