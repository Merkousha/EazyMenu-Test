# Progress Log

A running history of significant work completed in this repository.

## How to Use
- Append new entries to the top of the file with the current date so the latest progress stays visible.
- Summarize what was finished, notable commands/tests that ran, and any follow-up actions.
- Reference related tasks in `Docs/Todo.md` when closing items.

## 2025-09-30 (latest)
- پیاده‌سازی فرمان و هندلر `ExpireSubscription` برای ثبت انقضای اشتراک فعال به همراه بررسی زمان انقضا.
- افزودن Query `GetActiveSubscription` و DTO `SubscriptionDetailsDto` جهت ارائه جزئیات اشتراک فعال (طرح، وضعیت، قیمت، تخفیف).
- نگارش تست‌های واحد برای فرمان انقضا و Query جدید شامل سناریوهای موفق و خطا؛ اجرای `dotnet test` با موفقیت (۵۶ تست).

## 2025-09-30 (earlier-4)
- اضافه شدن فرمان‌های `SuspendSubscription` و `ReinstateSubscription` برای تعلیق و بازگشت اشتراک فعال به همراه اعتبارسنجی وضعیت فعلی.
- گسترش دامنه با متدهای `SuspendActiveSubscription` و `ReinstateSuspendedSubscription` در اگریگیت مستاجر جهت پشتیبانی از جریان جدید.
- نوشتن تست‌های واحد برای سناریوهای موفق و خطا (نبود مستاجر، نبود اشتراک فعال، بازگشت از حالت غیرمعلق) و اجرای `dotnet test` با موفقیت (۴۸ تست).

## 2025-09-30 (earlier)
- پیاده‌سازی فرمان و هندلر `CancelSubscription` برای لغو اشتراک فعال همراه با اعتبارسنجی شناسه مستاجر و وجود اشتراک فعال.
- افزودن تست‌های واحد برای سناریوهای لغو موفق، نبود مستاجر و نبود اشتراک فعال.
- اجرای `dotnet test` با موفقیت (۴۱ تست) پس از افزودن قابلیت لغو.

## 2025-09-30 (earlier-2)
- پیاده‌سازی جریان تمدید اشتراک در لایه Application: فرمان `RenewSubscriptionCommand` و هندلر جدید همراه با بررسی تاریخ‌ها، قیمت و امکان اعمال تخفیف.
- افزودن تست‌های واحد برای سناریوهای موفق، نبود اشتراک فعال و تاریخ شروع نامعتبر با استفاده از انباره درون‌حافظه‌ای.
- اجرای `dotnet test` با موفقیت (۳۸ تست) برای اطمینان از سلامت تغییرات.

## 2025-09-30 (earlier)
- تکمیل لایه Application برای مدیریت شعبه و میز: معرفی `ITenantRepository` و پیاده‌سازی فرمان‌های ایجاد/ویرایش/حذف شعبه، بروزرسانی ساعات کاری و مدیریت میزها (افزودن، ویرایش، حذف، خارج‌از-سرویس/بازگشت به سرویس).
- افزودن Queryهای `GetTenantBranches` و `GetBranchDetails` برای بازگرداندن خلاصه و جزئیات شعبه به همراه DTOهای جدید.
- نگارش تست‌های واحد برای تمام هندلرهای جدید و به‌روزرسانی پوشش آزمون‌ها؛ اجرای `dotnet test` با موفقیت (۳۴ تست).

## 2025-09-30 (later)
- پیاده‌سازی مدیریت میزها در دامنه: `TableId`، کلاس `Table` و متدهای شعبه برای افزودن/ویرایش/خارج‌از-سرویس کردن میزها.
- تکمیل اگریگیت رزرو با تاریخچه وضعیت، رویدادهای دامنه (`ReservationCreated/Confirmed/Cancelled/CheckedIn/NoShow`) و نگاشت به میز مشخص.
- افزودن Value Object های تازه (`ReservationId`, `TableId`) و متد همپوشانی `ScheduleSlot.Overlaps` برای تشخیص تداخل زمان‌ها.
- تعریف سیاست زمان‌بندی پایه (`DefaultReservationSchedulingPolicy`) برای تخصیص میز آزاد بر اساس ظرفیت و بازه زمانی.
- به‌روزرسانی `Docs/DomainModel.md` مطابق مدل جدید رزرو و میزها.
- افزودن تست‌های واحد برای اگریگیت رزرو، مدیریت میز شعبه و سیاست زمان‌بندی به همراه اجرای `dotnet test` جهت اطمینان از عبور همه سناریوها.

## 2025-09-30
- Structured the domain foundation with `ValueObject` and `Entity` base abstractions plus a shared `DomainException` for guard clauses.
- Added rich value objects (`Money`, `Address`, `Email`, `PhoneNumber`, `BrandProfile`, `Percentage`, `ScheduleSlot`, `QrCodeReference`) aligned with the DomainModel specification.
- Implemented the Tenant aggregate (Tenant, Branch, Subscription, enums) complete with domain events for registration and subscription activation.
- Ran `dotnet build` on the full solution to confirm the domain enhancements compile cleanly.

## 2025-09-29 (later)
- Added coordination documents for multi-agent collaboration: `Docs/AGENTS.md` and `Docs/copilot-instructions.md`.
- Documented roles, coding conventions, workflow expectations, and updated guidance for future contributors.

## 2025-09-29
- Converted both `EazyMenu.Web` and `EazyMenu.Public` presentation projects from Razor Pages to ASP.NET Core MVC (controllers, views, layouts, updated startup pipelines).
- Updated `Docs/Architecture.md` and `README.md` to reflect the MVC presentation approach.
- Added clean scaffolding artifacts for the admin and public MVC apps (layouts, view imports, placeholder screens).
- Ensured solution builds and automated tests run successfully after the conversion.

## 2025-09-29 (earlier)
- Scaffolded the Clean Architecture solution: Domain, Application, Infrastructure, Presentation (Web/Public), and `tests` projects.
- Implemented initial domain primitives (`TenantId`, `Money`, `Restaurant`, `MenuCategory`, `MenuItem`) and a sample onboarding use case with an in-memory provisioning service.
- Created architecture overview documentation plus README with structure, commands, and domain context (`eazymenu.ir`).
- Replaced template tests with unit/integration tests covering value objects and the onboarding command flow.
- Ran `dotnet build` and `dotnet test` to validate the initial scaffold.
