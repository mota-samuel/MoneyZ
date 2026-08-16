using MoneyZ.Domain.Entities;

namespace MoneyZ.Domain.Interfaces.Repository;
public interface IUserRepository
{
    Task<User?> GetByTelephone(string telephone, CancellationToken ct);
    Task<User?> GetById(Guid id, CancellationToken ct);
    Task Add(User user, CancellationToken ct);
    Task Update(User user, CancellationToken ct);
}
