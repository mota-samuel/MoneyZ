using MoneyZ.Domain.Entities;
using MoneyZ.Domain.Objects;

namespace MoneyZ.Domain.Interfaces.Repository;
public interface IUserRepository
{
    Task<User?> GetByTelephone(Telefone telephone, CancellationToken ct);
    Task<User?> GetById(Guid id, CancellationToken ct);
    Task Add(User user, CancellationToken ct);
    Task Update(User user, CancellationToken ct);
}
