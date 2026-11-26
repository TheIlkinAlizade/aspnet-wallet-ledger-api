using System.Globalization;
using WalletLedger.Application.Accounts;

namespace WalletLedger.Api.Contracts.Accounts;

public sealed record AccountResponse(Guid Id, string Currency, long BalanceMinorUnits, string Balance)
{
    public static AccountResponse From(AccountView view) => new(
        view.Id,
        view.Currency.Code,
        view.Balance.MinorUnits,
        FormatAmount(view));

    private static string FormatAmount(AccountView view) =>
        view.Balance.ToDecimal().ToString($"F{view.Currency.DecimalPlaces}", CultureInfo.InvariantCulture);
}