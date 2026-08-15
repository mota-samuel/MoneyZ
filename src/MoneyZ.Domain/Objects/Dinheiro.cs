namespace MoneyZ.Domain.Objects;
public sealed record Dinheiro
{
    public decimal Valor { get; }

    private Dinheiro(decimal valor) => Valor = Math.Round(valor, 2);

    public static Dinheiro Criar(decimal valor)
    {
        if(valor < 0)
            throw new ArgumentException("O valor não pode ser negativo.", nameof(valor));
        return new Dinheiro(valor);
    }

    public static Dinheiro Zero => new(0m);

    public static Dinheiro operator +(Dinheiro a, Dinheiro b) => new(a.Valor + b.Valor);

    public static Dinheiro operator -(Dinheiro a, Dinheiro b) => new(a.Valor - b.Valor);

    public override string ToString() => $"R$ {Valor:F2}";
}
