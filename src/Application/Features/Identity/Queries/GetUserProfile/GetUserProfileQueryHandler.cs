using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Exceptions;
using EazyMenu.Application.Common.Interfaces.Identity;
using EazyMenu.Application.Features.Identity.Login;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Identity.Queries.GetUserProfile;

public sealed class GetUserProfileQueryHandler : IQueryHandler<GetUserProfileQuery, UserDto>
{
    private readonly IUserRepository _userRepository;

    public GetUserProfileQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> HandleAsync(GetUserProfileQuery query, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(UserId.From(query.UserId), cancellationToken);
        
        if (user is null)
        {
            throw new NotFoundException($"کاربر با شناسه {query.UserId} یافت نشد.");
        }

        return new UserDto(
            user.Id.Value,
            user.TenantId.Value,
            user.Email,
            user.FullName,
            user.PhoneNumber.Value,
            user.Role,
            user.Status);
    }
}
