using MoneyZ.Domain.Enums;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MoneyZ.Domain.Entities;
public sealed class Debt
{
    public Guid ID { get;private set; }
    public Guid UserID { get; private set; }
    public string Description { get; private set; }
    public string Credor { get; private set; }
    public decimal ValorOriginal { get; private set; }
    public decimal SaldoDevedor { get; private set; }
    public decimal ParcelaMensal { get; private set; }
    public decimal JurosMensalPercent { get; private set; }
    public int TotalParcelas { get; private set; }
    public int ParcelasPagas { get; private set; }
    public StatusDivida Status { get; private set; }

    private Debt() { }

    /// <summary>
    /// Cria uma nova dívida aplicando um cálculo de juros simplificado: a taxa mensal informada
    /// é aplicada uma única vez sobre o valor original, e o resultado é diluído igualmente entre
    /// as parcelas. Este modelo NÃO representa uma tabela Price nem SAC — é uma aproximação
    /// pensada para acompanhamento via chat, não para cálculo financeiro/contratual da dívida.
    /// </summary>
    public static Debt Create(
        Guid userID,
        string description,
        string credor,
        decimal valorOriginal,
        decimal jurosMensalPercent,
        int totalParcelas)
    {
        var juros = valorOriginal * (jurosMensalPercent / 100m);
        var parcela = Math.Round((valorOriginal / totalParcelas) + juros, 2);

        return new Debt
        {
           ID = Guid.NewGuid(),
            UserID = userID,
            Description = description,
            Credor = credor,
            ValorOriginal = valorOriginal,
            SaldoDevedor = valorOriginal + juros,
            ParcelaMensal = parcela,
            JurosMensalPercent = jurosMensalPercent,
            TotalParcelas = totalParcelas,
            ParcelasPagas = 0,
            Status = StatusDivida.EmAberto
        };
    }

    public void DoPayment()
    {
        ParcelasPagas++;
        SaldoDevedor -= ParcelaMensal;

        if(SaldoDevedor <= 0 || ParcelasPagas >= TotalParcelas)
        {
            SaldoDevedor = 0;
            Status = StatusDivida.Quitada;
        }
    }

    public void UpdateStatusAtrasada()
    {
        if(Status.Equals(StatusDivida.EmAberto))
            Status = StatusDivida.Atrasada;
    }

    /// <summary>
    /// Reconstrói uma dívida a partir do estado já persistido, preservando Id, saldo devedor,
    /// parcelas pagas e status exatamente como gravados (Criar não deve ser usado para isso,
    /// pois recalcula parcela/saldo e não existe forma de restaurar Status = Atrasada só
    /// reaplicando RegistrarPagamento em loop).
    /// </summary>
    public static Debt Reidratar(
        Guid id,
        Guid usuarioId,
        string descricao,
        string credor,
        decimal valorOriginal,
        decimal saldoDevedor,
        decimal parcelaMensal,
        decimal jurosMensalPercent,
        int totalParcelas,
        int parcelasPagas,
        StatusDivida status)
    {
        return new Debt
        {
            ID = id,
            UserID = usuarioId,
            Description = descricao,
            Credor = credor,
            ValorOriginal = valorOriginal,
            SaldoDevedor = saldoDevedor,
            ParcelaMensal = parcelaMensal,
            JurosMensalPercent = jurosMensalPercent,
            TotalParcelas = totalParcelas,
            ParcelasPagas = parcelasPagas,
            Status = status
        };
    }

}
