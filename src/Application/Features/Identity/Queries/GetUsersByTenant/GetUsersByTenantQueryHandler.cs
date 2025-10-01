using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EazyMenu.Application.Abstractions.Messaging;
using EazyMenu.Application.Common.Interfaces.Identity;
using EazyMenu.Application.Features.Identity.Login;
using EazyMenu.Domain.ValueObjects;

namespace EazyMenu.Application.Features.Identity.Queries.GetUsersByTenant;

public sealed class GetUsersByTenantQueryHandler : IQueryHandler<GetUsersByTenantQuery, List<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersByTenantQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<UserDto>> HandleAsync(
        GetUsersByTenantQuery query,
        CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetByTenantIdAsync(
            TenantId.FromGuid(query.TenantId),
            cancellationToken);

        return users
            .Select(user => new UserDto(
                user.Id.Value,
                user.TenantId.Value,
                user.Email,
                user.FullName,
                user.PhoneNumber.Value,
                user.Role,
                user.Status))
            .ToList();
    }
}
