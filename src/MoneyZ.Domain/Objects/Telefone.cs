using System.Text.RegularExpressions;

namespace MoneyZ.Domain.Objects;
public sealed partial record Telefone
{
    private const int MinDigits = 8;
    private const int MaxDigits = 15;

    public string Numero { get; }

    private Telefone(string numero) => Numero = numero;
    
    public static Telefone Create(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new ArgumentException("Número de telefone não pode ser vazio.", nameof(raw));
        }
        
        var digitos = OnlyDigits().Replace(raw, string.Empty);

        if (digitos.Length < MinDigits || digitos.Length > MaxDigits)
            throw new ArgumentException($"Número de telefone deve ter entre {MinDigits} e {MaxDigits} dígitos.", nameof(raw));
        return new Telefone(digitos);
    }

    public override string ToString() => $"+{Numero}";

    [GeneratedRegex(@"[^\d]")]
    private static partial Regex OnlyDigits();

}