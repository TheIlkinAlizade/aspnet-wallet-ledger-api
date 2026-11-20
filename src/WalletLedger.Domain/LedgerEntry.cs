namespace WalletLedger.Domain;

public sealed class LedgerEntry
{
    private LedgerEntry()
    {
        Currency = null!;
    }

    internal LedgerEntry(Guid transactionId, Guid accountId, Money amount, DateTimeOffset createdAt)
    {
        if (amount.IsZero)
        {
            throw new InvalidLedgerTransactionException("A ledger entry cannot have a zero amount.");
        }

        Id = Guid.CreateVersion7();
        TransactionId = transactionId;
        AccountId = accountId;
        AmountMinorUnits = amount.MinorUnits;
        Currency = amount.Currency;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid TransactionId { get; private set; }
    public Guid AccountId { get; private set; }
    public long AmountMinorUnits { get; private set; }
    public Currency Currency { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Money Amount => Money.Of(AmountMinorUnits, Currency);
}