using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.Common.Guards;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Aggregates.Tenants;

public sealed class Branch : Entity<BranchId>
{
    private readonly List<QrCodeReference> _qrCodes = new();
    private readonly List<ScheduleSlot> _workingHours = new();

    private Branch(BranchId id, string name, Address address)
        : base(id)
    {
        Name = name;
        Address = address;
    }

    public string Name { get; private set; }

    public Address Address { get; private set; }

    public IReadOnlyCollection<QrCodeReference> QrCodes => new ReadOnlyCollection<QrCodeReference>(_qrCodes);

    public IReadOnlyCollection<ScheduleSlot> WorkingHours => new ReadOnlyCollection<ScheduleSlot>(_workingHours);

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
