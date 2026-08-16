using MoneyZ.Domain.Enums;
using MoneyZ.Domain.Objects;

namespace MoneyZ.Domain.Entities;
public sealed class Expense
{
    public Guid ID { get;private set; }
    public Guid UserID { get;private set; }
    public DateTime Date { get;private set; }
    public string Description { get;private set; }
    public Dinheiro Valor { get;private set; }
    public CategoricoGasto Categoria { get;private set; }
    public Payment Payment { get;private set; }
    public string NumTelephone { get;private set; }

    private Expense() { }

    public static Expense NewExpense(
        Guid userId,
        string description,
        Dinheiro valor,
        CategoricoGasto categoria,
        Payment payment,
        string numTelephone
        )
    {
        return new Expense
        {
            ID = Guid.NewGuid(),
            UserID = userId,
            Date = DateTime.UtcNow,
            Description = description,
            Valor = valor,
            Categoria = categoria,
            Payment = payment,
            NumTelephone = numTelephone
        };
    }
}
