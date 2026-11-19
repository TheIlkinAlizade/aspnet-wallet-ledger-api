namespace WalletLedger.Domain;

public sealed class LedgerEntry
{
    internal LedgerEntry(Guid transactionId, Guid accountId, Money amount, DateTimeOffset createdAt)
    {
        if (amount.IsZero)
        {
            throw new InvalidLedgerTransactionException("A ledger entry cannot have a zero amount.");
        }

        Id = Guid.CreateVersion7();
        TransactionId = transactionId;
        AccountId = accountId;
        Amount = amount;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }
    public Guid TransactionId { get; }
    public Guid AccountId { get; }
    public Money Amount { get; }
    public DateTimeOffset CreatedAt { get; }
}