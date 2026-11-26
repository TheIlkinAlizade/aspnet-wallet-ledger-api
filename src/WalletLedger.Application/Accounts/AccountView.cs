using WalletLedger.Domain;

namespace WalletLedger.Application.Accounts;

public sealed record AccountView(Guid Id, Currency Currency, Money Balance)
{
    public static AccountView From(Account account) => new(account.Id, account.Currency, account.Balance);
}