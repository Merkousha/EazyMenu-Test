using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EazyMenu.Domain.Abstractions;
using EazyMenu.Domain.Common.Exceptions;
using EazyMenu.Domain.Common.Guards;
using EazyMenu.Domain.Events;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Domain.Aggregates.Tenants;

public sealed class Tenant : Entity<TenantId>, IAggregateRoot
{
    private readonly List<Branch> _branches = new();
    private readonly List<Subscription> _subscriptions = new();
    private Subscription? _activeSubscription;

    private Tenant(TenantId id, string businessName, BrandProfile branding, Email contactEmail, PhoneNumber contactPhone, DateTime createdAtUtc)
        : base(id)
    {
        Guard.AgainstNullOrWhiteSpace(businessName, nameof(businessName));
        Guard.AgainstNull(branding, nameof(branding));
        Guard.AgainstNull(contactEmail, nameof(contactEmail));
        Guard.AgainstNull(contactPhone, nameof(contactPhone));

        BusinessName = businessName.Trim();
        Branding = branding;
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
        CreatedAtUtc = createdAtUtc;

        RaiseDomainEvent(new TenantRegisteredDomainEvent(Id, BusinessName));
    }

    private Tenant()
    {
    }

    public string BusinessName { get; private set; } = string.Empty;

    public BrandProfile Branding { get; private set; } = null!;

    public Email ContactEmail { get; private set; } = null!;

    public PhoneNumber ContactPhone { get; private set; } = null!;

    public Address? HeadquartersAddress { get; private set; }

    public bool IsSuspended { get; private set; }

    public DateTime? SuspendedAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public Subscription? ActiveSubscription
    {
        get => _activeSubscription;
        private set => _activeSubscription = value;
    }

    public IReadOnlyCollection<Branch> Branches => new ReadOnlyCollection<Branch>(_branches);

    public IReadOnlyCollection<Subscription> Subscriptions => new ReadOnlyCollection<Subscription>(_subscriptions);

    public static Tenant Register(string businessName, BrandProfile branding, Email contactEmail, PhoneNumber contactPhone, Address? headquartersAddress = null)
    {
        var tenant = new Tenant(TenantId.New(), businessName, branding, contactEmail, contactPhone, DateTime.UtcNow)
        {
            HeadquartersAddress = headquartersAddress
        };

        return tenant;
    }

    public void UpdateBusinessName(string businessName)
    {
        Guard.AgainstNullOrWhiteSpace(businessName, nameof(businessName));
        BusinessName = businessName.Trim();
    }

    public void UpdateBranding(BrandProfile branding)
    {
        Guard.AgainstNull(branding, nameof(branding));
        Branding = branding;
    }

    public void UpdateContactInformation(Email email, PhoneNumber phone)
    {
        Guard.AgainstNull(email, nameof(email));
        Guard.AgainstNull(phone, nameof(phone));

        ContactEmail = email;
        ContactPhone = phone;
    }

    public void UpdateHeadquartersAddress(Address address)
    {
        Guard.AgainstNull(address, nameof(address));
        HeadquartersAddress = address;
    }

    public Branch AddBranch(string name, Address address)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        Guard.AgainstNull(address, nameof(address));

        if (_branches.Any(branch => branch.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException("شعبه‌ای با این نام قبلاً ثبت شده است.");
        }

        var branch = Branch.Create(name, address);
        _branches.Add(branch);
        return branch;
    }

    public void RemoveBranch(BranchId branchId)
    {
        var branch = _branches.SingleOrDefault(b => b.Id.Equals(branchId));
        if (branch is null)
        {
            throw new DomainException("شعبه مورد نظر یافت نشد.");
        }

        _branches.Remove(branch);
    }

    public void ActivateSubscription(Subscription subscription)
    {
        Guard.AgainstNull(subscription, nameof(subscription));

        EnsureSubscriptionRegistered(subscription);
        subscription.Activate();
        ActiveSubscription = subscription;

        RaiseDomainEvent(new SubscriptionActivatedDomainEvent(Id, subscription.Id, subscription.Plan, DateTime.UtcNow));
    }

    public void RegisterPendingSubscription(Subscription subscription)
    {
        Guard.AgainstNull(subscription, nameof(subscription));
        EnsureSubscriptionRegistered(subscription);
    }

    public void SuspendTenant()
    {
        if (IsSuspended)
        {
            return;
        }

        IsSuspended = true;
        SuspendedAtUtc = DateTime.UtcNow;
    }

    public void ReinstateTenant()
    {
        if (!IsSuspended)
        {
            return;
        }

        IsSuspended = false;
        SuspendedAtUtc = null;
    }

    public void EndActiveSubscriptionAsExpired(DateTime expiredAtUtc)
    {
        if (ActiveSubscription is null)
        {
            throw new DomainException("اشتراک فعالی جهت انقضا وجود ندارد.");
        }

        ActiveSubscription.MarkExpired(expiredAtUtc);
        ActiveSubscription = null;
    }

    public void CancelActiveSubscription()
    {
        if (ActiveSubscription is null)
        {
            throw new DomainException("اشتراک فعالی جهت لغو وجود ندارد.");
        }

        ActiveSubscription.Cancel();
        ActiveSubscription = null;
    }

    public void SuspendActiveSubscription()
    {
        if (ActiveSubscription is null)
        {
            throw new DomainException("اشتراک فعالی جهت تعلیق وجود ندارد.");
        }

        ActiveSubscription.Suspend();
    }

    public void ReinstateSuspendedSubscription()
    {
        if (ActiveSubscription is null)
        {
            throw new DomainException("اشتراک فعالی برای بازگشت وجود ندارد.");
        }

        if (ActiveSubscription.Status != SubscriptionStatus.Suspended)
        {
            throw new DomainException("تنها اشتراک معلق شده قابل بازگشت است.");
        }

        ActiveSubscription.Activate();
    }

    public Subscription RenewSubscription(SubscriptionPlan plan, DateTime newStartUtc, DateTime? newEndUtc, Money newPrice, bool asTrial = false)
    {
        var renewal = Subscription.Create(plan, newPrice, newStartUtc, newEndUtc, asTrial);
        ActivateSubscription(renewal);
        return renewal;
    }

    private void EnsureSubscriptionRegistered(Subscription subscription)
    {
        if (_subscriptions.All(existing => existing.Id != subscription.Id))
        {
            _subscriptions.Add(subscription);
        }
    }
}
