# Work Tracker

Quick snapshot of what we're working on, what's queued next, and what has been delivered.

## How to Use
- Move items between sections (`In Progress`, `Up Next`, `Done`) at the end of each working session.
- Keep checkboxes in sync with actual status; link to issues or PRs when helpful.
- When closing an item here, log the details in `Docs/ProgressLog.md` for long-term history.

## In Progress
- [ ] توسعه قابلیت‌های تعاملی مدیریت منو (CRUD کامل، مرتب‌سازی، انتشار) با تکیه بر فرمان‌های Application و Endpointهای AJAX (مسیر Quick Update تکمیل شد؛ گام بعد: SignalR و همگام‌سازی سایت عمومی پس از انتشار).

## Up Next
- [ ] Integrate external providers: wire Zarinpal production callbacks, secure Kavenegar API secrets with resiliency/fallback flows, and extend email/notification channels.
- [ ] تکمیل جریان‌های سفارش مشتری و کانال‌های آنلاین پس از آماده‌سازی UI مدیریت منو و اتصال آن به داده واقعی.
- [ ] Harden security (OAuth/OIDC, MFA) and add observability (logging, metrics, tracing).

## Done
- [x] طراحی و پیاده‌سازی مسیر Quick Update قیمت/موجودی و به‌روزرسانی سریع Dashboard (فرمان تجمیعی، اکشن AJAX، View `QuickUpdate` و اسکریپت `menu-quick-update.js`).
- [x] اتصال UI مدیریت منو به داده نمونه و Queries لایه Application (Controller، ViewModel، Viewها، ناوبری).
- [x] تکمیل لایه Persistency منو و آماده‌سازی داده توسعه (Use Caseها، ریپازیتوری EF Core، مایگریشن `AddMenus`، Seed خودکار در محیط Development).
- [x] پیاده‌سازی دامنه منو (Value Objectها، Aggregate، رویدادهای دامنه) به همراه تست‌های واحد.
- [x] تدوین سند طراحی جامع مدیریت منو (دامنه، Use Case، Persistence، UI) در `Docs/Design/MenuManagement.md`.
- [x] افزودن پایش مصرف پیامک مستاجر (کوئری خلاصه، خوانشگر EF، کارت‌های داشبورد) و به‌روزرسانی تست‌ها.
- [x] راه‌اندازی هشدار زنده شکست پیامک با SignalR و اتصال آن به داشبورد مدیریتی.
- [x] تکمیل fallback ایمیل/اعلان برای موارد شکست ارسال پیامک و ثبت آن در گزارش پیامک‌ها.
- [x] نمایش گزارش پیامک‌ها در داشبورد مدیریتی با فیلتر وضعیت و صفحه‌بندی (NotificationsController + Razor View).
- [x] پیاده‌سازی ورود پیامکی مشتریان (OTP) در سایت عمومی به همراه تست‌های کاربردی.
- [x] طراحی و پیاده‌سازی ویزارد آنبوردینگ (کنترلر، ViewModel، Viewها) با اتصال به `RegisterTenantCommand` و هدایت به پرداخت.
- [x] نمایش نتیجه بازگشت زرین‌پال در MVC با View `Payments/Callback` و کنترلر مرتبط.
- [x] پیاده‌سازی اعتبارسنجی پرداخت زرین‌پال و فعال‌سازی اشتراک پس از تأیید.
- [x] اتصال جریان پرداخت به ثبت اشتراک (ایجاد فاکتور، ثبت وضعیت پرداخت، مدیریت تخفیف‌ها).
- [x] Convert presentation layer projects to ASP.NET Core MVC and update documentation.
- [x] Scaffold Clean Architecture solution with Domain, Application, Infrastructure, Presentation, and tests.
- [x] Implement core domain/value objects and sample onboarding command + tests.
- [x] Document architecture vision, solution layout, and repo usage in `Docs/Architecture.md` and `README.md`.
- [x] Implement tenant-aware persistence (EF Core + SQL Server) including DbContext, entity mappings, DI registration، و نخستین مایگریشن.
- [x] اتصال هندلرهای لایه Application به لایه Persistency مبتنی بر EF Core به همراه تست‌های یکپارچه.
- [x] جایگزینی سرویس ثبت مستاجر با پیاده‌سازی EF Core و ذخیره رکوردهای Provisioning.
- [x] فعال‌سازی چرخه اشتراک در فرایند ثبت مستاجر با قیمت‌گذاری اولیه و دوره آزمایشی.
