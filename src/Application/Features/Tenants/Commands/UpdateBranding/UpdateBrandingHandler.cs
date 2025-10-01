using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Tenants.Commands.UpdateBranding;

public sealed class UpdateBrandingHandler
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBrandingHandler(ITenantRepository tenantRepository, IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(UpdateBrandingCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantId.FromGuid(command.TenantId);
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        
        if (tenant == null)
            return Result.Failure($"رستوران با شناسه {command.TenantId} یافت نشد");

        // Update branding
        var updatedBranding = tenant.Branding.Update(
            displayName: command.DisplayName,
            logoUrl: command.LogoUrl,
            primaryColor: command.PrimaryColor,
            secondaryColor: command.SecondaryColor,
            bannerImageUrl: command.BannerImageUrl,
            aboutText: command.AboutText,
            openingHours: command.OpeningHours,
            templateName: command.TemplateName);

        // Handle publish state
        if (command.ShouldPublish.HasValue)
        {
            updatedBranding = command.ShouldPublish.Value 
                ? updatedBranding.Publish() 
                : updatedBranding.Unpublish();
        }

        tenant.UpdateBranding(updatedBranding);

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed record Result
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }

    private Result(bool isSuccess, string? error = null)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true);
    public static Result Failure(string error) => new(false, error);
}
