using MoneyZ.Application.Services.Command;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MoneyZ.Application.Services;
public sealed partial class MessageParser
{
    public ParseExpense? ParseExpense(string message)
    {
        // Padrão: "descrição valor" ou "descrição valor formaPagamento"
        // Ex: "almoço 25.90" | "mercado 150 pix" | "uber 18.50 crédito"
        var match = ExpensePattern().Match(message.Trim());

        if(!match.Success) return null;

        var description  = match.Groups["description"].Value;
        var valueString   = match.Groups["value"].Value.Replace(',', '.'); // Substitui vírgula por ponto para conversão
        var paymentMethod = match.Groups["paymentMethod"].Success ? match.Groups["paymentMethod"].Value : null;

        if (!decimal.TryParse(valueString, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
        {
            return null; // Falha ao converter o valor para decimal
        }

        return new ParseExpense(description, value, MoneyZ.Application.Helpers.Helpers.PaymentHelper(paymentMethod));
    }


    [GeneratedRegex(@"^(?<descricao>.+?)\s+(?<valor>[\d.,]+)(?:\s+(?<forma>\w+))?$")]
    private static partial Regex ExpensePattern();

}
