namespace WalletLedger.Domain;

public sealed class Transfer
{
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
        Amount = amount;
        LedgerTransactionId = ledgerTransactionId;
        CreatedAt = createdAt;
        Status = TransferStatus.Completed;
    }

    public Guid Id { get; }
    public Guid SourceAccountId { get; }
    public Guid DestinationAccountId { get; }
    public Money Amount { get; }
    public Guid LedgerTransactionId { get; }
    public DateTimeOffset CreatedAt { get; }
    public TransferStatus Status { get; }

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