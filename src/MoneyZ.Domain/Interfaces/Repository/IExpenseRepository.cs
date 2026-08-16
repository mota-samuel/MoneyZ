using MoneyZ.Domain.Entities;

namespace MoneyZ.Domain.Interfaces.Repository;
public interface IExpenseRepository
{
    Task<Expense?> GetById(Guid id, CancellationToken ct);
    Task<IEnumerable<Expense>> ListByUser(Guid userId, CancellationToken ct);
    Task<IEnumerable<Expense>> ListByUserAndDateRange(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct);
    Task Add(Expense expense, CancellationToken ct);

}
