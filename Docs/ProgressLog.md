# Progress Log

A running history of significant work completed in this repository.

## How to Use
- Append new entries to the top of the file with the current date so the latest progress stays visible.
- Summarize what was finished, notable commands/tests that ran, and any follow-up actions.
- Reference related tasks in `Docs/Todo.md` when closing items.

## 2025-10-02 (latest)
- تکمیل قرارداد خوانش لاگ پیامک با پیاده‌سازی `ISmsDeliveryLogReader` در `EfSmsDeliveryStore` شامل صفحه‌بندی، مرتب‌سازی بر اساس زمان و فیلتر وضعیت.
- ثبت چندگانه‌ی وابستگی‌ها در DI تا هر دو رابط `ISmsDeliveryStore` و `ISmsDeliveryLogReader` از پیاده‌سازی EF استفاده کنند.
- نگارش تست یکپارچه `EfSmsDeliveryStoreTests` با استفاده از پایگاه‌داده InMemory برای اعتبارسنجی ترتیب، صفحه‌بندی و فیلتر وضعیت لاگ‌ها.
- اجرای `dotnet build` و `dotnet test` (۷۸ تست موفق، مدت ۲.۸ ثانیه) برای اطمینان از سلامت تغییرات.
- گام بعدی: نمایش صفحه گزارش پیامک در داشبورد مدیریتی و افزودن فیلترهای UI.

## 2025-10-02 (earlier-0)
- طراحی قرارداد جدید `ISmsDeliveryStore` به همراه مدل‌های `SmsDeliveryRecord` و `SmsDeliveryStatus` برای ثبت وضعیت ارسال پیامک‌ها در لایه Application.
- افزودن موجودیت `SmsDeliveryLog`، پیکربندی EF Core و مایگریشن `AddSmsDeliveryLogs` جهت ذخیره‌سازی نتایج ارسال پیامک در پایگاه‌داده.
- پیاده‌سازی `EfSmsDeliveryStore` و اتصال آن به سرویس‌های `LoggingSmsSender` و `KavenegarSmsSender` با ثبت موفقیت/خطا و مدیریت خطاهای شبکه.
- نگارش تست‌های جدید `LoggingSmsSenderTests` (واحد) و به‌روزرسانی `KavenegarSmsSenderTests` برای پوشش ثبت لاگ‌های ارسال.
- اجرای `dotnet build` و `dotnet test` (۷۷ تست موفق، مدت ۲.۸ ثانیه) برای اعتبارسنجی تغییرات.
- گام بعدی: نمایش گزارش پیامک‌ها در داشبورد و آماده‌سازی fallback ایمیلی هنگام شکست ارسال پیامک.

## 2025-10-02 (earlier-0)
- تکمیل پیکربندی چندارائه‌دهنده پیامک با افزودن `SmsOptions`، `SmsProvider` و ثبت آن‌ها در `DependencyInjection` به‌همراه پیش‌فرض‌های پیکربندی در `appsettings` پروژه‌های وب و عمومی.
- پیاده‌سازی کلاینت `KavenegarSmsSender` مبتنی بر `HttpClient` با مدیریت خطا، ثبت لاگ و اعتبارسنجی ورودی‌ها.
- نگارش تست‌های یکپارچه `KavenegarSmsSenderTests` برای پوشش سناریوهای موفق، نبود کلید API و خطای درگاه با استفاده از هندلر ساختگی.
- اجرای `dotnet build` و `dotnet test` (۷۶ تست موفق، مدت ۳.۲ ثانیه) جهت اطمینان از سلامت تغییرات.
- گام بعدی: دریافت کلیدهای محیطی کاوه‌نگار و پیاده‌سازی fallbackهای پیامک و ایمیل برای سناریوهای خطا.

## 2025-10-01 (earlier-0)
- راه‌اندازی ویزارد آنبوردینگ در لایه Presentation با کنترلر `OnboardingController`، مدل‌های `RegisterTenantViewModel`/`ProvisioningSuccessViewModel` و صفحات Razor (`Onboarding/Start`, `Onboarding/Success`) جهت ثبت مستاجر و جمع‌آوری اطلاعات تماس، آدرس و طرح انتخابی.
- اتصال فرم آنبوردینگ به Use Case `RegisterTenantCommand` و مدیریت سناریوهای آزمایشی و پرداختی همراه با پیام‌های فارسی و ریدایرکت خودکار به درگاه زرین‌پال یا نمایش نتیجه فعال‌سازی اشتراک.
- افزودن تست‌های واحد `OnboardingControllerTests` برای پوشش مسیرهای موفق، اعتبارسنجی ModelState و ریدایرکت پرداخت.
- به‌روزرسانی DI جهت ثبت Handlerها با `ICommandHandler<TCommand, TResult>` و هم‌راست‌سازی کنترلرهای پرداخت/آنبوردینگ با این الگو؛ اجرای `dotnet build` و `dotnet test` (۶۸ تست موفق، مدت ۲.۵ ثانیه).
- گام بعدی: تکمیل صفحات مدیریتی پس از ورود (داشبورد، مدیریت منو) و آماده‌سازی جریان احراز هویت و ارتباطات بیرونی.

## 2025-10-01 (earlier-1)
- تکمیل جریان بازگشت زرین‌پال در لایه Presentation با پیاده‌سازی View جدید `Views/Payments/Callback.cshtml` و اتصال آن به `PaymentsController` برای نمایش وضعیت پرداخت.
- نمایش پیام‌های موفق/ناموفق همراه با کد پیگیری و شناسه اشتراک در رابط کاربری بر اساس `PaymentCallbackViewModel`.
- اجرای `dotnet build` و `dotnet test` (۶۴ تست موفق، مدت ۳.۴ ثانیه) جهت اطمینان از سلامت تغییرات UI و عدم Regression.
- گام بعدی: تکمیل سایر صفحات MVC و شروع به یکپارچه‌سازی سرویس‌های بیرونی (SMS/Email) پس از نهایی شدن فلوهای پرداخت.

## 2025-10-01 (earlier-2)
- افزودن Use Case جدید `VerifyPaymentCommand` برای اعتبارسنجی پرداخت زرین‌پال، همراه با `PaymentVerificationRequest/Response` و انبار `IPaymentTransactionRepository`.
- توسعه دامنه جهت فعال‌سازی اشتراک‌های در حالت Pending پس از موفقیت پرداخت و نگهداری مرجع تراکنش.
- ارتقای کلاینت شبیه‌ساز زرین‌پال برای پشتیبانی از متد `VerifyPaymentAsync` و سناریوهای انقضا/لغو.
- نگارش تست‌های یکپارچه برای تأیید سناریوهای موفق و ناموفق پرداخت به همراه تقویت تست واحد تراکنش پرداخت؛ اجرای `dotnet test` (۶۴ تست موفق، مدت ۳.۱ ثانیه).
- گام بعدی: پیاده‌سازی سرویس Callback در لایه Presentation جهت دریافت پارامترهای `Authority`/`Status` و فراخوانی Use Case جدید.

## 2025-10-01 (earlier-3)
- معرفی دامنه پرداخت با `PaymentTransaction`, `PaymentId`, و Enumerationهای `PaymentStatus`/`PaymentMethod` به همراه قابلیت ثبت تخفیف و مرجع درگاه.
- بازطراحی `EfTenantProvisioningService` برای استفاده از سرویس قیمت‌گذاری جدید، صدور تراکنش پرداخت زرین‌پال، نگهداری وضعیت اشتراک معلق و ثبت رکوردهای پرویژنینگ با شناسه‌های اشتراک/پرداخت.
- پیاده‌سازی `SubscriptionPricingService` و کلاینت شبیه‌ساز `ZarinpalSandboxPaymentGatewayClient` به همراه `PaymentGatewayOptions` و ثبت تزریق وابستگی‌ها.
- ایجاد مایگریشن `AddPayments` جهت افزودن جدول `PaymentTransactions` و ستون‌های `SubscriptionId`/`PaymentId` به `TenantProvisionings`.
- نگارش تست واحد `PaymentTransactionTests` و توسعه تست‌های یکپارچه ثبت مستاجر برای سناریوهای آزمایشی و غیرآزمایشی؛ اجرای `dotnet build` و `dotnet test` (۶۱ تست موفق، مدت ۴.۹ ثانیه).
- فرض‌های جاری: کدهای تخفیف ثابت (`WELCOME10=10%`, `SPRING15=15%`, `SUMMER20=20%`) و آدرس بازگشت پیش‌فرض زرین‌پال (`https://eazymenu.ir/payments/callback`) تا زمان دریافت تنظیمات محیطی.

## 2025-09-30 (earlier-0)
- بازطراحی فرایند ثبت مستاجر با پیاده‌سازی `EfTenantProvisioningService` و ذخیره رکوردهای درخواست در جدول `TenantProvisionings`.
- گسترش قرارداد ثبت‌نام (`RegisterTenantCommand`) برای دریافت شماره تماس، آدرس کامل و ترجیحات آزمایشی/کد تخفیف.
- فعال‌سازی خودکار اشتراک اولیه بر اساس `PlanCode` با قیمت‌گذاری پیش‌فرض، دوره آزمایشی ۱۴ روزه و ثبت رکورد پرویژنینگ.
- افزودن مایگریشن `AddTenantProvisioning` و تست یکپارچه ثبت مستاجر با EF Core InMemory جهت تضمین ذخیره‌سازی واقعی.
- اجرای `dotnet build` و `dotnet test` پس از اعمال تغییرات (۵۸ تست موفق).

## 2025-09-30 (earlier-1)
- توسعه `AddInfrastructureServices` برای پذیرش پیکربندی سفارشی DbContext و پشتیبانی از ارائه‌دهنده‌های تستی.
- افزودن تست‌های یکپارچه شعبه (`CreateBranch`, `UpdateBranchWorkingHours`) با استفاده از EF Core InMemory جهت اعتبارسنجی ذخیره‌سازی واقعی.
- اضافه کردن پکیج `Microsoft.EntityFrameworkCore.InMemory` به پروژه تست و اجرای `dotnet build` و `dotnet test` (۵۸ تست موفق).
- به‌روزرسانی `Docs/Todo.md` با معرفی گام بعدی (جایگزینی سرویس ثبت مستاجر با پیاده‌سازی پایگاه‌داده).

## 2025-09-30 (earlier-2)
- تکمیل پیکربندی EF Core برای دامنه مستاجر: نگاشت‌های `Tenant`, `Branch`, `Subscription` به همراه کانورژن Value Objectها و پشتیبانی از کالکشن‌های تملیکی.
- رفع هشدارهای کامپایل و ایجاد `EazyMenuDbContext` با پیاده‌سازی `SaveChangesAsync` سازگار با `IUnitOfWork`.
- اجرای `dotnet ef migrations add InitialCreate --project src/Infrastructure/EazyMenu.Infrastructure.csproj --startup-project src/Presentation/Web/EazyMenu.Web.csproj` و تولید اولین مایگریشن پایگاه‌داده.
- اجرای `dotnet build` و `dotnet test` جهت اطمینان از سلامت راه‌اندازی لایه Persistency.

## 2025-09-30 (earlier-1)
- پیاده‌سازی فرمان و هندلر `ExpireSubscription` برای ثبت انقضای اشتراک فعال به همراه بررسی زمان انقضا.
- افزودن Query `GetActiveSubscription` و DTO `SubscriptionDetailsDto` جهت ارائه جزئیات اشتراک فعال (طرح، وضعیت، قیمت، تخفیف).
- نگارش تست‌های واحد برای فرمان انقضا و Query جدید شامل سناریوهای موفق و خطا؛ اجرای `dotnet test` با موفقیت (۵۶ تست).

## 2025-09-30 (earlier-3)
- اضافه شدن فرمان‌های `SuspendSubscription` و `ReinstateSubscription` برای تعلیق و بازگشت اشتراک فعال به همراه اعتبارسنجی وضعیت فعلی.
- گسترش دامنه با متدهای `SuspendActiveSubscription` و `ReinstateSuspendedSubscription` در اگریگیت مستاجر جهت پشتیبانی از جریان جدید.
- نوشتن تست‌های واحد برای سناریوهای موفق و خطا (نبود مستاجر، نبود اشتراک فعال، بازگشت از حالت غیرمعلق) و اجرای `dotnet test` با موفقیت (۴۸ تست).

## 2025-09-30 (earlier-4)
- پیاده‌سازی فرمان و هندلر `CancelSubscription` برای لغو اشتراک فعال همراه با اعتبارسنجی شناسه مستاجر و وجود اشتراک فعال.
- افزودن تست‌های واحد برای سناریوهای لغو موفق، نبود مستاجر و نبود اشتراک فعال.
- اجرای `dotnet test` با موفقیت (۴۱ تست) پس از افزودن قابلیت لغو.

## 2025-09-30 (earlier-5)
- پیاده‌سازی جریان تمدید اشتراک در لایه Application: فرمان `RenewSubscriptionCommand` و هندلر جدید همراه با بررسی تاریخ‌ها، قیمت و امکان اعمال تخفیف.
- افزودن تست‌های واحد برای سناریوهای موفق، نبود اشتراک فعال و تاریخ شروع نامعتبر با استفاده از انباره درون‌حافظه‌ای.
- اجرای `dotnet test` با موفقیت (۳۸ تست) برای اطمینان از سلامت تغییرات.

## 2025-09-30 (earlier-6)
- تکمیل لایه Application برای مدیریت شعبه و میز: معرفی `ITenantRepository` و پیاده‌سازی فرمان‌های ایجاد/ویرایش/حذف شعبه، بروزرسانی ساعات کاری و مدیریت میزها (افزودن، ویرایش، حذف، خارج‌از-سرویس/بازگشت به سرویس).
- افزودن Queryهای `GetTenantBranches` و `GetBranchDetails` برای بازگرداندن خلاصه و جزئیات شعبه به همراه DTOهای جدید.
- نگارش تست‌های واحد برای تمام هندلرهای جدید و به‌روزرسانی پوشش آزمون‌ها؛ اجرای `dotnet test` با موفقیت (۳۴ تست).

## 2025-09-30 (earlier-7)
- پیاده‌سازی مدیریت میزها در دامنه: `TableId`، کلاس `Table` و متدهای شعبه برای افزودن/ویرایش/خارج‌از-سرویس کردن میزها.
- تکمیل اگریگیت رزرو با تاریخچه وضعیت، رویدادهای دامنه (`ReservationCreated/Confirmed/Cancelled/CheckedIn/NoShow`) و نگاشت به میز مشخص.
- افزودن Value Object های تازه (`ReservationId`, `TableId`) و متد همپوشانی `ScheduleSlot.Overlaps` برای تشخیص تداخل زمان‌ها.
- تعریف سیاست زمان‌بندی پایه (`DefaultReservationSchedulingPolicy`) برای تخصیص میز آزاد بر اساس ظرفیت و بازه زمانی.
- به‌روزرسانی `Docs/DomainModel.md` مطابق مدل جدید رزرو و میزها.
- افزودن تست‌های واحد برای اگریگیت رزرو، مدیریت میز شعبه و سیاست زمان‌بندی به همراه اجرای `dotnet test` جهت اطمینان از عبور همه سناریوها.

## 2025-09-30 (earlier-8)
- Structured the domain foundation with `ValueObject` and `Entity` base abstractions plus a shared `DomainException` for guard clauses.
- Added rich value objects (`Money`, `Address`, `Email`, `PhoneNumber`, `BrandProfile`, `Percentage`, `ScheduleSlot`, `QrCodeReference`) aligned with the DomainModel specification.
- Implemented the Tenant aggregate (Tenant, Branch, Subscription, enums) complete with domain events for registration and subscription activation.
- Ran `dotnet build` on the full solution to confirm the domain enhancements compile cleanly.

## 2025-10-01 (latest)
- پیاده‌سازی احراز هویت پیامکی مشتریان در لایه Application با معرفی سرویس‌های `IOneTimePasswordGenerator`/`IOneTimePasswordStore` و فرمان‌های `RequestCustomerLogin` و `VerifyCustomerLogin`.
- ایجاد پیاده‌سازی زیرساختی مبتنی بر حافظه (`InMemoryOneTimePasswordStore`) و ارسال پیامک لاگ محور (`LoggingSmsSender`) به همراه تولیدکننده کد تصادفی.
- افزودن کنترلر `AuthController`، مدل‌های `RequestLoginViewModel` و `VerifyLoginViewModel` و صفحات Razor (`Auth/Login`, `Auth/Verify`) در پروژه عمومی برای ورود صرفاً با SMS؛ پیکربندی کوکی احراز هویت و دکمه خروج در ناوبری.
- نگارش تست‌های واحد `CustomerSmsLoginTests` برای فرمان‌های جدید و اجرای `dotnet build` و `dotnet test` (۷۳ تست موفق، مدت ۲.۸ ثانیه) جهت اطمینان از سلامت جریان.
- گام بعدی: اتصال به ارائه‌دهنده پیامک واقعی (کاوه‌نگار) و تکمیل تجربه مشتری (داشبورد شخصی، سفارش سریع بدون ثبت‌نام تکراری).

## 2025-10-01 (earlier-1)
- راه‌اندازی ویزارد آنبوردینگ در لایه Presentation با کنترلر `OnboardingController`، مدل‌های `RegisterTenantViewModel`/`ProvisioningSuccessViewModel` و صفحات Razor (`Onboarding/Start`, `Onboarding/Success`) جهت ثبت مستاجر و جمع‌آوری اطلاعات تماس، آدرس و طرح انتخابی.
- اتصال فرم آنبوردینگ به Use Case `RegisterTenantCommand` و مدیریت سناریوهای آزمایشی و پرداختی همراه با پیام‌های فارسی و ریدایرکت خودکار به درگاه زرین‌پال یا نمایش نتیجه فعال‌سازی اشتراک.
- افزودن تست‌های واحد `OnboardingControllerTests` برای پوشش مسیرهای موفق، اعتبارسنجی ModelState و ریدایرکت پرداخت.
- به‌روزرسانی DI جهت ثبت Handlerها با `ICommandHandler<TCommand, TResult>` و هم‌راست‌سازی کنترلرهای پرداخت/آنبوردینگ با این الگو؛ اجرای `dotnet build` و `dotnet test` (۶۸ تست موفق، مدت ۲.۵ ثانیه).
- گام بعدی: تکمیل صفحات مدیریتی پس از ورود (داشبورد، مدیریت منو) و آماده‌سازی جریان احراز هویت و ارتباطات بیرونی.
## 2025-10-01 (earlier-2)
## 2025-09-29 (earlier)
- Scaffolded the Clean Architecture solution: Domain, Application, Infrastructure, Presentation (Web/Public), and `tests` projects.
- Implemented initial domain primitives (`TenantId`, `Money`, `Restaurant`, `MenuCategory`, `MenuItem`) and a sample onboarding use case with an in-memory provisioning service.
- Created architecture overview documentation plus README with structure, commands, and domain context (`eazymenu.ir`).
- Replaced template tests with unit/integration tests covering value objects and the onboarding command flow.
## 2025-10-01 (earlier-3)
