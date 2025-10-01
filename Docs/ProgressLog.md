# Progress Log

A running history of significant work completed in this repository.

## How to Use
- Append new entries to the top of the file with the current date so the latest progress stays visible.
- Summarize what was finished, notable commands/tests that ran, and any follow-up actions.
- Reference related tasks in `Docs/Todo.md` when closing items.

## 2025-10-10 (signalr)
- پیاده‌سازی کامل SignalR برای اعلان بلادرنگ سفارش‌های جدید به داشبورد مدیریتی شامل OrderAlertsHub و SignalROrderNotifier.
- ایجاد infrastructure لازم با استفاده از IOrderRealtimeNotifier موجود و OrderRealtimeNotification record برای انتشار رویدادهای سفارش.
- اتصال PlaceOrderCommandHandler به SignalR برای ارسال اعلان "order-created" پس از ثبت موفق سفارش با TenantId، OrderId، OrderNumber و TotalAmount.
- افزودن OrderAlertsHub به Web/Program.cs در endpoint "/hubs/order-alerts" و پیکربندی tenant-based groups.
- پیاده‌سازی SignalR client در Orders/Index.cshtml با قابلیت‌های:
  - اتصال خودکار و عضویت در کانال تنانت
  - دریافت رویدادهای orderCreated، orderConfirmed، orderCompleted، orderCancelled
  - نمایش browser notification (با درخواست permission)
  - پخش صدای اعلان (beep sound)
  - بارگذاری مجدد خودکار صفحه پس از 2 ثانیه
  - fallback alert برای مرورگرهای بدون پشتیبانی Notification API
- اجرای `dotnet build` (موفق، مدت 13.9 ثانیه) و `dotnet test` (موفق، 110 تست، مدت 3.0 ثانیه) جهت اطمینان از سلامت تغییرات.
- گام بعدی: تست کامل جریان (ثبت سفارش از سایت عمومی → دریافت اعلان در داشبورد) و اتصال رویدادهای Confirm/Complete/Cancel به SignalR.

## 2025-10-10 (checkout-flow)
- پیاده‌سازی کامل جریان سبد خرید و checkout برای مشتریان شامل Models (CartItemModel, ShoppingCartModel)، ViewModels (CartViewModel, CheckoutViewModel, OrderConfirmationViewModel).
- ایجاد سرویس سبد خرید مبتنی بر Session (`SessionShoppingCartService`) با قابلیت Add/Remove/Update/Clear items و ذخیره‌سازی MenuContext.
- پیاده‌سازی `CartController` با اکشن‌های AddToCart، RemoveFromCart، UpdateQuantity، ClearCart، Checkout (GET/POST) و OrderConfirmation.
- ساخت سه View کامل: Cart/Index (سبد خرید با جدول آیتم‌ها و خلاصه سفارش)، Cart/Checkout (فرم اطلاعات مشتری و انتخاب نوع تحویل)، Cart/OrderConfirmation (صفحه تأیید سفارش).
- افزودن دکمه "افزودن به سبد" به هر آیتم منوی عمومی با ارسال MenuId و TenantId برای حفظ Context.
- اضافه کردن لینک سبد خرید با آیکون به منوی ناوبری سایت عمومی و افزودن Bootstrap Icons.
- پیکربندی Session middleware در Public/Program.cs با DistributedMemoryCache و ثبت ShoppingCartService در DI.
- اجرای `dotnet build` (موفق، مدت 4.6 ثانیه) و `dotnet test` (موفق، 110 تست، مدت 3.0 ثانیه) جهت اطمینان از سلامت تغییرات.
- گام بعدی: پیاده‌سازی SignalR برای اعلان بلادرنگ سفارش‌های جدید به داشبورد مدیریتی.

## 2025-10-10 (final)
- ساخت کامل UI داشبورد مدیریت سفارش‌ها شامل Razor Views (`Index.cshtml` با جدول، فیلترها و صفحه‌بندی، `Details.cshtml` با جزئیات سفارش و modal لغو).
- افزودن Bootstrap Icons به Layout، ثبت `DashboardOrderViewModelFactory` در DI و تبدیل آن از static به instance-based.
- اتصال کنترلر Orders به ViewModelFactory، اصلاح enum `FulfillmentMethod` از TakeAway به Pickup و افزودن لینک سفارش‌ها به منوی ناوبری.
- اجرای مایگریشن `AddOrders` برای ایجاد جداول Orders و OrderItems در SQL Server (پایگاه EazyMenu با ۸ مایگریشن).
- اجرای `dotnet build` (موفق، مدت 3.6 ثانیه) و `dotnet test` (موفق، 110 تست، مدت 3.2 ثانیه) جهت اطمینان از سلامت تغییرات.
- گام بعدی: اتصال سایت عمومی برای ثبت سفارش مشتریان (Customer Checkout Flow)، سپس پیاده‌سازی SignalR برای اعلان بلادرنگ سفارش‌های جدید به داشبورد.

## 2025-10-10 (later)
- پیاده‌سازی کامل لایه Application برای سفارش‌گیری شامل Commands (PlaceOrder، ConfirmOrder، CompleteOrder، CancelOrder) و Queries (GetOrders، GetOrderDetails) به همراه Handlers و DTOها.
- افزودن `SequentialOrderNumberGenerator` برای تولید شماره سفارش به فرمت `ORD-YYYYMMDD-NNNN` با الگوریتم thread-safe.
- ایجاد لایه Persistence با `OrderConfiguration` (EF Core mapping)، `OrderRepository` و ثبت `DbSet<Order>` در DbContext.
- تولید Migration `AddOrders` برای ایجاد جداول Orders و OrderItems با روابط owned entity و indexهای مناسب.
- ثبت تمام Command/Query Handlerها در DI container و اتصال OrderRepository و OrderNumberGenerator.
- اجرای `dotnet build` (موفق، مدت 6.0 ثانیه) و `dotnet test` (موفق، 110 تست، مدت 7.8 ثانیه) برای اطمینان از سلامت تغییرات.
- گام بعدی: ساخت UI داشبورد مدیریت سفارش‌ها (OrdersController + Views) و جریان checkout مشتری.

## 2025-10-10
- رفع نگاشت `OrderMapper` برای تبدیل شناسه مستاجر به Guid هنگام تولید `OrderSummaryDto` و جلوگیری از بروز استثناهای زمان اجرا در صفحه داشبورد سفارش‌ها.
- به‌روزرسانی رویداد دامنه `OrderCreatedDomainEvent` به رکورد برای هماهنگی با کلاس پایه `DomainEventBase` و ساده‌سازی مقداردهی.
- اجرای `dotnet build` (موفق، مدت 12.1 ثانیه) و `dotnet test` (موفق، 110 تست، مدت 4.9 ثانیه) جهت اطمینان از سلامت تغییرات.
- گام بعدی: ادامه پیاده‌سازی جریان‌های سفارش شامل هندلرهای Application و لایه Persistency.

## 2025-10-09 (later)
- اجرای گام اول بهبود تجربه منوی عمومی با افزودن جستجوی متنی سمت سرور و فیلتر نتایج در ViewModel (`PublicMenuViewModelFactory`) همراه با نگاشت وضعیت جستجو (`SearchTerm`, `HasResults`).
- توسعه کنترلر/ویوی عمومی برای پذیرش پارامتر `q`، حذف فاصله‌های اضافی و نمایش فرم جستجو، حالت بدون نتیجه و لینک پاک‌سازی در `Views/Menus/Index.cshtml`.
- نگارش تست‌های واحد جدید برای کنترلر (`MenusControllerTests`) و کارخانه ViewModel جهت پوشش سناریوهای جستجوی موفق/ناموفق و تضمین پالایش دسته‌ها.
- اجرای `dotnet build` (موفق، مدت 15.9 ثانیه) و `dotnet test` (موفق، 110 تست، مدت 2.4 ثانیه) پس از اعمال تغییرات جهت اطمینان از سلامت کد.
- گام بعدی: بررسی افزودن فیلترهای تکمیلی (بر اساس برچسب/قیمت) و پشتیبانی از انتخاب زبان دوم پس از ارزیابی بازخورد کاربران.

## 2025-10-09
- تکمیل جریان انتشار منو با افزودن DTOهای انتشار (`MenuPublicationDto`)، کارخانه ساخت اسنپ‌شات و قراردادهای `IMenuPublicationWriter/Reader` برای ذخیره نسخه‌های منتشرشده.
- پیاده‌سازی مدل و پیکربندی EF Core (`MenuPublication`)، ریپازیتوری `EfMenuPublicationStore` و مایگریشن `AddMenuPublications` به‌همراه ثبت وابستگی‌ها و Seed اولیه.
- ساخت کنترلر و View عمومی `Menus/Index` با ViewModelهای جدید، نمایش فارسی راست‌به‌چپ و اسکریپت SignalR برای تازه‌سازی فوری پس از انتشار.
- گسترش اعلام رویدادهای بلادرنگ در داشبورد (به‌روزرسانی `PublishMenuCommandHandler` و `MenuCategoriesController`) و افزودن تست‌های واحد برای پوشش سناریوهای اسنپ‌شات و UI عمومی.
- اجرای `dotnet build` (موفق، مدت 5.4 ثانیه) و `dotnet test` (موفق، 108 تست، مدت 3.5 ثانیه) جهت تضمین سلامت تغییرات.
- گام بعدی: بررسی بهبود تجربه کاربری منوی عمومی (جستجو، فیلتر و زبان دوم) پس از جمع‌آوری بازخورد.

## 2025-10-08
- اتصال SignalR به جریان آیتم‌های منو با تزریق `IMenuRealtimeNotifier` در `MenuItemsController` و انتشار رویدادهای `item-created`, `item-updated`, `item-availability`, `item-quick-update`, `item-removed`, `items-reordered` پس از هر فرمان موفق.
- به‌روزرسانی تست واحد `MenuItemsControllerTests` با موک نوتیفایر جدید و اصلاح عدم تطابق امضا پس از افزودن وابستگی.
- اجرای `dotnet build` (موفق، مدت 3.0 ثانیه) و `dotnet test` (موفق، 105 تست، مدت 5.1 ثانیه) برای تضمین سلامت تغییرات.
- گام بعدی: تعمیم اعلان‌های SignalR به `MenuCategoriesController` و سناریوهای انتشار منو برای همگام‌سازی داشبورد و سایت عمومی.

## 2025-09-30
- پیاده‌سازی فرمان تجمیعی `QuickUpdateMenuItemCommand` به همراه متد دامنه `SetMenuItemInventory` برای بروزرسانی همزمان قیمت، موجودی و وضعیت آیتم در یک تراکنش.
- افزودن اکشن‌های `MenusController.QuickUpdate` و `MenuItemsController.QuickUpdate` با ورودی JSON، بازگشت Partial و استفاده از ViewModel موجود برای بارگذاری داده‌های به‌روز.
- ساخت View جدید `Views/Menus/QuickUpdate.cshtml` و Partial `_QuickUpdateTable.cshtml` با اسکریپت اختصاصی `menu-quick-update.js` جهت ارسال درخواست‌های AJAX و به‌روزرسانی لحظه‌ای جدول.
- تکمیل تست‌های واحد برای حوزه (رویداد تغییر موجودی) و لایه Application/Presentation (`QuickUpdateMenuItemCommandHandlerTests`, `MenuItemsControllerTests`) جهت تضمین رفتار مسیر جدید.
- اجرای `dotnet build` (موفق، مدت 15.4 ثانیه) و `dotnet test` (موفق، 105 تست، مدت 2.5 ثانیه) پس از اعمال تغییرات.
- گام بعدی: راه‌اندازی همگام‌سازی بلادرنگ با SignalR و اتصال فرآیند انتشار به سایت عمومی.

## 2025-10-07
- بازنویسی کامل View `Views/Menus/Details.cshtml` برای نمایش ترکیبی دسته‌ها و آیتم‌ها با Partialهای جدید و فرم افزودن دسته، رفع خطاهای Razor و تنظیم namespaceهای لازم.
- هم‌ترازسازی بخش اطلاعات پایه، پیام‌های بدون داده و پوشه اسکریپت‌ها با داده‌های واقعی منو و آماده‌سازی توکن ضدتقلب جهت استفاده در درخواست‌های AJAX آینده.
- پیاده‌سازی کنترلرهای `MenuCategoriesController` و `MenuItemsController` بر پایه کلاس مشترک `MenuDashboardControllerBase` برای پوشش سناریوهای ایجاد/ویرایش/آرشیو، تغییر وضعیت، مرتب‌سازی و انتشار منو با بازگشت Partial جهت بروزرسانی آنی UI.
- ساخت اسکریپت `wwwroot/js/menu-management.js` برای مدیریت فرم‌های AJAX، فراخوانی Endpointهای جدید، نمایش پیام‌های کاربر و پشتیبانی از Drag & Drop با SortableJS.
- افزودن تست‌های واحد `MenuCategoriesControllerTests` و `MenuItemsControllerTests` با استفاده از Moq جهت تضمین رفتار جریان‌های موفق و خطا، به‌روزرسانی پروژه تست برای استفاده از بسته Moq.
- اجرای `dotnet build` (موفق، مدت 3.7 ثانیه) و `dotnet test` (موفق، 102 تست، مدت 5.0 ثانیه) پس از افزودن Endpointها، اسکریپت و تست‌ها به‌منظور تضمین سلامت سراسری.
- گام بعدی: تکمیل مسیر Quick Update قیمت/موجودی و SignalR برای همگام‌سازی لحظه‌ای، به‌همراه انتشار محتوا در سایت عمومی بر اساس نسخه جدید.

## 2025-09-30 (latest)
- افزودن سرویس `DashboardTenantProvider` برای استخراج شناسه مستاجر فعال از پایگاه‌داده و ثبت آن در DI پروژه وب.
- پیاده‌سازی `MenusController` به‌همراه ViewModelهای مدیریت منو جهت نمایش فهرست و جزئیات منو با داده نمونه توسعه.
- ایجاد Viewهای Razor (`Menus/Index`, `Menus/Details`) شامل حالت بدون داده، نمایش چندزبانه و وضعیت موجودی آیتم‌ها.
- به‌روزرسانی ناوبری اصلی و صفحه خانه برای دسترسی سریع به بخش مدیریت منو.
- اجرای `dotnet build` و `dotnet test` (۹۴ تست موفق، مدت ۲٫۴ ثانیه) پس از افزودن UI جدید برای اطمینان از سلامت سیستم.
- گام بعدی: فعال‌سازی عملیات CRUD و ویرایش سریع آیتم‌ها از طریق فرم‌ها و Endpointهای AJAX متکی بر فرمان‌های Application.

## 2025-10-06 (latest)
- پیکربندی کامل EF Core برای اگریگیت منو همراه با نگاشت دسته‌ها و آیتم‌ها به جداول تملیکی و استفاده از ناوبری‌های field-backed برای حفظ قوانین دامنه.
- افزودن و اصلاح کانورترها و ValueComparerهای `LocalizedText` (شامل حالت‌های nullable)، `InventoryState`, قیمت‌های کانال و برچسب‌ها جهت پشتیبانی از ردیابی تغییرات و ذخیره‌سازی JSON.
- ایجاد `MenuRepository` مبتنی بر EF Core، افزودن `DbSet<Menu>` به `EazyMenuDbContext` و ثبت آن در DI برای پاسخ‌گویی به فرمان‌ها و کوئری‌های لایه Application.
- تولید مایگریشن `AddMenus` با روابط کلید خارجی به مستاجر و ایجاد جداول `Menus`, `MenuCategories`, `MenuItems` همراه با شاخص‌های ترتیب.
- پیاده‌سازی `EazyMenuDbContextSeeder` برای تولید داده نمونه (مستاجر نمایشی، دسته‌ها و آیتم‌های منو) و اجرای خودکار آن به‌همراه `Database.MigrateAsync` در پروژه‌های وب هنگام توسعه.
- اجرای `dotnet build` و `dotnet test` (۹۴ تست موفق، مدت ۲٫۶ ثانیه) پس از افزودن فرآیند Seed جهت اطمینان از سلامت زنجیره.
- گام بعدی: اتصال UI مدیریت منو به داده نمونه و تکمیل صفحات رابط کاربری.

## 2025-10-04 (latest)
- پیاده‌سازی Value Object‌های منو (`MenuId`, `MenuCategoryId`, `MenuItemId`, `LocalizedText`, `InventoryState`, `MenuChannel`, `MenuTag`) مطابق سند طراحی.
- ایجاد اگریگیت `Menu` همراه با موجودیت‌های داخلی `MenuCategory` و `MenuItem`، اعمال قوانین ترتیب، موجودی و رویدادهای دامنه جدید.
- تعریف رویدادهای دامنه (`MenuCreated`, `MenuPublished`, `MenuItemAvailabilityChanged`, `MenuItemPriceChanged`, `MenuCategoriesReordered`, `MenuItemsReordered`) و افزودن اینترفیس `IMenuRepository` در لایه Application.
- نگارش تست‌های واحد `MenuTests`, `LocalizedTextTests`, `InventoryStateTests` برای پوشش سناریوهای کلیدی دامنه جدید.
- اجرای `dotnet test` با موفقیت (۹۴ تست، مدت ۳.۲ ثانیه) و به‌روزرسانی `Docs/Todo.md` برای تمرکز روی Use Caseها و لایه Persistency.
- گام بعدی: پیاده‌سازی Use Caseهای Application و نگاشت EF Core/مایگریشن `AddMenus`.

## 2025-10-05
- تکمیل سبد فرمان‌های آیتم منو: افزودن هندلرهای `UpdateMenuItemDetails`, `UpdateMenuItemPricing`, `SetMenuItemAvailability`, `AdjustMenuItemInventory`, `RemoveMenuItem`, `ReorderMenuItems` و `PublishMenu` به‌همراه تعریف رکوردهای فرمان متناظر.
- توسعه اگریگیت `Menu` با متدهای `RemoveMenuItem` و `UpdateMenuItemTags` برای محافظت از مرز دامنه و پشتیبانی از فرمان‌های جدید.
- پیاده‌سازی کوئری‌های `GetMenusQuery` و `GetMenuDetailsQuery` با استفاده از `MenuMapper` و فیلتر دسته‌های آرشیوشده برای مصرف داشبورد مدیریتی.
- ثبت تمام فرمان‌ها و کوئری‌های منو در `DependencyInjection` جهت دسترس‌پذیری از طریق الگوی `ICommandHandler`/`IQueryHandler`.
- اجرای `dotnet build` و `dotnet test` (۹۴ تست موفق، مدت ۲٫۵ ثانیه) پس از اضافه‌شدن فرمان‌ها برای اطمینان از سلامت راهکار.
- گام بعدی: ورود به پیاده‌سازی Persistency منو (EF Core، مپینگ، مایگریشن) و افزودن تست‌های پوششی برای هندلرهای جدید.

## 2025-10-04 (earlier-0)
- تدوین سند طراحی «مدیریت منو» شامل مدل دامنه، Use Caseهای لایه Application، نگاشت پایگاه‌داده، APIهای داشبورد و سناریوهای انتشار در `Docs/Design/MenuManagement.md`.
- به‌روزرسانی `Docs/Todo.md` برای اضافه‌کردن گام‌های پیاده‌سازی دامنه/پایستگی و مشخص کردن مرحله بعد UI.
- در این مرحله فقط مستندسازی انجام شد؛ اجرای `dotnet build` و `dotnet test` لازم نبود.

## 2025-10-03 (latest)
- افزودن متاداده مستاجر (TenantId، SubscriptionPlan) به مسیر کامل ارسال پیامک شامل `SmsDeliveryRecord`، `RequestCustomerLoginCommandHandler` و پیاده‌سازی‌های ارسال/ثبت لاگ.
- معرفی کوئری `GetSmsUsageSummaryQuery`، هندلر آن و خوانشگر `ISmsUsageReader` مبتنی بر EF برای جمع‌آوری آمار ماهانه (ارسال موفق/ناموفق، سهمیه باقی‌مانده).
- ارتقای داشبورد `Notifications/SmsLogs` برای نمایش کارت‌های سهمیه پیامک و ارائه اطلاعات پلن در هشدارهای لحظه‌ای SignalR.
- به‌روزرسانی تست‌های واحد (`GetSmsUsageSummaryQueryHandlerTests`، `CustomerSmsLoginTests`، `NotificationsControllerTests`) مطابق امضاهای جدید و اجرای `dotnet build` و `dotnet test` (۸۳ تست موفق، مدت ۳.۳ ثانیه).
- گام بعدی: افزودن پایش پیشگیرانه (هشدار نزدیک‌شدن به سقف پیامک) و یکپارچه‌سازی کانال‌های اعلان اضافی.

## 2025-10-03 (earlier-0)
- پیاده‌سازی اعلان زنده شکست پیامک با SignalR شامل هاب `SmsAlertsHub`، سرویس `SignalRSmsFailureAlertNotifier` و اتصال آن در DI.
- افزودن اسکریپت SignalR به صفحه `Views/Notifications/SmsLogs` برای نمایش فوری هشدارها و عضویت خودکار داشبورد در گروه.
- به‌روزرسانی `Program.cs` برای فعال‌سازی SignalR و نگارش `FrameworkReference` در زیرساخت جهت دسترسی به هاب.
- اجرای `dotnet build` و `dotnet test` (۸۲ تست موفق، مدت ۲.۶ ثانیه) جهت اطمینان از سلامت زنجیره اعلان.
- گام بعدی: پایش بهره‌برداری و بررسی ارسال اعلان‌های سطح کانال‌های دیگر (مانند پیامک پشتیبان یا بات پیام‌رسان).

## 2025-10-02 (latest)
- راه‌اندازی صفحه داشبورد «گزارش پیامک‌ها» با کنترلر `NotificationsController`، مدل‌های نمایشی جدید و جدول راست‌چین همراه با فیلتر وضعیت و صفحه‌بندی.
- افزودن لینک ناوبری و ثبت `GetSmsDeliveryLogsQueryHandler` در DI برای اتصال UI به لایه Application.
- نگارش تست‌های واحد `NotificationsControllerTests` برای پوشش مسیر موفق و خطای بازیابی؛ تعداد کل آزمون‌ها به ۸۰ رسید.
- اجرای `dotnet build` و `dotnet test` (۸۰ تست موفق، مدت ۲.۷ ثانیه) به‌منظور اطمینان از سلامت تغییرات.
- گام بعدی: پیاده‌سازی fallback ایمیل/اعلان برای پیامک‌های ناموفق و نمایش هشدار در داشبورد.

## 2025-10-02 (earlier-0)
- تکمیل قرارداد خوانش لاگ پیامک با پیاده‌سازی `ISmsDeliveryLogReader` در `EfSmsDeliveryStore` شامل صفحه‌بندی، مرتب‌سازی بر اساس زمان و فیلتر وضعیت.
- ثبت چندگانه‌ی وابستگی‌ها در DI تا هر دو رابط `ISmsDeliveryStore` و `ISmsDeliveryLogReader` از پیاده‌سازی EF استفاده کنند.
- نگارش تست یکپارچه `EfSmsDeliveryStoreTests` با استفاده از پایگاه‌داده InMemory برای اعتبارسنجی ترتیب، صفحه‌بندی و فیلتر وضعیت لاگ‌ها.
- اجرای `dotnet build` و `dotnet test` (۷۸ تست موفق، مدت ۲.۸ ثانیه) برای اطمینان از سلامت تغییرات.
- گام بعدی: نمایش صفحه گزارش پیامک در داشبورد مدیریتی و افزودن فیلترهای UI.

## 2025-10-02 (earlier-1)
- طراحی قرارداد جدید `ISmsDeliveryStore` به همراه مدل‌های `SmsDeliveryRecord` و `SmsDeliveryStatus` برای ثبت وضعیت ارسال پیامک‌ها در لایه Application.
- افزودن موجودیت `SmsDeliveryLog`، پیکربندی EF Core و مایگریشن `AddSmsDeliveryLogs` جهت ذخیره‌سازی نتایج ارسال پیامک در پایگاه‌داده.
- پیاده‌سازی `EfSmsDeliveryStore` و اتصال آن به سرویس‌های `LoggingSmsSender` و `KavenegarSmsSender` با ثبت موفقیت/خطا و مدیریت خطاهای شبکه.
- نگارش تست‌های جدید `LoggingSmsSenderTests` (واحد) و به‌روزرسانی `KavenegarSmsSenderTests` برای پوشش ثبت لاگ‌های ارسال.
- اجرای `dotnet build` و `dotnet test` (۷۷ تست موفق، مدت ۲.۸ ثانیه) برای اعتبارسنجی تغییرات.
- گام بعدی: نمایش گزارش پیامک‌ها در داشبورد و آماده‌سازی fallback ایمیلی هنگام شکست ارسال پیامک.

## 2025-10-02 (earlier-2)
- تکمیل پیکربندی چندارائه‌دهنده پیامک با افزودن `SmsOptions`، `SmsProvider` و ثبت آن‌ها در `DependencyInjection` به‌همراه پیش‌فرض‌های پیکربندی در `appsettings` پروژه‌های وب و عمومی.
- پیاده‌سازی کلاینت `KavenegarSmsSender` مبتنی بر `HttpClient` با مدیریت خطا، ثبت لاگ و اعتبارسنجی ورودی‌ها.
- نگارش تست‌های یکپارچه `KavenegarSmsSenderTests` برای پوشش سناریوهای موفق، نبود کلید API و خطای درگاه با استفاده از هندلر ساختگی.
- اجرای `dotnet build` و `dotnet test` (۷۶ تست موفق، مدت ۳.۲ ثانیه) جهت اطمینان از سلامت تغییرات.
- گام بعدی: دریافت کلیدهای محیطی کاوه‌نگار و پیاده‌سازی fallbackهای پیامک و ایمیل برای سناریوهای خطا.

## 2025-10-02 (latest)
- پیاده‌سازی سرویس `SmsFailureAlertService` برای ارسال ایمیل هشدار به پشتیبانی هنگام شکست پیامک و ثبت لاگ «fallback-email» در پایگاه‌داده.
- افزودن `IEmailSender`، `LoggingEmailSender` و تنظیمات `EmailOptions` به لایه زیرساخت به همراه تزریق وابستگی‌ها.
- به‌روزرسانی `RequestCustomerLoginCommandHandler` برای فعال‌سازی fallback و پوشش تستی سناریوی شکست SMS؛ مجموع آزمون‌ها به ۸۲ رسید (تست واحد + یکپارچه برای fallback).
- ارتقای صفحه گزارش پیامک با هایلایت ردیف‌های fallback و نمایش برچسب کانال جایگزین.
- اجرای `dotnet build` و `dotnet test` (۸۲ تست موفق، مدت ۲.۴ ثانیه) جهت اعتبارسنجی تغییرات.
- گام بعدی: بررسی ارسال اعلان داخلی یا UI real-time برای هشدارهای فوری پس از شکست پیامک.

## 2025-10-02 (earlier-0)
- راه‌اندازی صفحه داشبورد «گزارش پیامک‌ها» با کنترلر `NotificationsController`، مدل‌های نمایشی جدید و جدول راست‌چین همراه با فیلتر وضعیت و صفحه‌بندی.
- افزودن لینک ناوبری و ثبت `GetSmsDeliveryLogsQueryHandler` در DI برای اتصال UI به لایه Application.
- نگارش تست‌های واحد `NotificationsControllerTests` برای پوشش مسیر موفق و خطای بازیابی؛ تعداد کل آزمون‌ها به ۸۰ رسید.
- اجرای `dotnet build` و `dotnet test` (۸۰ تست موفق، مدت ۲.۷ ثانیه) به‌منظور اطمینان از سلامت تغییرات.
- گام بعدی: پیاده‌سازی fallback ایمیل/اعلان برای پیامک‌های ناموفق و نمایش هشدار در داشبورد.

## 2025-10-02 (earlier-1)
- تکمیل جریان بازگشت زرین‌پال در لایه Presentation با پیاده‌سازی View جدید `Views/Payments/Callback.cshtml` و اتصال آن به `PaymentsController` برای نمایش وضعیت پرداخت.
- نمایش پیام‌های موفق/ناموفق همراه با کد پیگیری و شناسه اشتراک در رابط کاربری بر اساس `PaymentCallbackViewModel`.
- اجرای `dotnet build` و `dotnet test` (۶۴ تست موفق، مدت ۳.۴ ثانیه) جهت اطمینان از سلامت تغییرات UI و عدم Regression.
- گام بعدی: تکمیل سایر صفحات MVC و شروع به یکپارچه‌سازی سرویس‌های بیرونی (SMS/Email) پس از نهایی شدن فلوهای پرداخت.

## 2025-10-01 (earlier-2)
## 2025-10-02 (earlier-2)
- توسعه دامنه جهت فعال‌سازی اشتراک‌های در حالت Pending پس از موفقیت پرداخت و نگهداری مرجع تراکنش.
- ارتقای کلاینت شبیه‌ساز زرین‌پال برای پشتیبانی از متد `VerifyPaymentAsync` و سناریوهای انقضا/لغو.
- نگارش تست‌های یکپارچه برای تأیید سناریوهای موفق و ناموفق پرداخت به همراه تقویت تست واحد تراکنش پرداخت؛ اجرای `dotnet test` (۶۴ تست موفق، مدت ۳.۱ ثانیه).
- گام بعدی: پیاده‌سازی سرویس Callback در لایه Presentation جهت دریافت پارامترهای `Authority`/`Status` و فراخوانی Use Case جدید.

## 2025-10-01 (earlier-3)
## 2025-10-02 (earlier-3)
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
