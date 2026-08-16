namespace MoneyZ.Application.Commands;
public record RegisterExpenseCommand(
    
    string NumberTel,
    string Description,
    decimal Amount,
    string PaymentMethod
    );
