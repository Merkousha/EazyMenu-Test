using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Interfaces.Identity;
using EazyMenu.Domain.Aggregates.Users;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Identity.ChangePassword;

public sealed class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HandleAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default)
    {
        // یافتن کاربر
        var user = await _userRepository.GetByIdAsync(UserId.From(command.UserId), cancellationToken);
        
        if (user is null)
        {
            throw new BusinessRuleViolationException("کاربر یافت نشد.");
        }

        // اعتبارسنجی رمز عبور فعلی
        var isCurrentPasswordValid = _passwordHasher.VerifyPassword(
            user.PasswordHash.HashedValue,
            command.CurrentPassword);

        if (!isCurrentPasswordValid)
        {
            throw new BusinessRuleViolationException("رمز عبور فعلی اشتباه است.");
        }

        // هش کردن رمز عبور جدید
        var newHashedPassword = _passwordHasher.HashPassword(command.NewPassword);
        var newPasswordHash = PasswordHash.Create(newHashedPassword);

        // تغییر رمز عبور
        user.ChangePassword(newPasswordHash);

        // ذخیره تغییرات
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}
