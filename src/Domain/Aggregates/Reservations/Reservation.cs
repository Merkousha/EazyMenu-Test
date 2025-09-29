using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Domain.Common.Guards;
using EazyMenu.Domain.Events;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Aggregates.Reservations;

public sealed class Reservation : Entity<ReservationId>, IAggregateRoot
{
    private readonly List<ReservationStatusHistoryEntry> _statusHistory = new();

    private Reservation(
        ReservationId id,
        TenantId tenantId,
        BranchId branchId,
        TableId tableId,
        ScheduleSlot scheduleSlot,
        int partySize,
        string? specialRequest,
        string? customerName,
        PhoneNumber? customerPhone,
        DateTime createdAtUtc)
        : base(id)
    {
    Guard.AgainstNull(scheduleSlot, nameof(scheduleSlot));
    Guard.AgainstDefault(tenantId, nameof(tenantId));
    Guard.AgainstDefault(branchId, nameof(branchId));
    Guard.AgainstDefault(tableId, nameof(tableId));

        TenantId = tenantId;
        BranchId = branchId;
        TableId = tableId;
        ScheduleSlot = scheduleSlot;
        PartySize = partySize;
        SpecialRequest = NormalizeSpecialRequest(specialRequest);
        CustomerName = NormalizeCustomerName(customerName);
        CustomerPhone = customerPhone;
        CreatedAtUtc = createdAtUtc;
        Status = ReservationStatus.Pending;
        ConfirmedAtUtc = null;
        CancelledAtUtc = null;
        CheckedInAtUtc = null;
        NoShowRecordedAtUtc = null;

        AddStatusHistory(ReservationStatus.Pending, createdAtUtc, "رزرو ثبت شد.");
    }

    private Reservation()
    {
    }

    public TenantId TenantId { get; private set; }

    public BranchId BranchId { get; private set; }

    public TableId TableId { get; private set; }

    public ScheduleSlot ScheduleSlot { get; private set; } = null!;

    public int PartySize { get; private set; }

    public string? SpecialRequest { get; private set; }

    public string? CustomerName { get; private set; }

    public PhoneNumber? CustomerPhone { get; private set; }

    public ReservationStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? ConfirmedAtUtc { get; private set; }

    public DateTime? CancelledAtUtc { get; private set; }

    public DateTime? CheckedInAtUtc { get; private set; }

    public DateTime? NoShowRecordedAtUtc { get; private set; }

    public IReadOnlyCollection<ReservationStatusHistoryEntry> StatusHistory => new ReadOnlyCollection<ReservationStatusHistoryEntry>(_statusHistory);

    public static Reservation Schedule(
        TenantId tenantId,
        BranchId branchId,
        TableId tableId,
        ScheduleSlot scheduleSlot,
        int partySize,
        int tableCapacity,
        string? specialRequest = null,
        string? customerName = null,
        PhoneNumber? customerPhone = null,
        DateTime? createdAtUtc = null)
    {
        Guard.AgainstDefault(tenantId, nameof(tenantId));
        Guard.AgainstDefault(branchId, nameof(branchId));
        Guard.AgainstDefault(tableId, nameof(tableId));
        Guard.AgainstNull(scheduleSlot, nameof(scheduleSlot));

        ValidatePartySize(partySize);
        EnsurePartySizeFits(partySize, tableCapacity);

        var reservation = new Reservation(
            ReservationId.New(),
            tenantId,
            branchId,
            tableId,
            scheduleSlot,
            partySize,
            specialRequest,
            customerName,
            customerPhone,
            createdAtUtc ?? DateTime.UtcNow);

        reservation.RaiseDomainEvent(new ReservationCreatedDomainEvent(
            reservation.Id,
            reservation.TenantId,
            reservation.BranchId,
            reservation.TableId,
            reservation.ScheduleSlot,
            reservation.PartySize));

        return reservation;
    }

    public void Confirm(DateTime confirmedAtUtc, string? note = null)
    {
        EnsureTransitionAllowed(ReservationStatus.Confirmed);
        Status = ReservationStatus.Confirmed;
        ConfirmedAtUtc = confirmedAtUtc;
        AddStatusHistory(Status, confirmedAtUtc, note);

        RaiseDomainEvent(new ReservationConfirmedDomainEvent(
            Id,
            TenantId,
            BranchId,
            TableId,
            confirmedAtUtc));
    }

    public void Cancel(DateTime cancelledAtUtc, string reason)
    {
        Guard.AgainstNullOrWhiteSpace(reason, nameof(reason));
        EnsureNotFinalized();

        Status = ReservationStatus.Cancelled;
        CancelledAtUtc = cancelledAtUtc;
        AddStatusHistory(Status, cancelledAtUtc, reason);

        RaiseDomainEvent(new ReservationCancelledDomainEvent(
            Id,
            TenantId,
            BranchId,
            TableId,
            cancelledAtUtc,
            reason));
    }

    public void MarkAsCheckedIn(DateTime checkedInAtUtc, string? note = null)
    {
        if (Status != ReservationStatus.Confirmed)
        {
            throw new DomainException("فقط رزروهای تاییدشده قابل ثبت حضور هستند.");
        }

        Status = ReservationStatus.CheckedIn;
        CheckedInAtUtc = checkedInAtUtc;
        AddStatusHistory(Status, checkedInAtUtc, note);

        RaiseDomainEvent(new ReservationCheckedInDomainEvent(
            Id,
            TenantId,
            BranchId,
            TableId,
            checkedInAtUtc));
    }

    public void MarkAsNoShow(DateTime recordedAtUtc, string? note = null)
    {
        if (Status != ReservationStatus.Confirmed)
        {
            throw new DomainException("فقط رزروهای تاییدشده قابل ثبت عدم حضور هستند.");
        }

        Status = ReservationStatus.NoShow;
        NoShowRecordedAtUtc = recordedAtUtc;
        AddStatusHistory(Status, recordedAtUtc, note);

        RaiseDomainEvent(new ReservationNoShowRecordedDomainEvent(
            Id,
            TenantId,
            BranchId,
            TableId,
            recordedAtUtc));
    }

    public void Reschedule(ScheduleSlot newSlot, DateTime changedAtUtc, string? note = null)
    {
        Guard.AgainstNull(newSlot, nameof(newSlot));
        EnsureNotFinalized();

        ScheduleSlot = newSlot;
        AddStatusHistory(Status, changedAtUtc, note ?? "برنامه زمانی رزرو تغییر کرد.");
    }

    public void ChangeTable(TableId newTableId, int tableCapacity, DateTime changedAtUtc, string? note = null)
    {
        Guard.AgainstNull(newTableId, nameof(newTableId));
        EnsureNotFinalized();

        EnsurePartySizeFits(PartySize, tableCapacity);

        TableId = newTableId;
        AddStatusHistory(Status, changedAtUtc, note ?? "میز رزرو تغییر یافت.");
    }

    public void UpdatePartySize(int newPartySize, int tableCapacity, DateTime changedAtUtc, string? note = null)
    {
        ValidatePartySize(newPartySize);
        EnsurePartySizeFits(newPartySize, tableCapacity);
        EnsureNotFinalized();

        PartySize = newPartySize;
        AddStatusHistory(Status, changedAtUtc, note ?? "تعداد نفرات رزرو تغییر یافت.");
    }

    public void UpdateSpecialRequest(string? specialRequest)
    {
        SpecialRequest = NormalizeSpecialRequest(specialRequest);
    }

    public void UpdateContactInformation(string? customerName, PhoneNumber? customerPhone)
    {
        CustomerName = NormalizeCustomerName(customerName);
        CustomerPhone = customerPhone;
    }

    private static void EnsurePartySizeFits(int partySize, int tableCapacity)
    {
        if (tableCapacity <= 0)
        {
            throw new DomainException("ظرفیت میز باید بزرگ‌تر از صفر باشد.");
        }

        if (partySize > tableCapacity)
        {
            throw new DomainException("تعداد نفرات با ظرفیت میز همخوانی ندارد.");
        }
    }

    private static void ValidatePartySize(int partySize)
    {
        if (partySize <= 0)
        {
            throw new DomainException("تعداد نفرات رزرو باید بزرگ‌تر از صفر باشد.");
        }
    }

    private void EnsureTransitionAllowed(ReservationStatus targetStatus)
    {
        if (Status == ReservationStatus.Cancelled || Status == ReservationStatus.CheckedIn || Status == ReservationStatus.NoShow)
        {
            throw new DomainException("رزرو نهایی‌شده قابل تغییر وضعیت نیست.");
        }

        if (Status == targetStatus)
        {
            return;
        }

        if (targetStatus == ReservationStatus.Confirmed && Status != ReservationStatus.Pending)
        {
            throw new DomainException("تنها رزروهای در وضعیت انتظار قابل تایید هستند.");
        }
    }

    private void EnsureNotFinalized()
    {
        if (Status is ReservationStatus.Cancelled or ReservationStatus.CheckedIn or ReservationStatus.NoShow)
        {
            throw new DomainException("رزرو نهایی‌شده قابل ویرایش نیست.");
        }
    }

    private void AddStatusHistory(ReservationStatus status, DateTime changedAtUtc, string? note)
    {
        _statusHistory.Add(new ReservationStatusHistoryEntry(status, changedAtUtc, note));
    }

    private static string? NormalizeSpecialRequest(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static string? NormalizeCustomerName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
