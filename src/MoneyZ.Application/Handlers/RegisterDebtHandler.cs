using Microsoft.Extensions.Logging;
using MoneyZ.Application.Commands;
using MoneyZ.Domain.Entities;
using MoneyZ.Domain.Interfaces.Repository;
using MoneyZ.Domain.Objects;

namespace MoneyZ.Application.Handlers;
public sealed class RegisterDebtHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IDebtRepository _debtRepository;
    private readonly ILogger<RegisterDebtHandler> _logger;

    public RegisterDebtHandler(IUserRepository userRepository, IDebtRepository debtRepository, ILogger<RegisterDebtHandler> logger)
    {
        _userRepository = userRepository;
        _debtRepository = debtRepository;
        _logger = logger;
    }

    public async Task<string> HandlerUseCase(RegisterDebtCommand command, CancellationToken ct)
    {
        var telephone = Telefone.Create(command.NumberTel);
        var user = await _userRepository.GetByTelephone(telephone, ct);

        if (user is null || user.StatusCadastro == Domain.Enums.StatusCadastro.Inativo)
        {
            return "Voce precisa concluir seu cadastro antes de registrar gastos e dividas!!";
        }

        var debt = Debt.Create(user.ID, command.Description, command.Credor, command.Amount, command.JurosMensalPercent, command.TotalParcelas);

        await _debtRepository.Add(debt, ct);

        _logger.LogInformation(
            $"Dívida registrada: {command.Description} - {command.Amount} em {command.TotalParcelas}x");

        return $"✅ Dívida registrada!\n\n{command.Description} ({command.Credor})\n"
             + $"Valor: R$ {command.Amount:F2}\n"
             + $"Parcelas: {command.TotalParcelas}x de R$ {debt.ParcelaMensal:F2}\n"
             + $"Juros: {command.JurosMensalPercent}% a.m.";

    }
}
