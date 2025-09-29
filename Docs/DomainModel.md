# دامنه محصول ایزی‌منو

این سند خلاصه‌ای از نیازهای دامنه‌ای استخراج‌شده از PRD و یوزر استوری‌هاست تا پایه طراحی مدل‌های Clean Architecture فراهم شود.

## قابلیت‌ها و تاثیر دامنه‌ای

| قابلیت | نیازمندی دامنه‌ای |
|--------|--------------------|
| ثبت‌نام و اشتراک | مدیریت چرخه عمر «مستاجر» (Tenant) شامل پلن، وضعیت فعال/تعلیق، دوره آزمایشی، تخفیف، تاریخ انقضا. |
| ورود و احراز هویت | پروفایل مدیران و کارکنان، وضعیت MFA، قفل حساب پس از خطا، ثبت لاگ نشست‌ها. |
| ساخت وب‌سایت اختصاصی | پیکربندی برند (لوگو، رنگ، قالب)، تنظیمات دامنه/زیر دامنه، ساعات کاری، محتوای صفحات. |
| مدیریت منوی دیجیتال | ساختار منو (Menu) با دسته‌ها، آیتم‌ها، قیمت، موجودی، برچسب‌های رژیمی، چندزبانه. |
| تولید QR Code | انتساب QR به هر شعبه/کمپین، ردیابی آمار اسکن. |
| سفارش آنلاین | ثبت سفارش، آیتم‌ها، سبد خرید، یادداشت‌ها، محاسبه مالیات/هزینه ارسال، وضعیت سفارش. |
| رزرو میز | برنامه زمان‌بندی، ظرفیت، جلوگیری از رزرو تکراری، وضعیت رزرو (تایید، لغو، حضور). |
| پرداخت زرین‌پال | تراکنش، وضعیت پرداخت، امکان تلاش مجدد، بازگشت وجه. |
| پیامک کاوه‌نگار | صف ارسال پیامک تراکنشی، پیگیری وضعیت، سهمیه مصرفی بر اساس پلن. |
| اعلان‌ها | ثبت رویدادهای اعلان، کانال‌های فعال، وضعیت خوانده شده/نشده، سطح اهمیت. |
| داشبورد تحلیلی | تولید آمار پرفروش/کم‌فروش، تحلیل سفارشات، رضایت مشتری، مصرف پیامک. |
| مرکز پشتیبانی | تیکت پشتیبانی، وضعیت رسیدگی، سطح اولویت. |

## موجودیت‌ها و اگریگیت‌های پیشنهادی

- **Tenant** (اگریگیت ریشه):
  - فیلدها: `TenantId`, اطلاعات کسب‌وکار، وضعیت تأیید، پلن فعال، تاریخچه اشتراک.
  - رابطه با: `Branch`, `Subscription`, `Manager`.
  - رویدادها: `TenantRegistered`, `SubscriptionActivated`.

- **Subscription** (اگریگیت مستقل یا بخشی از Tenant):
  - پلن، قیمت، دوره، وضعیت، تاریخ شروع/پایان، تخفیف، کدReferral.

- **Branch**:
  - اطلاعات مکان، ساعات کاری، ظرفیت، اطلاع‌رسانی، QR های فعال.

- **Menu** (اگریگیت):
  - دسته‌ها (`MenuCategory`)، آیتم‌ها (`MenuItem`)، نسخه‌بندی، زبان.

- **Order** (اگریگیت):
  - اطلاعات مشتری (مهمان)، اقلام (`OrderItem`)، نوع سفارش (DineIn, Pickup, Delivery), وضعیت، مجموع مبالغ، پرداخت.
  - رویدادها: `OrderPlaced`, `OrderAccepted`, `OrderFulfilled`, `OrderCancelled`.

- **Reservation** (اگریگیت):
  - تاریخ/ساعت، تعداد نفرات، درخواست ویژه، وضعیت، لینک به Branch.
  - رویدادها: `ReservationCreated`, `ReservationCancelled`, `ReservationCheckedIn`.

- **PaymentTransaction**:
  - شناسه زرین‌پال، مبلغ، وضعیت، نوع پرداخت، لینک به Order یا Subscription.

- **Notification**:
  - نوع رویداد، کانال (Dashboard, Email, SMS), سطح اهمیت، وضعیت (Unread/Read).

- **SupportTicket**:
  - عنوان، شرح، اولویت، وضعیت، پیام‌ها.

- **Manager/User**:
  - نقش، دسترسی، وضعیت MFA، نشست‌های فعال.

- **SmsMessage**:
  - الگو، مقصد، وضعیت ارسال، خطا، مصرف.

## ارزش‌اشیاء پیشنهادی (Value Objects)

- `TenantId`, `BranchId`, `MenuId`, `OrderId`, `ReservationId`, `PaymentId`
- `Money`, `Percentage`, `Quantity`
- `Email`, `PhoneNumber`
- `Address` (شامل شهر، خیابان، کدپستی)
- `ScheduleSlot` (تاریخ + بازه زمانی)
- `LocalizedText` (برای چندزبانه)
- `QrCodeReference`
- `OrderStatus`, `ReservationStatus`, `SubscriptionStatus`, `PaymentStatus` (Enum-VO)

## رویدادهای دامنه‌ای محتمل

- `TenantRegisteredDomainEvent`
- `SubscriptionActivatedDomainEvent`
- `BranchCreatedDomainEvent`
- `MenuPublishedDomainEvent`
- `OrderPlacedDomainEvent`
- `OrderStatusChangedDomainEvent`
- `PaymentCompletedDomainEvent`
- `ReservationCreatedDomainEvent`
- `ReservationCancelledDomainEvent`
- `NotificationDispatchedDomainEvent`

## سرویس‌های دامنه‌ای و سیاست‌ها

- **TenantProvisioningService** (در حال حاضر InMemory – نیاز به پیاده‌سازی واقعی).
- **PricingService** برای محاسبه قیمت پلن‌ها و سفارش‌ها (مالیات، تخفیف، هزینه ارسال).
- **SchedulingPolicy** برای جلوگیری از تداخل رزروها.
- **InventoryPolicy** برای موجودی آیتم‌ها (فعلاً خارج از محدوده MVP ولی در نظر گرفته شود).
- **NotificationPolicy** برای انتخاب کانال اعلان بر اساس پلن و تنظیمات.

## اولویت پیاده‌سازی (بر اساس MVP)

1. Tenant, Subscription, Branch, Manager (احراز هویت و مالکیت).
2. Menu, MenuCategory, MenuItem + سیاست موجودی.
3. Order, OrderItem, PaymentTransaction + رویدادهای پرداخت.
4. Reservation + SchedulingPolicy.
5. Notification, SmsMessage.
6. SupportTicket (در صورت رسیدن به MVP).

## ساختار پیشنهادی کد در لایه Domain

```
src/Domain
 ├── Abstractions
 │   ├── IAggregateRoot.cs
 │   ├── IDomainEvent.cs
 │   └── IRepository<T>.cs (در صورت نیاز)
 ├── Common
 │   ├── Guards/
 │   └── Exceptions/
 ├── ValueObjects
 │   ├── TenantId.cs
 │   ├── BranchId.cs
 │   ├── Money.cs
 │   ├── Email.cs
 │   ├── PhoneNumber.cs
 │   ├── Address.cs
 │   ├── ScheduleSlot.cs
 │   ├── OrderNumber.cs
 │   └── Status ها (OrderStatus, ReservationStatus, PaymentStatus, SubscriptionStatus)
 ├── Aggregates
 │   ├── Tenants
 │   │   ├── Tenant.cs
 │   │   ├── Subscription.cs
 │   │   ├── SubscriptionPlan.cs (Enum)
 │   │   └── Branch.cs
 │   ├── Menus
 │   │   ├── Menu.cs
 │   │   ├── MenuCategory.cs
 │   │   └── MenuItem.cs
 │   ├── Orders
 │   │   ├── Order.cs
 │   │   ├── OrderItem.cs
 │   │   └── OrderNote.cs (VO)
 │   ├── Reservations
 │   │   ├── Reservation.cs
 │   │   └── ReservationStatusHistory.cs
 │   ├── Payments
 │   │   └── PaymentTransaction.cs
 │   ├── Notifications
 │   │   └── Notification.cs
 │   └── Support
 │       └── SupportTicket.cs
 ├── Events
 │   ├── TenantRegisteredDomainEvent.cs
 │   ├── SubscriptionActivatedDomainEvent.cs
 │   ├── OrderPlacedDomainEvent.cs
 │   ├── PaymentCompletedDomainEvent.cs
 │   └── ReservationCreatedDomainEvent.cs
 └── Services
     ├── Pricing
     ├── Scheduling
     └── NotificationRouting
```

## مشخصات کلیدی کلاس‌ها

- **Tenant**: نگهداری اطلاعات پایه، پلن جاری، شعب، مدیران. متدها: `ActivateSubscription`, `Suspend`, `AddBranch`, `UpdateBranding`.
- **Subscription**: `Plan`, `Price`, `Status`, `StartDate`, `EndDate`, `IsTrial`, `DiscountCode`. متدها: `Activate`, `Cancel`, `Renew`, `ApplyDiscount`.
- **Branch**: `BranchId`, `Name`, `Address`, `WorkingHours`, `QrCodes`. متدها: `UpdateWorkingHours`, `RegisterQrCampaign`.
- **Menu/MenuCategory/MenuItem**: پشتیبانی چندزبانه، موجودی، قیمت‌های متفاوت آنلاین/حضوری. متدها: `Publish`, `Archive`, `UpdatePrice`, `SetAvailability`.
- **Order**: `OrderNumber`, `TenantId`, `BranchId`, `CustomerInfo`, `Items`, `Totals`, `Status`, `PaymentStatus`, `DeliveryMethod`. متدها: `AddItem`, `ApplyDiscount`, `Confirm`, `StartPreparation`, `Complete`, `Cancel`.
- **OrderItem**: `MenuItemId`, `Quantity`, `UnitPrice`, `Notes`. متدها: `UpdateQuantity`, `UpdateNotes`.
- **Reservation**: `ReservationId`, `TenantId`, `BranchId`, `ScheduleSlot`, `PartySize`, `Status`, `SpecialRequest`. متدها: `Confirm`, `Cancel`, `MarkAsNoShow`, `CheckIn`.
- **PaymentTransaction**: `PaymentId`, `ExternalReference`, `Amount`, `Status`, `Method`, `IssuedAt`, `CompletedAt`. متدها: `MarkSucceeded`, `MarkFailed`, `MarkRefunded`.
- **Notification**: `NotificationId`, `Type`, `Channel`, `Severity`, `Content`, `IsRead`. متدها: `MarkAsRead`, `ScheduleRetry`.
- **SupportTicket**: `TicketId`, `Subject`, `Description`, `Priority`, `Status`, `Messages`. متدها: `AddMessage`, `Assign`, `Resolve`, `Reopen`.

## تصمیمات طراحی تکمیلی

- استفاده از Value Object برای شناسه‌ها، داده‌های فرمت‌دار و وضعیت‌ها.
- مدیریت خطاهای دامنه با استثناهای اختصاصی در `Domain.Common.Exceptions`.
- اجتناب از وابستگی مستقیم دامنه به فریم‌ورک‌ها یا سرویس‌های خارجی.
- ثبت تاریخچه وضعیت‌ها (سفارش، رزرو) با استفاده از آبجکت‌های ارزش یا Entity داخلی.

این فهرست مبنای طراحی و پیاده‌سازی لایه دامنه خواهد بود.
