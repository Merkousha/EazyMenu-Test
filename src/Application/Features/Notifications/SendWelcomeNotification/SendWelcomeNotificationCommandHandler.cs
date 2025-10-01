using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Interfaces.Notifications;
using EazyMenu.Application.Common.Notifications;
using EazyMenu.Domain.Aggregates.Tenants;
using Microsoft.Extensions.Logging;

namespace EazyMenu.Application.Features.Notifications.SendWelcomeNotification;

public sealed class SendWelcomeNotificationCommandHandler : ICommandHandler<SendWelcomeNotificationCommand>
{
    private readonly ISmsSender _smsSender;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<SendWelcomeNotificationCommandHandler> _logger;

    public SendWelcomeNotificationCommandHandler(
        ISmsSender smsSender,
        IEmailSender emailSender,
        ILogger<SendWelcomeNotificationCommandHandler> logger)
    {
        _smsSender = smsSender;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<Unit> HandleAsync(SendWelcomeNotificationCommand command, CancellationToken cancellationToken = default)
    {
        // Send Welcome SMS
        await SendWelcomeSmsAsync(command, cancellationToken);

        // Send Welcome Email
        await SendWelcomeEmailAsync(command, cancellationToken);

        return Unit.Value;
    }

    private async Task SendWelcomeSmsAsync(SendWelcomeNotificationCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var persianCulture = new CultureInfo("fa-IR");
            var endDatePersian = command.SubscriptionEndDate.ToString("yyyy/MM/dd", persianCulture);
            
            var trialText = command.IsTrial ? "آزمایشی " : "";
            
            var message = $"به ایزی‌منو خوش آمدید!\n" +
                         $"{command.RestaurantName} عزیز، ثبت‌نام شما با موفقیت انجام شد.\n" +
                         $"پلن {trialText}{command.PlanName} تا {endDatePersian} فعال است.\n" +
                         $"از همراهی شما سپاسگزاریم.";

            var context = new SmsSendContext(
                command.TenantId,
                SubscriptionPlan.Starter); // TODO: Pass actual plan from command

            await _smsSender.SendAsync(command.ManagerPhone, message, context, cancellationToken);
            
            _logger.LogInformation(
                "Welcome SMS sent successfully to tenant {TenantId} at phone {Phone}",
                command.TenantId,
                command.ManagerPhone);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send welcome SMS to tenant {TenantId} at phone {Phone}",
                command.TenantId,
                command.ManagerPhone);
            // Don't throw - SMS failure should not prevent registration completion
        }
    }

    private async Task SendWelcomeEmailAsync(SendWelcomeNotificationCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var subject = $"خوش آمدید به ایزی‌منو - {command.RestaurantName}";
            
            var trialText = command.IsTrial ? "آزمایشی " : "";
            var persianCulture = new CultureInfo("fa-IR");
            var endDatePersian = command.SubscriptionEndDate.ToString("yyyy/MM/dd", persianCulture);
            
            var body = $@"
<!DOCTYPE html>
<html dir=""rtl"" lang=""fa"">
<head>
    <meta charset=""UTF-8"">
    <style>
        body {{ font-family: Tahoma, Arial, sans-serif; background-color: #f4f4f4; padding: 20px; }}
        .container {{ background-color: white; max-width: 600px; margin: 0 auto; padding: 30px; border-radius: 10px; }}
        .header {{ background-color: #0d6efd; color: white; padding: 20px; border-radius: 5px; text-align: center; }}
        .content {{ padding: 20px; line-height: 1.8; }}
        .footer {{ text-align: center; color: #666; font-size: 12px; padding: 20px; }}
        .info-box {{ background-color: #e7f3ff; padding: 15px; border-right: 4px solid #0d6efd; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>به ایزی‌منو خوش آمدید!</h1>
        </div>
        <div class=""content"">
            <p>سلام <strong>{command.RestaurantName}</strong> عزیز،</p>
            
            <p>ثبت‌نام شما در پلتفرم ایزی‌منو با موفقیت انجام شد. از اینکه ما را برای دیجیتالی کردن کسب‌وکار خود انتخاب کردید، سپاسگزاریم.</p>
            
            <div class=""info-box"">
                <strong>اطلاعات اشتراک شما:</strong><br/>
                پلن: <strong>{trialText}{command.PlanName}</strong><br/>
                اعتبار تا: <strong>{endDatePersian}</strong>
            </div>
            
            <h3>مراحل بعدی:</h3>
            <ol>
                <li>وارد داشبورد مدیریتی خود شوید</li>
                <li>منوی دیجیتال رستوران خود را ایجاد کنید</li>
                <li>QR Code اختصاصی را دانلود و چاپ کنید</li>
                <li>شروع به دریافت سفارش‌ها و رزرو میز کنید</li>
            </ol>
            
            <p>در صورت هرگونه سوال یا نیاز به راهنمایی، تیم پشتیبانی ما آماده کمک به شماست.</p>
            
            <p>موفق و پیروز باشید،<br/>
            <strong>تیم ایزی‌منو</strong></p>
        </div>
        <div class=""footer"">
            <p>این ایمیل به صورت خودکار ارسال شده است.</p>
            <p>© 2025 EazyMenu - تمامی حقوق محفوظ است</p>
        </div>
    </div>
</body>
</html>";

            await _emailSender.SendAsync(
                command.ManagerEmail,
                subject,
                body,
                cancellationToken);
            
            _logger.LogInformation(
                "Welcome email sent successfully to tenant {TenantId} at email {Email}",
                command.TenantId,
                command.ManagerEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send welcome email to tenant {TenantId} at email {Email}",
                command.TenantId,
                command.ManagerEmail);
            // Don't throw - Email failure should not prevent registration completion
        }
    }
}
