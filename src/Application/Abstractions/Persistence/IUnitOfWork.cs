using System.Threading;
using System.Threading.Tasks;

namespace EazyMenu.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
