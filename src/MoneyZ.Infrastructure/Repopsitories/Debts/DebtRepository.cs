using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MoneyZ.Domain.Entities;
using MoneyZ.Domain.Enums;
using MoneyZ.Domain.Interfaces.Repository;

namespace MoneyZ.Infrastructure.Repopsitories.Debts;
public class DebtRepository : IDebtRepository
{
    private readonly string _connectionString;

    public DebtRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")!;
    }

    public async Task Add(Debt debt, CancellationToken ct)
    {
        await using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            new CommandDefinition(
                @"
            INSERT INTO Debts (ID, UserID, Description, Credor, ValorOriginal, SaldoDevedor, ParcelaMensal, JurosMensalPercent, TotalParcelas, ParcelasPagas, Status)
            VALUES (@ID, @UserID, @Description, @Credor, @ValorOriginal, @SaldoDevedor, @ParcelaMensal, @JurosMensalPercent, @TotalParcelas, @ParcelasPagas, @Status)",
            new
            {
               debt.ID,
                debt.UserID,
                debt.Description,
                debt.Credor,
                debt.ValorOriginal,
                debt.SaldoDevedor,
                debt.ParcelaMensal,
                debt.JurosMensalPercent,
                debt.TotalParcelas,
                debt.ParcelasPagas,
                debt.Status
            },
            cancellationToken: ct)
            );

    }

    public async Task<Debt?> GetByID(Guid id, CancellationToken ct)
    {
        await using var connection = new SqlConnection(_connectionString);

        var dto = await connection.QueryFirstOrDefaultAsync<DebtDto>(
            new CommandDefinition(
                "SELECT * FROM Debts WHERE ID = @ID",
                new { ID = id },
                cancellationToken: ct)
            );

        return dto is null ? null : MapToEntity(dto);
    }

    public async Task<IEnumerable<Debt>> ListByUser(Guid userId, CancellationToken ct)
    {
        await using var connection = new SqlConnection(_connectionString);

        var dtos = await connection.QueryAsync<DebtDto>(
            new CommandDefinition(
                "SELECT * FROM Debts WHERE UserID = @UserID" +
                "ORDER BY SaldoDevedor DESC",
                new { UserID = userId },
                cancellationToken: ct)
            );

        return dtos.Select(MapToEntity);
    }

    public async Task<IEnumerable<Debt>> ListInDebts(Guid userId, CancellationToken ct)
    {
        await using var connection = new SqlConnection(_connectionString);

        var dtos = await connection.QueryAsync<DebtDto>(
            new CommandDefinition(@"
                SELECT * FROM Debts
                WHERE UserID = @UserID AND Status = 0
                ORDER BY SaldoDevedor DESC
            ", new { UserID = userId },
            cancellationToken: ct)
            );

        return dtos.Select(MapToEntity);
    }

    public async Task Update(Debt debt, CancellationToken ct)
    {
        await using var connection = new SqlConnection( _connectionString);

        await connection.ExecuteAsync(
            new CommandDefinition(@"
                UPDATE Debts
                SET SaldoDevedor = @SaldoDevedor,
                ParcelasPagas = @ParcelasPagas,
                Status = @Status
                WHERE ID = @ID
            ",
            new { debt.ID, debt.SaldoDevedor, debt.ParcelasPagas, Status = (int)debt.Status },
            cancellationToken: ct)
            );
    }

    private static Debt MapToEntity(DebtDto dto)
    {
        // Reidrata com o SaldoDevedor, ParcelasPagas e Status exatamente como persistidos, em vez
        // de recalcular a parcela via Criar e "andar" o estado chamando RegistrarPagamento em loop
        // (o que não conseguiria reproduzir Status = Atrasada, por exemplo).
        return Debt.Reidratar(
            dto.ID,
            dto.UserID,
            dto.Description,
            dto.Credor,
            dto.ValorOriginal,
            dto.SaldoDevedor,
            dto.ParcelaMensal,
            dto.JurosMensalPercent,
            dto.TotalParcelas,
            dto.ParcelasPagas,
            (StatusDivida)dto.Status);
    }

    /// <summary>
    /// DTO fortemente tipado para o resultado do Dapper — substitui o mapeamento via "dynamic".
    /// </summary>
    private sealed class DebtDto
    {
        public Guid ID { get; init; }
        public Guid UserID { get; init; }
        public string Description { get; init; } = default!;
        public string Credor { get; init; } = default!;
        public decimal ValorOriginal { get; init; }
        public decimal SaldoDevedor { get; init; }
        public decimal ParcelaMensal { get; init; }
        public decimal JurosMensalPercent { get; init; }
        public int TotalParcelas { get; init; }
        public int ParcelasPagas { get; init; }
        public int Status { get; init; }
    }

}
