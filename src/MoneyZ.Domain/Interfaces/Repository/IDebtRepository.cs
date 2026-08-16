using MoneyZ.Domain.Entities;

namespace MoneyZ.Domain.Interfaces.Repository;
public interface IDebtRepository
{
    Task<Debt?> GetByID(Guid id, CancellationToken ct);
    Task<IEnumerable<Debt>> ListByUser(Guid userId, CancellationToken ct);
    Task<IEnumerable<Debt>> ListInDebts(Guid userId, CancellationToken ct);
    Task Add(Debt debt, CancellationToken ct);
    Task Update(Debt debt, CancellationToken ct);
}
