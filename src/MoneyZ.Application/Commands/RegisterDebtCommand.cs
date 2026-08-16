namespace MoneyZ.Application.Commands;
public record RegisterDebtCommand
(
    string NumberTel,
    string Description,
    string Credor,
    decimal Amount,
    decimal JurosMensalPercent,
    int TotalParcelas
    );
