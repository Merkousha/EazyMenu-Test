# تحلیل جامع پروژه EazyMenu
**تاریخ:** ۱۰ مهر ۱۴۰۴  
**وضعیت:** MVP در حال توسعه

---

## خلاصه اجرایی

پلتفرم **ایزی‌منو** یک راهکار SaaS چندمستاجری است که با معماری Clean Architecture و .NET 9 در حال توسعه است. هدف ارائه منوی دیجیتال، سفارش آنلاین و رزرو میز برای رستوران‌ها و کافه‌هاست.

### پیشرفت کلی: **~55%** از MVP
- ✅ زیرساخت و معماری: **90%** (Complete)
- ✅ احراز هویت و ثبت‌نام: **85%** (نیاز به OAuth/MFA)
- ✅ مدیریت منو: **100%** (Complete with SignalR)
- 🟡 سفارش‌گیری: **15%** (Domain آماده، نیاز به Application/UI)
- 🟡 رزرو میز: **65%** (Domain/Application آماده، نیاز به UI)
- 🟡 پرداخت: **70%** (Zarinpal sandbox آماده، نیاز به production)
- 🟡 پیامک: **80%** (Kavenegar stub، نیاز به API واقعی)
- ❌ QR Code: **0%** (طراحی شده، پیاده‌سازی نشده)
- ❌ گزارش‌ها و داشبورد: **10%** (فقط خلاصه پیامک)

---

## 1️⃣ معماری و زیرساخت

### ✅ انجام شده
- **Clean Architecture** با تفکیک کامل لایه‌ها:
  - `Domain`: Aggregates (Tenant, Menu, Order, Reservation, Payment)
  - `Application`: CQRS با Commands/Queries و Handlers
  - `Infrastructure`: EF Core + SQL Server، پیاده‌سازی Repository
  - `Presentation`: دو پروژه MVC (Web برای داشبورد، Public برای مشتریان)
- **Multi-Tenancy**: مدل tenant-aware با TenantId در تمام Aggregates
- **EF Core Migrations**: 5 مایگریشن (InitialCreate، AddTenantProvisioning، AddPayments، AddSmsDeliveryLogs، AddMenus)
- **Testing**: 110 تست (واحد + یکپارچه) با پوشش Domain و Application
- **DI Container**: ثبت سرویس‌ها با Scrutor

### 📌 نکات کلیدی
- استفاده از **Value Objects** (Money، Address، PhoneNumber، LocalizedText)
- پشتیبانی از **Domain Events** و Dispatcher
- **IUnitOfWork** برای تراکنش‌های Transactional
- **Guard Clauses** و Exception‌های دامنه‌ای
- معماری سازگار با OWASP و Clean Code

---

## 2️⃣ وضعیت فیچرها (بر اساس PRD و User Stories)

### ✅ US1: ثبت‌نام و مدیریت اشتراک (85%)
**انجام شده:**
- ویزارد Onboarding با انتخاب پلن، دوره آزمایشی و کد تخفیف
- ثبت Tenant، Branch، Subscription با قیمت‌گذاری پایه
- جریان پرداخت Zarinpal (sandbox) و فعال‌سازی اشتراک
- ذخیره TenantProvisioning برای تاریخچه

**باقی‌مانده:**
- [ ] ارسال پیامک/ایمیل خوش‌آمدگویی بعد از فعال‌سازی
- [ ] نمایش وضعیت اشتراک و تاریخ انقضا در داشبورد
- [ ] تمدید و ارتقای اشتراک از UI
- [ ] مدیریت تخفیف‌های پویا (کدهای فعلی hardcoded هستند)

---

### ✅ US2: ورود و احراز هویت (85%)
**انجام شده:**
- احراز هویت مشتریان با OTP پیامکی (Public site)
- کوکی احراز هویت و مدیریت نشست
- ثبت لاگ ارسال پیامک (موفق/ناموفق، fallback ایمیل)

**باقی‌مانده:**
- [ ] ورود مدیران رستوران (Dashboard login با ایمیل/پسورد)
- [ ] MFA (Multi-Factor Authentication) اختیاری
- [ ] قفل حساب پس از 3 بار ورود ناموفق
- [ ] OAuth/OIDC برای احراز هویت امن
- [ ] لاگ امنیتی نشست‌ها (login/logout/IP tracking)

---

### 🟡 US3: ساخت وب‌سایت اختصاصی (50%)
**انجام شده:**
- صفحه عمومی منو (`/menus/{tenantSlug}`) با جستجو و فیلتر
- نمایش دسته‌ها و آیتم‌ها با Mobile First design
- همگام‌سازی SignalR پس از انتشار منو

**باقی‌مانده:**
- [ ] ویزارد ساخت وب‌سایت با انتخاب قالب
- [ ] بارگذاری لوگو و شخصی‌سازی رنگ
- [ ] تنظیم دامنه سفارشی یا زیردامنه
- [ ] پیش‌نمایش زنده قبل از انتشار
- [ ] SEO optimization و meta tags

---

### ✅ US4: مدیریت منوی دیجیتال (100%) ⭐
**انجام شده:**
- CRUD کامل برای Menu، Category، MenuItem
- قیمت‌گذاری چندکاناله (حضوری، آنلاین، ارسال)
- مدیریت موجودی (Infinite/Track) و برچسب‌های رژیمی
- Quick Update قیمت/موجودی با AJAX
- چندزبانه با LocalizedText (فارسی پیش‌فرض)
- انتشار منو با Snapshot و SignalR realtime
- Drag & Drop مرتب‌سازی دسته‌ها و آیتم‌ها
- جستجوی سمت سرور با فیلتر نتایج

**نکات:**
- Migration `AddMenus` با 3 جدول (Menus، MenuCategories، MenuItems)
- محدودیت MVP: 100 دسته، 1000 آیتم در هر منو
- تصاویر آیتم‌ها فعلاً URL‌اند (Blob Storage در آینده)

---

### ❌ US5: تولید QR Code (0%)
**وضعیت:** طراحی شده در Domain (QrCodeReference در Branch) اما پیاده‌سازی نشده

**نیازمندی‌ها:**
- [ ] سرویس تولید QR Code (کتابخانه QRCoder)
- [ ] دانلود PNG/SVG از داشبورد
- [ ] ذخیره در Blob Storage یا فایل سیستم
- [ ] لینک QR Code به `/menus/{tenantSlug}/{branchSlug}`
- [ ] رهگیری اسکن‌ها (آمار تاریخ/کمپین)
- [ ] QR های مجزا برای کمپین‌ها

---

### 🟡 US6: سفارش آنلاین (15%)
**انجام شده:**
- Domain layer: `Order` Aggregate با OrderItem، FulfillmentMethod، OrderStatus
- Value Objects: OrderId، OrderItemId
- Domain Events: OrderCreatedDomainEvent
- DTO/Mapper: OrderDetailsDto، OrderSummaryDto

**باقی‌مانده:**
- [ ] Application Commands:
  - [ ] PlaceOrderCommand (ایجاد سفارش از سبد خرید)
  - [ ] UpdateOrderItemCommand (ویرایش آیتم)
  - [ ] ConfirmOrderCommand (تایید توسط رستوران)
  - [ ] CancelOrderCommand (لغو سفارش)
- [ ] Persistence: IOrderRepository + EF Core mapping + Migration `AddOrders`
- [ ] UI مشتری: صفحه سبد خرید، انتخاب روش دریافت، یادداشت
- [ ] UI داشبورد: مدیریت سفارش‌های دریافتی
- [ ] نمایش زمان آماده‌سازی و هزینه ارسال
- [ ] SignalR برای اعلان سفارش جدید به رستوران

---

### 🟡 US7: رزرو میز (65%)
**انجام شده:**
- Domain: `Reservation` Aggregate با وضعیت‌های مختلف (Pending، Confirmed، CheckedIn، NoShow، Cancelled)
- `Table` entity در Branch با ظرفیت و وضعیت OutOfService
- `DefaultReservationSchedulingPolicy` برای تخصیص میز با الگوریتم smart
- Application Commands: ScheduleReservationCommand
- Query: GetReservationsForDayQuery
- تست‌های واحد و یکپارچه

**باقی‌مانده:**
- [ ] UI مشتری: صفحه انتخاب تاریخ/زمان/تعداد نفرات
- [ ] نمایش ظرفیت آزاد در هر بازه زمانی
- [ ] UI داشبورد: تقویم رزروها، تغییر وضعیت
- [ ] Persistence: Migration برای Reservation (فعلاً InMemory در تست‌ها)
- [ ] پیامک تایید/یادآوری رزرو
- [ ] امکان لغو/ویرایش رزرو تا مهلت مشخص
- [ ] جلوگیری از double-booking (همپوشانی زمانی)

---

### 🟡 US8: پرداخت آنلاین (70%)
**انجام شده:**
- `PaymentTransaction` Aggregate با وضعیت‌های مختلف
- یکپارچه‌سازی Zarinpal Sandbox با `ZarinpalSandboxPaymentGatewayClient`
- Callback و تایید پرداخت
- فعال‌سازی اشتراک پس از پرداخت موفق
- Migration `AddPayments`

**باقی‌مانده:**
- [ ] تنظیمات Production Zarinpal (MerchantID واقعی)
- [ ] پشتیبانی از بیعانه (partial payment)
- [ ] صدور رسید پرداخت PDF
- [ ] گزارش تراکنش‌ها در داشبورد
- [ ] مدیریت استرداد وجه (refund)
- [ ] تسویه خودکار با کیف پول

---

### 🟡 US9: ارسال پیامک تراکنشی (80%)
**انجام شده:**
- `ISmsSender` با دو پیاده‌سازی: `LoggingSmsSender` (توسعه)، `KavenegarSmsSender` (stub)
- `SmsDeliveryLog` برای ذخیره وضعیت ارسال
- Fallback ایمیل با `SmsFailureAlertService`
- داشبورد گزارش پیامک با فیلتر وضعیت
- SignalR برای هشدار زنده شکست پیامک
- پایش مصرف پیامک بر اساس پلن اشتراک

**باقی‌مانده:**
- [ ] دریافت API Key واقعی کاوه‌نگار و تنظیم محیطی
- [ ] الگوهای تایید شده پیامک (OTP، تایید سفارش، یادآوری رزرو)
- [ ] ارسال ظرف 10 ثانیه (حداکثر)
- [ ] مانیتورینگ و Retry برای شکست‌های موقت
- [ ] محدودیت سهمیه روزانه بر اساس پلن

---

### ❌ US10: اعلان‌های درون‌سیستمی و ایمیلی (20%)
**انجام شده:**
- SignalR Hub برای منو و پیامک
- `IEmailSender` با `LoggingEmailSender`
- ارسال ایمیل هشدار fallback پیامک

**باقی‌مانده:**
- [ ] مرکز اعلان در داشبورد (bell icon)
- [ ] تاریخچه اعلان‌ها با وضعیت خوانده/نخوانده
- [ ] فیلتر بر اساس نوع (سفارش، رزرو، هشدار)
- [ ] تنظیمات کانال‌های اعلان برای هر مدیر
- [ ] اعلان بحرانی با صدای هشدار
- [ ] پشتیبانی از چند نقش (مدیر، کارمند سالن، صندوق)

---

## 3️⃣ وضعیت فنی

### ✅ پایگاه داده (SQL Server)
**جداول موجود:**
- `Tenants` + `TenantBranches` + `TenantSubscriptions`
- `BranchTables` + `BranchWorkingHours` + `BranchQrCodes`
- `Menus` + `MenuCategories` + `MenuItems` + `MenuItemChannelPrices` + `MenuItemTags`
- `MenuPublications` (snapshot انتشار منو)
- `PaymentTransactions`
- `TenantProvisionings`
- `SmsDeliveryLogs`

**جداول مفقود:**
- [ ] `Orders` + `OrderItems`
- [ ] `Reservations` + `ReservationStatusHistory`
- [ ] `Notifications` (اعلان‌های داخلی)
- [ ] `QrCodeScans` (آمار اسکن)
- [ ] `AuditLogs` (ممیزی امنیتی)

### ✅ سرویس‌های خارجی
| سرویس | وضعیت | نیازمندی |
|--------|--------|----------|
| Zarinpal | Sandbox ✅ | Production MerchantID |
| Kavenegar | Stub ✅ | API Key واقعی |
| Email | Logging ✅ | SMTP واقعی (SendGrid/Mailgun) |
| Blob Storage | ❌ | Azure Blob یا MinIO |
| SignalR | ✅ | - |

### 📊 آمار کد
- **تست‌ها:** 110 (100% passing)
- **Migrations:** 5
- **Domain Events:** 15
- **Aggregates:** 5 (Tenant، Menu، Order، Reservation، Payment)
- **Controllers:** 8 (Web: 5، Public: 3)

---

## 4️⃣ نیازهای فوری (Priority)

### 🔴 High Priority (برای MVP)
1. **تکمیل سفارش‌گیری:**
   - [ ] Application layer commands/queries
   - [ ] Persistence (Migration + Repository)
   - [ ] UI مشتری (سبد خرید، checkout)
   - [ ] داشبورد مدیریت سفارش

2. **UI رزرو میز:**
   - [ ] صفحه رزرو برای مشتریان
   - [ ] داشبورد مدیریت رزروها

3. **QR Code:**
   - [ ] سرویس تولید و ذخیره‌سازی
   - [ ] دانلود از داشبورد

4. **احراز هویت مدیران:**
   - [ ] صفحه ورود داشبورد
   - [ ] مدیریت نقش‌ها

5. **تنظیمات Production:**
   - [ ] Zarinpal/Kavenegar API keys
   - [ ] SMTP واقعی

### 🟡 Medium Priority (بعد از MVP)
- [ ] داشبورد تحلیلی (گزارش فروش، آیتم‌های پرفروش)
- [ ] مرکز اعلان‌های داخلی
- [ ] MFA و OAuth
- [ ] Blob Storage برای تصاویر
- [ ] SEO و بهینه‌سازی صفحه عمومی

### 🟢 Low Priority (فاز 2)
- [ ] تاریخچه نسخه‌های منو
- [ ] قیمت‌گذاری پویا و کمپین‌ها
- [ ] یکپارچه‌سازی POS
- [ ] پشتیبانی چند شعبه پیشرفته
- [ ] API عمومی برای شخص ثالث

---

## 5️⃣ ریسک‌ها و مسدودکننده‌ها

### 🚨 Blockers
1. **کلیدهای API خارجی:** بدون Kavenegar/Zarinpal واقعی، تست production ممکن نیست
2. **ذخیره تصاویر:** بدون Blob Storage، بارگذاری تصویر آیتم‌ها محدود است
3. **احراز هویت مدیران:** داشبورد فعلاً بدون محافظت است

### ⚠️ Technical Debt
- کدهای تخفیف hardcoded (`WELCOME10`، `SPRING15`، `SUMMER20`) باید به DB منتقل شوند
- LocalizedText با ستون‌های مجزا (NameFa، NameEn) باید به JSON تبدیل شود
- بدون Caching لایه بین (Redis)، load بالا می‌تواند مشکل ساز شود
- تست‌های UI (Playwright) هنوز اضافه نشده

---

## 6️⃣ نقشه راه پیشنهادی

### Sprint 1 (2 هفته) - Order Management
- [ ] Implement Application layer (Commands/Queries)
- [ ] Add EF Core mapping + Migration
- [ ] Build customer checkout UI
- [ ] Add dashboard order management
- [ ] Write integration tests

### Sprint 2 (1 هفته) - Reservation UI
- [ ] Customer reservation form
- [ ] Dashboard calendar view
- [ ] SMS/Email notifications
- [ ] Add Persistence layer

### Sprint 3 (1 هفته) - QR + Auth
- [ ] QR Code generation service
- [ ] Dashboard login with Identity
- [ ] Role-based access control
- [ ] Generate QR for each branch

### Sprint 4 (1 هفته) - Production Setup
- [ ] Configure real API keys
- [ ] Setup Blob Storage
- [ ] Deploy to staging
- [ ] Load testing
- [ ] Security audit

### Sprint 5 (1 هفته) - Polish & Launch
- [ ] Analytics dashboard
- [ ] Notification center
- [ ] Documentation
- [ ] Beta testing
- [ ] Go live! 🚀

---

## 7️⃣ معیارهای موفقیت MVP

### Technical
- ✅ Build بدون error
- ✅ 100+ تست با 100% passing
- 🟡 کد coverage > 70% (فعلاً untracked)
- ❌ Load test 10k req/hour
- ❌ Page load < 2s on 4G

### Functional
- ✅ ثبت‌نام مستاجر و فعال‌سازی اشتراک
- ✅ مدیریت منو کامل با انتشار
- 🟡 سفارش آنلاین (در حال پیاده‌سازی)
- 🟡 رزرو میز (Backend آماده)
- 🟡 پرداخت Zarinpal (Sandbox)
- 🟡 پیامک OTP (Stub)
- ❌ QR Code برای هر شعبه

### Business
- ❌ 50 مشتری در 3 ماه اول
- ❌ CSAT > 4/5
- ❌ Conversion rate > 15%
- ❌ Setup time < 30 دقیقه

---

## 8️⃣ جمع‌بندی

### ✅ نقاط قوت
- معماری Clean و مقیاس‌پذیر
- تست‌های جامع
- Multi-tenancy درست پیاده‌سازی شده
- Domain model غنی با رویدادها
- مدیریت منو کامل و عالی

### ⚠️ شکاف‌ها
- سفارش‌گیری 85% ناتمام
- UI رزرو میز صفر
- QR Code هنوز نساخته شده
- گزارش‌ها بسیار محدود
- احراز هویت مدیران مفقود

### 🎯 تمرکز فوری
**باید این 3 کار را زودتر تمام کنیم:**
1. تکمیل جریان سفارش (Application + Persistence + UI)
2. UI رزرو میز (Frontend + Dashboard)
3. QR Code و احراز هویت داشبورد

با تکمیل این موارد، MVP قابل رونمایی خواهد بود.

---

**تاریخ بعدی تحلیل:** پس از Sprint 1 (2 هفته)
