using WalletLedger.Domain;

namespace WalletLedger.Infrastructure.Ledger;

internal static class AmountParser
{
    public static Money ParsePositive(string text, Currency currency)
    {
        var amount = Money.Parse(text, currency);

        return amount.IsPositive
            ? amount
            : throw new InvalidAmountException("Amount must be greater than zero.");
    }
}