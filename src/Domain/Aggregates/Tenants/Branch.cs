using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Domain.Common.Guards;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Aggregates.Tenants;

public sealed class Branch : Entity<BranchId>
{
    private readonly List<QrCodeReference> _qrCodes = new();
    private readonly List<ScheduleSlot> _workingHours = new();
    private readonly List<Table> _tables = new();


    // EF Core navigation property for multi-tenant support
    public TenantId TenantId { get; private set; }

    private Branch(BranchId id, string name, Address address)
        : base(id)
    {
        Name = name;
        Address = address;
    }

    private Branch()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public Address Address { get; private set; } = null!;

    public IReadOnlyCollection<QrCodeReference> QrCodes => new ReadOnlyCollection<QrCodeReference>(_qrCodes);

    public IReadOnlyCollection<ScheduleSlot> WorkingHours => new ReadOnlyCollection<ScheduleSlot>(_workingHours);

    public IReadOnlyCollection<Table> Tables => new ReadOnlyCollection<Table>(_tables);

    public static Branch Create(string name, Address address)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        Guard.AgainstNull(address, nameof(address));

        return new Branch(BranchId.New(), name.Trim(), address);
    }

    public void UpdateWorkingHours(IEnumerable<ScheduleSlot> slots)
    {
        Guard.AgainstNull(slots, nameof(slots));

        _workingHours.Clear();
        _workingHours.AddRange(slots.Distinct());
    }

    public Table AddTable(string label, int capacity, bool isOutdoor = false)
    {
        Guard.AgainstNullOrWhiteSpace(label, nameof(label));

        if (_tables.Any(table => table.Label.Equals(label, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException("میزی با این نام/شماره قبلاً ثبت شده است.");
        }

        var table = Table.Create(label, capacity, isOutdoor);
        _tables.Add(table);

        return table;
    }

    public void RemoveTable(TableId tableId)
    {
        var table = _tables.SingleOrDefault(t => t.Id.Equals(tableId));
        if (table is null)
        {
            throw new DomainException("میز موردنظر یافت نشد.");
        }

        _tables.Remove(table);
    }

    public void UpdateTable(TableId tableId, string? label = null, int? capacity = null, bool? isOutdoor = null)
    {
        var table = _tables.SingleOrDefault(t => t.Id.Equals(tableId));
        if (table is null)
        {
            throw new DomainException("میز موردنظر یافت نشد.");
        }

        if (!string.IsNullOrWhiteSpace(label) &&
            _tables.Any(t => t.Id != tableId && t.Label.Equals(label, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException("میزی با این نام/شماره قبلاً ثبت شده است.");
        }

        if (!string.IsNullOrWhiteSpace(label))
        {
            table.UpdateLabel(label);
        }

        if (capacity is not null)
        {
            table.UpdateCapacity(capacity.Value);
        }

        if (isOutdoor is not null)
        {
            table.SetOutdoor(isOutdoor.Value);
        }
    }

    public void MarkTableOutOfService(TableId tableId)
    {
        var table = _tables.SingleOrDefault(t => t.Id.Equals(tableId));
        if (table is null)
        {
            throw new DomainException("میز موردنظر یافت نشد.");
        }

        table.MarkOutOfService();
    }

    public void RestoreTableToService(TableId tableId)
    {
        var table = _tables.SingleOrDefault(t => t.Id.Equals(tableId));
        if (table is null)
        {
            throw new DomainException("میز موردنظر یافت نشد.");
        }

        table.RestoreToService();
    }

    public void RegisterQrCampaign(QrCodeReference qrCode)
    {
        Guard.AgainstNull(qrCode, nameof(qrCode));

        if (_qrCodes.Any(existing => existing == qrCode))
        {
            return;
        }

        _qrCodes.Add(qrCode);
    }

    public void UpdateName(string name)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        Name = name.Trim();
    }

    public void UpdateAddress(Address address)
    {
        Guard.AgainstNull(address, nameof(address));
        Address = address;
    }
}
