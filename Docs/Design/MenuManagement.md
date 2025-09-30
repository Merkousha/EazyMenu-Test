# طراحی حوزه مدیریت منو

## مقدمه
برای تحقق فیچر «مدیریت منوی دیجیتال» مطابق PRD و User Story 4، لازم است ساختار دامنه‌ای منسجم، Use Caseهای لایه Application، نگاشت پایگاه‌داده، و رابط مدیریتی مبتنی بر MVC طراحی شود. این سند دامنه MVP و مسیر توسعه تدریجی را مشخص می‌کند تا تیم‌ها با درک مشترک پیش بروند.

## اهداف و دامنه اولیه
- فراهم‌کردن امکان تعریف چند منو (مثلاً ناهار، شام، کمپین فصلی) در سطح هر مستاجر.
- پشتیبانی از دسته‌بندی، آیتم، قیمت‌گذاری (تفکیک کانال حضوری/آنلاین)، وضعیت موجودی و برچسب‌های رژیمی.
- انتشار سریع تغییرات (کمتر از ۶۰ ثانیه) به وب‌سایت عمومی و QR.
- فراهم‌سازی API و UI قابل‌اعتماد برای CRUD کامل منو و مرتب‌سازی دستی.
- ایجاد بستر چندزبانه و نسخه‌بندی که در مراحل بعدی قابل‌گسترش باشد.

### خارج از محدوده MVP (Backlog)
- تاریخچه نسخه‌های منو و زمان‌بندی انتشار.
- موجودی همگام با انبار یا POS بیرونی.
- گزارش تحلیلی پرفروش/کم‌فروش (با تکیه بر داده سفارش‌ها در فاز بعد).
- قیمت‌گذاری پویا بر اساس زمان یا کمپین‌ها.

## مفروضات کلیدی
- هر مستاجر دست‌کم یک منو فعال دارد؛ شاخه‌ها (Branch) می‌توانند منوی متفاوت انتخاب کنند.
- زبان پیش‌فرض فارسی است؛ از Value Object `LocalizedText` برای پشتیبانی آینده انگلیسی/عربی استفاده می‌کنیم.
- مسیر «Quick Update» قیمت/موجودی از طریق API سبک (JSON) انجام می‌شود و با SignalR یا In-Memory cache هم‌زمان می‌گردد.
- محدودیت تعداد دسته/آیتم در MVP ۱۰۰ دسته و ۱۰۰۰ آیتم به‌ازای هر منو است.
- تصاویر آیتم در Blob Storage مدیریت می‌شوند؛ در MVP تنها آدرس (URL) ذخیره می‌کنیم.

## مدل دامنه پیشنهادی
```
Tenant (Aggregate Root)
 └── Menu (Aggregate Root وابسته به Tenant)
      ├── MenuCategory (Entity)
      │    └── MenuItem (Entity)
      └── MenuPublication (Value Object)
```

### Menu Aggregate Root
- فیلدها: `MenuId`, `TenantId`, `Name (LocalizedText)`, `Description (LocalizedText?)`, `IsDefault`, `IsArchived`, `CreatedAtUtc`, `UpdatedAtUtc`, `PublishedSnapshotVersion`.
- رفتارها:
  - `CreateMenu`, `RenameMenu`, `ArchiveMenu`, `SetAsDefaultForBranch(branchId)`.
  - `Publish()` که باعث ایجاد رویداد `MenuPublishedDomainEvent` شود (برای همگام‌سازی با وب‌سایت).
  - مدیریت ترتیب (drag & drop) با متد `ReorderCategories`.
- قوانین:
  - حداکثر یک منو پیش‌فرض برای هر شعبه.
  - دسته‌ها باید `DisplayOrder` یکتا درون منو داشته باشند.

### MenuCategory Entity
- فیلدها: `MenuCategoryId`, `Name (LocalizedText)`, `DisplayOrder`, `IconUrl`, `IsArchived`.
- رفتارها: `Rename`, `ChangeOrder`, `Archive`, `Restore`, `AddItem`, `RemoveItem`, `ReorderItems`.
- قوانین:
  - حداقل یک آیتم فعال برای انتشار.
  - حداکثر ۳ سطح translation (fa-IR، en-US، ar-SA) در MVP.

### MenuItem Entity
- فیلدها: `MenuItemId`, `Name (LocalizedText)`, `Description (LocalizedText?)`, `BasePrice (Money)`, `ChannelPrices (Dictionary<MenuChannel, Money>)`, `IsAvailable`, `Inventory (InventoryState)`, `Tags (HashSet<MenuTag>)`, `ImageUrl`, `DisplayOrder`.
- `InventoryState` به‌صورت Value Object با حالت‌های `Infinite`, `Track(quantity, threshold)`.
- رفتارها: `UpdateDetails`, `UpdateDescription`, `SetAvailability`, `AdjustInventory`, `AssignTag`, `RemoveTag`, `UpdateChannelPrice`, `Reorder`.
- قوانین:
  - حداقل یک قیمت (Base یا کانال پیش‌فرض Online) باید تنظیم شود.
  - `Quantity` نمی‌تواند منفی شود؛ اگر به ۰ رسید `IsAvailable=false`.

### Value Objects جدید موردنیاز
- `MenuId`, `MenuCategoryId`, `MenuItemId` (Guid-based).
- `LocalizedText` (لغت‌نامه CultureCode→string) با حداکثر ۳۲ کاراکتر برای نام و ۴۰۰۰ کاراکتر برای توضیح.
- `MenuChannel` (Enum: `DineIn`, `TakeAway`, `Delivery`, `OnlineExclusive`).
- `MenuTag` (Enum/VO با مقادیر `Vegetarian`, `Vegan`, `GlutenFree`, `Spicy`, `Popular`).
- `InventoryState` (Record struct با Pattern Matching).

### رویدادهای دامنه‌ای
- `MenuPublishedDomainEvent`
- `MenuCategoryReorderedDomainEvent`
- `MenuItemAvailabilityChangedDomainEvent`
- `MenuItemPriceChangedDomainEvent`

## لایه Application
### Use Caseها (CQRS)
| نوع | سناریو | توضیح |
| --- | --- | --- |
| Command | `CreateMenuCommand` | ایجاد منو جدید برای مستاجر (اختیاری: کلون از منوی موجود).
| Command | `UpdateMenuMetadataCommand` | ویرایش نام/توضیح منو و وضعیت پیش‌فرض.
| Command | `ArchiveMenuCommand` | آرشیو کردن منو و انتقال شعب به منوی جایگزین.
| Command | `AddMenuCategoryCommand` | افزودن دسته با ترتیب اولیه.
| Command | `UpdateMenuCategoryCommand` | ویرایش نام، ترتیب و آیکون.
| Command | `ArchiveMenuCategoryCommand` | غیرفعال‌سازی دسته (انتقال آیتم‌ها به دسته دیگر اختیاری).
| Command | `ReorderMenuCategoriesCommand` | مرتب‌سازی دستی با لیست Guid.
| Command | `AddMenuItemCommand` | ایجاد آیتم در یک دسته.
| Command | `UpdateMenuItemCommand` | تغییر نام، توضیح، تصویر، قیمت‌ها و برچسب‌ها.
| Command | `SetMenuItemAvailabilityCommand` | روشن/خاموش کردن موجودی.
| Command | `AdjustMenuItemInventoryCommand` | کم/زیاد کردن موجودی پیگیری‌شده.
| Command | `ReorderMenuItemsCommand` | مرتب‌سازی آیتم‌های دسته.
| Command | `PublishMenuCommand` | تولید Snapshot و انتشار رویداد برای لایه Presentation/Public.
| Query | `GetMenusQuery` | لیست منوهای مستاجر برای داشبورد.
| Query | `GetMenuDetailsQuery` | بازگرداندن منو با دسته‌ها و آیتم‌ها برای صفحه ویرایش.
| Query | `GetMenuQuickEditQuery` | DTO سبک برای مسیر Quick Update قیمت/موجودی.
| Query | `GetBranchMenuProjectionQuery` | داده آماده جهت سایت عمومی و QR.

### قراردادهای اشتراکی
- `IMenuRepository` (Aggregate persistence + شامل روش `GetByIdWithDetailsAsync`).
- `IMenuReadRepository` یا `IMenuReadModel` برای پروجکشن سبک (ممکن است از EF Core یا Elastic آینده استفاده شود).
- `IMenuPublicationService` جهت ذخیره Snapshot در Cache/Blob.
- Validatorها با FluentValidation برای جلوگیری از داده نامعتبر (طول رشته‌ها، قیمت ≥ ۰، فرهنگ معتبر).

### جریان Quick Update
1. فرانت‌اند با API `/dashboard/menu/{menuId}/quick-update` آیتم‌ها را دریافت می‌کند.
2. تغییر قیمت یا موجودی با `PATCH` به `/dashboard/menu/items/{itemId}` ارسال می‌شود.
3. Handler `SetMenuItemAvailabilityCommand` یا `UpdateMenuItemPriceCommand` اجرا شده و بلافاصله رویداد `MenuItemAvailabilityChanged` را منتشر می‌کند.
4. Background Handler (`MenuPublicationHandler`) منو را مجدداً منتشر می‌کند و Cache/Websocket را تازه می‌نماید.

## لایه زیرساخت (Persistence)
### جداول پیشنهادی
- `Menus` (Id, TenantId, NameFa, NameEn, NameAr, DescriptionFa, ..., IsDefault, IsArchived, CreatedAt, UpdatedAt, PublishedVersion).
- `MenuCategories` (Id, MenuId, DisplayOrder, NameFa, ..., IconUrl, IsArchived).
- `MenuItems` (Id, CategoryId, DisplayOrder, NameFa, ..., DescriptionFa, ..., BasePriceAmount, BasePriceCurrency, IsAvailable, ImageUrl, InventoryType, InventoryQuantity, InventoryThreshold).
- `MenuItemChannelPrices` (Id, MenuItemId, Channel, Amount, Currency).
- `MenuItemTags` (MenuItemId, Tag) — جدول چند به چند ساده.
- ایندکس‌ها:
  - `(TenantId, IsDefault)` برای بازیابی سریع منوی فعال.
  - `(MenuId, DisplayOrder)` و `(CategoryId, DisplayOrder)` برای مرتب‌سازی.
  - فیلتر `IsArchived=0`.

### نگاشت EF Core
- استفاده از Owned Type برای `Money` (Amount decimal(18,2) + Currency char(3)).
- استفاده از `JsonConversion` (SQL Server 2022) برای `LocalizedText` (ستون NVARCHAR(MAX)) یا ایجاد ستون‌های جداگانه؛ در MVP ستون مجزا برای حداکثر ۳ زبان انتخاب می‌شود تا پرس‌و‌جو ساده بماند.
- `InventoryState` به‌صورت Discriminator: دو ستون `InventoryType` (TinyInt) و `InventoryQuantity`/`InventoryThreshold`.
- فعال‌سازی `OwnedNavigationBuilder` برای لیست آیتم‌ها با `PropertyAccessMode.Field` جهت جلوگیری از دورزدن invariantها.
- تمام جداول با `TenantId` به‌عنوان فیلد فیلتر شده برای Multi-Tenancy (قرار دادن فیلتر Global Query Filter).

### انتشار و Cache
- پس از `PublishMenuCommand`, Handler دوم منو را به DTO تبدیل کرده و در Redis/Memory با کلید `menu:{tenantId}:{menuId}:v{version}` ذخیره می‌کند.
- `MenuPublishedDomainEventHandler` در لایه Infrastructure وظیفه بروزرسانی `MenuPublication` جدول و نمایش در وب‌سایت را دارد.

## لایه Presentation (Dashboard)
### صفحات MVC
- `MenusController`
  - `Index` → لیست منوها + CTA ایجاد.
  - `Edit(menuId)` → مدیریت دسته/آیتم با drag & drop (استفاده از Stimulus.js یا SortableJS).
  - `QuickUpdate(menuId)` → modal یا صفحه سبک برای قیمت/موجودی با ارسال AJAX.
- Partialهایی برای فرم دسته و آیتم.
- استفاده از کامپوننت‌های Bootstrap RTL و نمایش پیام‌ها به فارسی.

### APIهای AJAX
| متد | مسیر | توضیح |
| --- | --- | --- |
| GET | `/dashboard/menus/{menuId}/categories` | واکشی دسته‌ها برای جدول مدیریت.
| POST | `/dashboard/menus/{menuId}/categories` | ایجاد دسته؛ بازگرداندن DTO.
| PATCH | `/dashboard/menus/categories/{categoryId}` | بروزرسانی نام/ترتیب.
| DELETE | `/dashboard/menus/categories/{categoryId}` | آرشیو دسته (Soft Delete).
| POST | `/dashboard/menus/categories/{categoryId}/items` | ایجاد آیتم.
| PATCH | `/dashboard/menus/items/{itemId}` | بروزرسانی آیتم (قیمت، توضیح، برچسب‌ها).
| PATCH | `/dashboard/menus/items/{itemId}/availability` | تغییر وضعیت موجودی/فعال.
| PATCH | `/dashboard/menus/items/{itemId}/reorder` | بروزرسانی ترتیب.
| POST | `/dashboard/menus/{menuId}/publish` | انتشار منو.

### همگام‌سازی بلادرنگ
- SignalR Hub `MenuUpdatesHub` که پس از رویداد `MenuPublished` پیام `menuUpdated` به کانال Tenant ارسال می‌کند؛ صفحات عمومی QR با گوش‌دادن به این پیام Refresh می‌شوند.

## وب‌سایت عمومی (Public Site)
- Endpoint بدون احراز هویت: `/menus/{tenantSlug}/{branchSlug}` که از `IMenuReadModel` استفاده می‌کند.
- قالب Razor آماده نمایش دسته‌بندی Accordion + کارت آیتم با قیمت/برچسب.
- قابلیت caching با ETag بر اساس `PublishedVersion`.

## مسیر تست و تضمین کیفیت
- تست واحد برای Aggregateها (Menu, Category, Item) جهت بررسی قوانین ترتیب، قیمت و موجودی.
- تست واحد Handlerها با Mock Repository (`IMenuRepository`).
- تست یکپارچه EF Core با پایگاه InMemory/SQLite برای نگاشت.
- تست UI (Playwright) برای سناریوهای CRUD پایه و Quick Update.
- بارانداز (Smoke) برای انتشار منو و بروزرسانی فوری Cache.

## سنجش عملکرد و قیود
- انتشار منو باید زیر ۵۰۰ms روی سرور انجام شود (با Cache داخلی).
- Quick Update باید کمتر از ۲ درخواست به پایگاه‌داده داشته باشد.
- اندازه پاسخ JSON Quick Update < 200KB برای منوهای بزرگ.

## نقشه راه پیشنهادی
1. **فاز ۱ - دامنه و Persistence**
   - ایجاد Value Object‌ها، Aggregate، Repository و Migration.
   - تست‌های واحد دامنه + تست یکپارچه EF.
2. **فاز ۲ - Use Caseها و API**
   - پیاده‌سازی Command/Query Handlerها + ثبت در DI.
   - افزودن مسیرهای API برای داشبورد.
3. **فاز ۳ - UI مدیریتی**
   - ساخت Razor Pages/Views، اجزای JS برای drag & drop و Quick Update.
   - افزودن Validation سمت کلاینت و پیام‌های فارسی.
4. **فاز ۴ - انتشار و همگام‌سازی**
   - افزودن Handler رویداد `MenuPublished`، Cache، SignalR Hub و ETag.
   - تست بار سبک برای تایید انتشار سریع.
5. **فاز ۵ - بهبودها**
   - پشتیبانی از گزارش پرفروش/کم‌فروش (پس از تکمیل سفارش‌ها).
   - اتصال به سیستم‌های بیرونی موجودی در صورت نیاز.

## شاخص‌های پذیرش MVP
- مدیر قادر است منو، دسته و آیتم را ایجاد/ویرایش/حذف کند و نتیجه را در وب‌سایت کمتر از ۶۰ ثانیه ببیند.
- مرتب‌سازی دستی در UI عمل می‌کند و ترتیب در اسنپ‌شات منتشرشده لحاظ می‌شود.
- قیمت‌ها از نوع `Money` با واحد تومان ذخیره و بازخوانی می‌شود.
- موجودی در صورت صفر شدن، آیتم را به‌طور خودکار غیرفعال می‌کند.
- تست‌های واحد/یکپارچه جدید اضافه شده و `dotnet build`, `dotnet test` بدون خطا اجرا می‌شوند.

## ریسک‌ها و اقدامات تکمیلی
- **چندزبانه بودن داده‌ها**: در MVP ستون‌های مجزا ذخیره می‌شود؛ باید Migration با پیشوند زبان داشته باشیم تا بعدها به JSON تبدیل شود.
- **مرتب‌سازی drag & drop**: نیازمند Endpoint اختصاصی و قفل Optimistic برای جلوگیری از از دست رفتن ترتیب.
- **موجودی**: در صورت ورود سفارش آنلاین در آینده باید سازوکار کاهش موجودی هماهنگ شود.
- **Caching**: اطمینان از این‌که انتشار جدید نسخه قبلی را باطل می‌کند.

## خروجی‌های موردنیاز برای شروع توسعه
- ایجاد پوشه `Domain/Aggregates/Menus` و انتقال موجودیت‌های پایه از `Domain/Entities`.
- تعریف `IMenuRepository` و DTOهای Query در لایه Application.
- ساخت Migration `AddMenus`.
- آماده‌سازی Controller و ViewModelهای اولیه در لایه Presentation.
- به‌روزرسانی `Docs/Todo.md` برای ردیابی پیشرفت و نگارش Progress Log پس از هر فاز.
