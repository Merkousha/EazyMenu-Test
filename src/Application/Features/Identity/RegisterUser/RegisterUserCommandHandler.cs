using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Interfaces.Identity;
using EazyMenu.Domain.Aggregates.Users;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Identity.RegisterUser;

public sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, UserId>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserId> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        // بررسی تکراری بودن ایمیل
        var emailExists = await _userRepository.ExistsWithEmailAsync(command.Email, cancellationToken);
        if (emailExists)
        {
            throw new BusinessRuleViolationException($"کاربری با ایمیل {command.Email} قبلاً ثبت شده است.");
        }

        // هش کردن رمز عبور
        var hashedPassword = _passwordHasher.HashPassword(command.Password);
        var passwordHash = PasswordHash.Create(hashedPassword);

        // ایجاد کاربر
        var user = User.Create(
            TenantId.FromGuid(command.TenantId),
            command.Email,
            command.FullName,
            PhoneNumber.Create(command.PhoneNumber),
            passwordHash,
            command.Role);

        // ذخیره کاربر
        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
