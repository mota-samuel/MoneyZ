using MoneyZ.Domain.Enums;

namespace MoneyZ.Application.Services.Command;
public record ParseExpense
(
    string Description,
    decimal Amount,
    Payment? Payment
);