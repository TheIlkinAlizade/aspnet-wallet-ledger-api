namespace WalletLedger.Domain;

public sealed class Transfer
{
    private Transfer()
    {
        Currency = null!;
    }

    private Transfer(
        Guid sourceAccountId,
        Guid destinationAccountId,
        Money amount,
        Guid ledgerTransactionId,
        DateTimeOffset createdAt)
    {
        Id = Guid.CreateVersion7();
        SourceAccountId = sourceAccountId;
        DestinationAccountId = destinationAccountId;
        AmountMinorUnits = amount.MinorUnits;
        Currency = amount.Currency;
        LedgerTransactionId = ledgerTransactionId;
        CreatedAt = createdAt;
        Status = TransferStatus.Completed;
    }

    public Guid Id { get; private set; }
    public Guid SourceAccountId { get; private set; }
    public Guid DestinationAccountId { get; private set; }
    public long AmountMinorUnits { get; private set; }
    public Currency Currency { get; private set; }
    public Guid LedgerTransactionId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public TransferStatus Status { get; private set; }
    public Money Amount => Money.Of(AmountMinorUnits, Currency);

    public static Transfer Create(
        Account source,
        Account destination,
        Money amount,
        Guid ledgerTransactionId,
        DateTimeOffset createdAt)
    {
        EnsureDifferentAccounts(source, destination);
        EnsurePositive(amount);
        EnsureCurrenciesMatch(source, destination, amount);

        return new Transfer(source.Id, destination.Id, amount, ledgerTransactionId, createdAt);
    }

    private static void EnsureDifferentAccounts(Account source, Account destination)
    {
        if (source.Id == destination.Id)
        {
            throw new InvalidTransferException("Source and destination accounts must differ.");
        }
    }

    private static void EnsurePositive(Money amount)
    {
        if (!amount.IsPositive)
        {
            throw new InvalidAmountException("Transfer amount must be positive.");
        }
    }

    private static void EnsureCurrenciesMatch(Account source, Account destination, Money amount)
    {
        if (amount.Currency != source.Currency)
        {
            throw new CurrencyMismatchException(source.Currency, amount.Currency);
        }

        if (amount.Currency != destination.Currency)
        {
            throw new CurrencyMismatchException(destination.Currency, amount.Currency);
        }
    }
}