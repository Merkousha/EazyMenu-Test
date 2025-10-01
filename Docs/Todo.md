# Work Tracker

Quick snapshot of what we're working on, what's queued next, and what has been delivered.

## How to Use
- Move items between sections (`In Progress`, `Up Next`, `Done`) at the end of each working session.
- Keep checkboxes in sync with actual status; link to issues or PRs when helpful.
- When closing an item here, log the details in `Docs/ProgressLog.md` for long-term history.

## In Progress


## Up Next
- [ ] افزودن نمودارها به داشبورد (Chart.js integration - GetSalesStatistics، GetPopularItems، GetOrderStatistics).
- [ ] گزارش فروش منو (صفحه Sales Report با فیلتر تاریخ و Excel export).
- [ ] Integrate external providers: wire Zarinpal production callbacks, secure Kavenegar API secrets with resiliency/fallback flows, and extend email/notification channels.
- [ ] Harden security (OAuth/OIDC, MFA) and add observability (logging, metrics, tracing).

## Done
- [x] پیاده‌سازی کامل Welcome Notification System با SMS و Email پس از ثبت‌نام (SendWelcomeNotificationCommand، Handler، یکپارچه‌سازی در EfTenantProvisioningService و VerifyPaymentCommandHandler).
- [x] افزودن نمایش اطلاعات اشتراک به Dashboard (اتصال GetActiveSubscriptionQuery به DashboardController، کارت اشتراک در Index.cshtml با status badges، تاریخ انقضا فارسی، هشدار اشتراک‌های نزدیک به پایان).
- [x] پیاده‌سازی کامل UI سیستم رزرو میز (ReservationsController، ViewModels، Views با AJAX، BranchId Provider، Automatic Table Selection، Navigation Integration).
- [x] رفع خطاهای compile مربوط به DayOfWeek nullable و TableId missing در Views.
- [x] آماده‌سازی پایگاه داده با افزودن 5 میز نمونه برای تست جریان رزرو.
- [x] همسوسازی لایه Presentation با Domain logic - حذف manual table selection و اتکا به automatic table selection مبتنی بر PartySize + PrefersOutdoor.
- [x] یکپارچه‌سازی ISmsSender در PlaceOrderCommandHandler با ارسال پیامک تأییدیه فارسی (شماره سفارش + مبلغ کل) به مشتری پس از ثبت موفق سفارش. شامل 6 تست واحد PlaceOrderCommandHandler جدید (مجموع 153 تست موفق).
- [x] تکمیل زیرساخت رزرو (پیاده‌سازی EF Core برای ReservationRepository، جایگزینی Stub Handlerها با نسخه واقعی و افزودن تست‌های یکپارچه، اصلاح کامل تست‌های Tenant و Reservation، اعتبارسنجی با dotnet build و dotnet test (153 تست موفق)).
