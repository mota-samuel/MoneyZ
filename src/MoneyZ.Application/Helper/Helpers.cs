using MoneyZ.Domain.Enums;

namespace MoneyZ.Application.Helpers;
public static class Helpers
{
    public static Payment PaymentHelper(string paymentMethod)
    {
        return paymentMethod.ToLower() switch
        {
            "dinheiro" => Payment.Dinheiro,
            "credito" => Payment.Credito,
            "debito" => Payment.Debito,
            "pix" => Payment.PIX,
            _ => Payment.Others
        };
    }
}
