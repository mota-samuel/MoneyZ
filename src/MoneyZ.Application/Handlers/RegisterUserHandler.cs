using Microsoft.Extensions.Logging;
using MoneyZ.Domain.Entities;
using MoneyZ.Domain.Enums;
using MoneyZ.Domain.Interfaces.Repository;
using MoneyZ.Domain.Objects;

namespace MoneyZ.Application.Handlers;
public sealed class RegisterUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RegisterUserHandler> _logger;

    public RegisterUserHandler(IUserRepository userRepository, ILogger<RegisterUserHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<string> HandlerUseCase(string numberTel, string message, CancellationToken ct)
    {
        var telephone = Telefone.Create(numberTel);
        var user = await _userRepository.GetByTelephone(telephone, ct);

        if (user is null)
        {
            var newUser = User.Create(telephone);
            await _userRepository.Add(newUser, ct);
            return "Olá! Bem-vindo ao MoneyZ 🤖\n\nPara começar, qual é o seu nome?";
        }

        return user.StatusCadastro switch
        {
            StatusCadastro.AguardandoNome => await ProcessName(user, message, ct),
            StatusCadastro.AguardandoRenda => await ProcessIncome(user, message, ct),
            StatusCadastro.Ativo => "Seu cadastro está completo! Você já pode registrar seus gastos.",
            _ => "Status de cadastro desconhecido. Por favor, tente novamente ou entre em contato com o suporte."
        };
    }

    private async Task<string> ProcessName(User user, string name, CancellationToken ct)
    {
        user.UpdateName(name);
        await _userRepository.Update(user,ct);
        _logger.LogInformation($"Nome definido para usuário {user.ID}");
        return $"Prazer, {name}! Agora, qual é a sua renda mensal fixa?";
    }

    private async Task<string> ProcessIncome(User user, string message, CancellationToken ct)
    {
        if(!decimal.TryParse(message, out var income))
            return "Não entendi o valor. Digite sua renda mensal (ex: 3500)";

        user.DefinirRendaFixa(income);
        await _userRepository.Update(user,ct);

        _logger.LogInformation($"Renda definida para o user {user.ID}: {income}");

        return $"✅ Cadastro concluído!\n\nRenda fixa: R$ {income:F2}\n"
             + "Agora você pode registrar gastos enviando:\n"
             + "Descrição + Valor (ex: \"almoço 25.90\")";

    }
}
