using WalletLedger.Application.Accounts;

namespace WalletLedger.Api.Contracts.Accounts;

public sealed record AccountResponse(Guid Id, string Currency, long BalanceMinorUnits, string Balance)
{
    public static AccountResponse From(AccountView view) => new(
        view.Id,
        view.Currency.Code,
        view.Balance.MinorUnits,
        view.Balance.ToAmountString());
}