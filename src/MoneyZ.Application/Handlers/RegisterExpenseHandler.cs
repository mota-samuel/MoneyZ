using Microsoft.Extensions.Logging;
using MoneyZ.Application.Commands;
using MoneyZ.Domain.Entities;
using MoneyZ.Domain.Enums;
using MoneyZ.Domain.Interfaces.Repository;
using MoneyZ.Domain.Objects;

namespace MoneyZ.Application.Handlers;
public  sealed class RegisterExpenseHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly ILogger<RegisterExpenseHandler> _logger;

    public RegisterExpenseHandler(IUserRepository userRepository, IExpenseRepository expenseRepository, ILogger<RegisterExpenseHandler> logger)
    {
        _userRepository = userRepository;
        _expenseRepository = expenseRepository;
        _logger = logger;
    }

    public async Task<string> HandlerUseCase(RegisterExpenseCommand comand, CancellationToken ct)
    {
        var telephone = Telefone.Create(comand.NumberTel);
        var user = await _userRepository.GetByTelephone(telephone, ct);

        if (user is null || user.StatusCadastro == StatusCadastro.Inativo)
        {
            return "Voce precisa concluir seu cadastro antes de registrar gastos e dividas!!";
        }

        var valor = Dinheiro.Create(comand.Amount);
        var categoria = CategoricoGasto.Resolver(comand.Description);
        var paymentMethod = MoneyZ.Application.Helpers.Helpers.PaymentHelper(comand.PaymentMethod);
        var expense = Expense.Create(user.ID, comand.Description,valor,categoria,paymentMethod,telephone.Numero);

        await _expenseRepository.Add(expense, ct);

        _logger.LogInformation($"Despesa registrada: {expense.Description} - {expense.Valor}");

        return $"✅ Gasto registrado!\n\n{categoria.Nome} > {categoria.SubCategoria}\n{valor} via {paymentMethod}";
    }

}
