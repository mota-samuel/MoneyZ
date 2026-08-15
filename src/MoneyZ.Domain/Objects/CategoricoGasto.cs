using System.Security.AccessControl;

namespace MoneyZ.Domain.Objects;
public sealed record CategoricoGasto
{
    public string Nome { get; }
    public string SubCategoria { get; }

    private static readonly Dictionary<string, (string Categoria, string SubCategoria)> Mapeamento =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["almoço"] = ("Alimentação", "Restaurantes"),
            ["ifood"] = ("Alimentação", "Delivery"),
            ["mercado"] = ("Alimentação", "Supermercado"),
            ["uber"] = ("Transporte", "App (Uber/99)"),
            ["aluguel"] = ("Moradia", "Aluguel"),
            ["netflix"] = ("Assinaturas", "Streaming"),
            ["academia"] = ("Saúde", "Academia"),
            ["farmácia"] = ("Saúde", "Medicamentos"),
            ["cinema"] = ("Lazer", "Cinema")

        };

    private CategoricoGasto(string categoria, string subCategoria)
    {
        Nome = categoria;
        SubCategoria = subCategoria;
    }

    public static CategoricoGasto Resolver(string descricao)
    {
        if(string.IsNullOrWhiteSpace(descricao))
            return new CategoricoGasto("Outros", "Geral");

        foreach (var (chave,valor) in Mapeamento)
        {
            if(descricao.Contains(chave, StringComparison.OrdinalIgnoreCase))
                return new CategoricoGasto(valor.Categoria, valor.SubCategoria);
        }

        return new CategoricoGasto("Outros", "Geral");
    }

    /// <summary>
    /// Reconstrói uma CategoriaGasto a partir de valores já persistidos (ex.: ao carregar do
    /// banco), sem reprocessar a heurística de Resolver. Use Resolver apenas para classificar
    /// uma descrição nova; Reidratar preserva o valor histórico gravado.
    /// </summary>
    public static CategoricoGasto Reidratar(string categoria, string subcategoria) =>
        new(categoria, subcategoria);

}
