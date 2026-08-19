using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MoneyZ.Domain.Enums;
using MoneyZ.Domain.Interfaces.Repository;
using MoneyZ.Domain.Objects;

namespace MoneyZ.Infrastructure.Repopsitories.User;
public class UserRepository : IUserRepository
{
    private readonly string _connectionString;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IConfiguration config, ILogger<UserRepository> logger)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")!;
        _logger = logger;
    }

    public async Task Add(Domain.Entities.User user, CancellationToken ct)
    {
        await using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            new CommandDefinition(
                @"INSERT INTO User (ID, Telephone, Name, RendaFixa, RendaVariavel, StatusCadastro, CriadoEm)
                    VALUES (@ID, @Telephone, @Name, @RendaFixa, @RendaVariavel, @StatusCadastro, @CriadoEm)",
            ),
            new
            {
                user.ID,
                Telephone = user.Telefone.Numero,
                user.Name,
                user.RendaFixa,
                user.RendaVariavel,
                StatusCadastro = (int)user.StatusCadastro,
                user.CriadoEm
            }, cancellationToken : ct);

        _logger.LogInformation($"Usuário adicionado: {user.ID} - {user.Telefone.Numero}");
    }

    public async Task<Domain.Entities.User?> GetById(Guid id, CancellationToken ct)
    {
        await using var connection = new SqlConnection(_connectionString);

        var dto = await connection.QueryFirstOrDefaultAsync<UserDto>(
            new CommandDefinition(
                "SELECT * FROM User WHERE ID = @ID",
                new { ID = id },
                cancellationToken: ct
                )
            );

        return dto is null ? null : MapToEntity(dto);
    }

    public async Task<Domain.Entities.User?> GetByTelephone(Telefone telephone, CancellationToken ct)
    {
        await using var connection = new SqlConnection(_connectionString);

        var dto = await connection.QueryFirstOrDefaultAsync<UserDto>(
            new CommandDefinition(
                "SELECT * FROM User WHERE Telephone = @Telephone",
                new { Telephone = telephone },
                cancellationToken: ct)
                );

        return dto is null ? null : MapToEntity(dto);
    }

    public async Task Update(Domain.Entities.User user, CancellationToken ct)
    {
        await using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(new CommandDefinition(@"
            UPDATE User
            SET Name = @Name, RendaFixa = @RendaFixa, RendaVariavel = @RendaVariavel, StatusCadastro = @StatusCadastro
            WHERE ID = @ID",
            new
            {
                user.ID,
                user.Name,
                user.RendaFixa,
                user.RendaVariavel,
                EstadoCadastro = (int)user.StatusCadastro
            }, cancellationToken: ct));

    }

    private static MoneyZ.Domain.Entities.User MapToEntity(UserDto dto)
    {
        // Reidrata o usuário com o estado exatamente como persistido (Id, EstadoCadastro e
        // CriadoEm reais) em vez de recompor via IniciarCadastro + DefinirNome/DefinirRendaFixa,
        // que sempre gerava um novo Id e forçava EstadoCadastro = AguardandoNome na leitura.
        return MoneyZ.Domain.Entities.User.Reidratar(
            dto.ID,
            Telefone.Create(dto.Telephone),
            dto.Name,
            dto.RendaFixa,
            dto.RendaVariavel,
            (StatusCadastro)dto.StatusCadastro,
            dto.CriadoEm);
    }


    /// <summary>
    /// DTO fortemente tipado para o resultado do Dapper — substitui o mapeamento via "dynamic",
    /// que só acusa erro em runtime quando o schema da tabela muda.
    /// </summary>
    private sealed class UserDto
    {
        public Guid ID { get; init; }
        public string Telephone { get; init; } = default!;
        public string? Name { get; init; }
        public decimal RendaFixa { get; init; }
        public decimal? RendaVariavel { get; init; }
        public int StatusCadastro { get; init; }
        public DateTime CriadoEm { get; init; }
    }

}
