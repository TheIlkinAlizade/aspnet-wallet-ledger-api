using Microsoft.EntityFrameworkCore;
using WalletLedger.Application.Accounts;
using WalletLedger.Application.Exceptions;
using WalletLedger.Application.Ledger;
using WalletLedger.Domain;
using WalletLedger.Infrastructure.Persistence;

namespace WalletLedger.Infrastructure.Ledger;

internal sealed class MoneyMovementService(WalletDbContext dbContext, TimeProvider timeProvider)
    : IMoneyMovementService
{
    public async Task<MovementResult> DepositAsync(
        Guid userId, Guid accountId, string amount, CancellationToken cancellationToken)
    {
        var account = await LoadOwnedAccountAsync(userId, accountId, cancellationToken);
        var money = ParsePositive(amount, account.Currency);

        return await PostAsync(LedgerTransactionType.Deposit, account, money, cancellationToken);
    }

    public async Task<MovementResult> WithdrawAsync(
        Guid userId, Guid accountId, string amount, CancellationToken cancellationToken)
    {
        var account = await LoadOwnedAccountAsync(userId, accountId, cancellationToken);
        var money = ParsePositive(amount, account.Currency);

        return await PostAsync(LedgerTransactionType.Withdrawal, account, money.Negate(), cancellationToken);
    }

    private async Task<MovementResult> PostAsync(
        LedgerTransactionType type, Account account, Money accountDelta, CancellationToken cancellationToken)
    {
        var clearing = await LoadClearingAccountAsync(account.Currency, cancellationToken);

        account.Apply(accountDelta);
        clearing.Apply(accountDelta.Negate());

        var transaction = LedgerTransaction.Post(
            type,
            [new EntryDraft(account.Id, accountDelta), new EntryDraft(clearing.Id, accountDelta.Negate())],
            timeProvider.GetUtcNow());

        dbContext.LedgerTransactions.Add(transaction);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new MovementResult(transaction.Id, AccountView.From(account));
    }

    private async Task<Account> LoadOwnedAccountAsync(
        Guid userId, Guid accountId, CancellationToken cancellationToken) =>
        await dbContext.Accounts.FirstOrDefaultAsync(
            account => account.Id == accountId && account.OwnerUserId == userId,
            cancellationToken)
        ?? throw new NotFoundException("Account not found.");

    private async Task<Account> LoadClearingAccountAsync(Currency currency, CancellationToken cancellationToken) =>
        await dbContext.Accounts.FirstOrDefaultAsync(
            account => account.Kind == AccountKind.SystemClearing && account.Currency == currency,
            cancellationToken)
        ?? throw new InvalidOperationException($"Clearing account for {currency.Code} is missing.");

    private static Money ParsePositive(string text, Currency currency)
    {
        var amount = Money.Parse(text, currency);

        return amount.IsPositive
            ? amount
            : throw new InvalidAmountException("Amount must be greater than zero.");
    }
}