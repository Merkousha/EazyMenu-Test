using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Abstractions.Persistence;
using EazyMenu.Application.Common.Interfaces.Identity;

namespace EazyMenu.Application.Features.Identity.Login;

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoginResult> HandleAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        // یافتن کاربر با ایمیل
        var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);
        
        if (user is null)
        {
            return new LoginResult(
                IsSuccessful: false,
                AccessToken: null,
                RefreshToken: null,
                User: null,
                ErrorMessage: "ایمیل یا رمز عبور اشتباه است.");
        }

        // بررسی وضعیت کاربر
        if (!user.CanLogin())
        {
            return new LoginResult(
                IsSuccessful: false,
                AccessToken: null,
                RefreshToken: null,
                User: null,
                ErrorMessage: user.Status switch
                {
                    Domain.Aggregates.Users.UserStatus.Inactive => "حساب کاربری شما غیرفعال شده است.",
                    Domain.Aggregates.Users.UserStatus.Blocked => "حساب کاربری شما مسدود شده است.",
                    _ => "امکان ورود به سیستم وجود ندارد."
                });
        }

        // اعتبارسنجی رمز عبور
        var isPasswordValid = _passwordHasher.VerifyPassword(
            user.PasswordHash.HashedValue,
            command.Password);

        if (!isPasswordValid)
        {
            return new LoginResult(
                IsSuccessful: false,
                AccessToken: null,
                RefreshToken: null,
                User: null,
                ErrorMessage: "ایمیل یا رمز عبور اشتباه است.");
        }

        // ثبت زمان ورود
        user.RecordLogin();
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // تولید توکن‌ها
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(
            user.Id.Value,
            user.TenantId.Value,
            user.Email,
            user.Role);

        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        // ایجاد DTO کاربر
        var userDto = new UserDto(
            user.Id.Value,
            user.TenantId.Value,
            user.Email,
            user.FullName,
            user.PhoneNumber.Value,
            user.Role,
            user.Status);

        return new LoginResult(
            IsSuccessful: true,
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            User: userDto,
            ErrorMessage: null);
    }
}
