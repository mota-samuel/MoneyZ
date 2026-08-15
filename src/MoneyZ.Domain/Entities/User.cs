using MoneyZ.Domain.Enums;
using MoneyZ.Domain.Objects;
using System.Security.Principal;

namespace MoneyZ.Domain.Entities;
public sealed class User
{
    public Guid ID { get; private set; }
    public Telefone Telefone { get; private set; }
    public string Name { get; private set; }
    public  decimal RendaFixa { get; private set; }
    public decimal? RendaVariavel { get; private set; }
    public StatusCadastro StatusCadastro { get;private set; }
    public DateTime CriadoEm { get; private set; }

    private User() { }

    public static User Create(Telefone tel)
    {
        return new User
        {
            ID = Guid.NewGuid(),
            Telefone = tel,
            RendaFixa = 0,
            RendaVariavel = null,
            StatusCadastro = StatusCadastro.AguardandoNome,
            CriadoEm = DateTime.UtcNow
        };
    }

    public void UpdateName(string name)
    {
        if(string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        Name = name.Trim();
        StatusCadastro = StatusCadastro.AguardandoRenda;
    }

    public void DefinirRendaFixa(decimal renda)
    {
        if (renda < 0)
            throw new ArgumentException("Renda inválida.");

        RendaFixa = renda;
        StatusCadastro = StatusCadastro.Ativo;
    }

    /// <summary>
    /// Reconstrói um usuário a partir do estado já persistido, preservando Id, EstadoCadastro
    /// e CriadoEm exatamente como gravados (IniciarCadastro não deve ser usado para isso, pois
    /// sempre gera um novo Id e força EstadoCadastro = AguardandoNome).
    /// </summary>
    public static User Reidratar(
        Guid id,
        Telefone telefone,
        string? nome,
        decimal rendaFixa,
        decimal? rendaVariavel,
        StatusCadastro estadoCadastro,
        DateTime criadoEm)
    {
        return new User
        {
            ID = id,
            Telefone = telefone,
            Name = nome,
            RendaFixa = rendaFixa,
            RendaVariavel = rendaVariavel,
            StatusCadastro = estadoCadastro,
            CriadoEm = criadoEm
        };
    }

}
