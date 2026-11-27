using System.Globalization;
using WalletLedger.Domain;

namespace WalletLedger.Api.Contracts;

public static class MoneyFormatting
{
    public static string ToAmountString(this Money money) =>
        money.ToDecimal().ToString($"F{money.Currency.DecimalPlaces}", CultureInfo.InvariantCulture);
}