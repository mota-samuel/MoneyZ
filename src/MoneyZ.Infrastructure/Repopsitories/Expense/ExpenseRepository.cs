using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MoneyZ.Domain.Entities;
using MoneyZ.Domain.Enums;
using MoneyZ.Domain.Interfaces.Repository;
using MoneyZ.Domain.Objects;

namespace MoneyZ.Infrastructure.Repopsitories.Expense;
public class ExpenseRepository : IExpenseRepository
{
    private readonly string _connectionString;

    public ExpenseRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")!;
    }

    public async Task Add(Domain.Entities.Expense expense, CancellationToken ct)
    {
        await using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            new CommandDefinition(@"
            INSERT INTO Expense (ID, UserID, Date, Description, Valor, Categoria, Subcategoria, FormaPagamento, NumeroTelefone)
                VALUES (@ID, @UserID, @Date, @Description, @Valor, @Categoria, @Subcategoria, @FormaPagamento, @NumeroTelefone)",
            new
            {
                expense.ID,
                expense.UserID,
                expense.Date,
                expense.Description,
                expense.Valor.Valor,
                expense.Categoria.Nome,
                expense.Categoria.SubCategoria,
                expense.Payment,
                expense.NumTelephone
            },
            cancellationToken: ct));

    }

    public async Task<Domain.Entities.Expense?> GetById(Guid id, CancellationToken ct)
    {
        await using var connection = new SqlConnection(_connectionString);

        var dto = await connection.QueryFirstOrDefaultAsync<ExpenseDto>(
            new CommandDefinition(
                "SELECT * FROM Expense WHERE ID = @ID",
                new { ID = id },
                cancellationToken: ct
            )
        );

        return dto is null ? null : MapToEntity(dto);
    }

    public async Task<IEnumerable<Domain.Entities.Expense>> ListByUser(Guid userId, CancellationToken ct)
    {
        await using var connection = new SqlConnection(_connectionString);

        var dtos = await connection.QueryAsync<ExpenseDto>(
            new CommandDefinition(
                "SELECT * FROM Expense WHERE UserID = @UserID ORDER BY Date DESC",
                new { UserID = userId },
                cancellationToken: ct));

        return dtos.Select(MapToEntity);

    }

    public async Task<IEnumerable<Domain.Entities.Expense>> ListByUserAndDateRange(Guid userId, DateTime startDate, DateTime endDate, CancellationToken ct)
    {
        await using var connection = new SqlConnection(_connectionString);

        var dtos = await connection.QueryAsync<ExpenseDto>(
            new CommandDefinition(
                @"SELECT * FROM Expense 
                  WHERE UserID = @UserID 
                  AND Data BETWEEN @StartDate AND @EndDate
                  ORDER BY Date DESC",
                new { UserId = userId, StartDate = startDate, EndDate = endDate },
                cancellationToken: ct
            )
        );

        return dtos.Select(MapToEntity);
    }

    private static MoneyZ.Domain.Entities.Expense MapToEntity(ExpenseDto dto)
    {
        // Reidrata Valor e Categoria a partir do que foi persistido, em vez de recalcular
        // (Resolver poderia classificar diferente se o dicionário de categorias mudar depois).
        var valor = Dinheiro.Create(dto.Valor);
        var categoria = CategoricoGasto.Reidratar(dto.Categoria, dto.Subcategoria);

        return MoneyZ.Domain.Entities.Expense.Reidratar(
            dto.ID,
            dto.UserID,
            dto.Date,
            dto.Description,
            valor,
            categoria,
            Application.Helper.Helpers.PaymentHelper(dto.FormaPagamento),
            dto.NumTelephone);
    }

    /// <summary>
    /// DTO fortemente tipado para o resultado do Dapper — substitui o mapeamento via "dynamic".
    /// </summary>
    private sealed class ExpenseDto
    {
        public Guid ID { get; init; }
        public Guid UserID { get; init; }
        public DateTime Date { get; init; }
        public string Description { get; init; } = default!;
        public decimal Valor { get; init; }
        public string Categoria { get; init; } = default!;
        public string Subcategoria { get; init; } = default!;
        public string FormaPagamento { get; init; } = default!;
        public string NumTelephone { get; init; } = default!;
    }


}
