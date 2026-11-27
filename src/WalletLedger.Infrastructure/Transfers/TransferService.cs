using WalletLedger.Application.Transfers;
using WalletLedger.Domain;
using WalletLedger.Infrastructure.Accounts;
using WalletLedger.Infrastructure.Ledger;
using WalletLedger.Infrastructure.Persistence;

namespace WalletLedger.Infrastructure.Transfers;

internal sealed class TransferService(WalletDbContext dbContext, TimeProvider timeProvider) : ITransferService
{
    public async Task<TransferView> TransferAsync(
        Guid userId,
        Guid sourceAccountId,
        Guid destinationAccountId,
        string amountText,
        CancellationToken cancellationToken)
    {
        var source = await dbContext.GetOwnedAsync(userId, sourceAccountId, cancellationToken);
        var destination = await dbContext.GetTransferTargetAsync(destinationAccountId, cancellationToken);
        var amount = AmountParser.ParsePositive(amountText, source.Currency);
        var now = timeProvider.GetUtcNow();

        var ledgerTransaction = LedgerTransaction.Post(
            LedgerTransactionType.Transfer,
            [new EntryDraft(source.Id, amount.Negate()), new EntryDraft(destination.Id, amount)],
            now);

        var transfer = Transfer.Create(source, destination, amount, ledgerTransaction.Id, now);

        source.Apply(amount.Negate());
        destination.Apply(amount);

        dbContext.LedgerTransactions.Add(ledgerTransaction);
        dbContext.Transfers.Add(transfer);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TransferView.From(transfer);
    }
}