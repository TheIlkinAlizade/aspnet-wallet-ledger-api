namespace WalletLedger.Domain;

public sealed class LedgerTransaction
{
    private readonly List<LedgerEntry> entries = [];

    private LedgerTransaction(LedgerTransactionType type, DateTimeOffset createdAt, Guid? reversalOfId)
    {
        Id = Guid.CreateVersion7();
        Type = type;
        CreatedAt = createdAt;
        ReversalOfId = reversalOfId;
    }

    public Guid Id { get; }
    public LedgerTransactionType Type { get; }
    public DateTimeOffset CreatedAt { get; }
    public Guid? ReversalOfId { get; }
    public IReadOnlyList<LedgerEntry> Entries => entries;

    public static LedgerTransaction Post(
        LedgerTransactionType type,
        IReadOnlyCollection<EntryDraft> drafts,
        DateTimeOffset createdAt,
        Guid? reversalOfId = null)
    {
        EnsureReversalLinkMatchesType(type, reversalOfId);
        EnsureAtLeastTwoEntries(drafts);
        EnsureSingleCurrency(drafts);
        EnsureBalanced(drafts);

        var transaction = new LedgerTransaction(type, createdAt, reversalOfId);
        transaction.AddEntries(drafts);
        return transaction;
    }

    public LedgerTransaction CreateReversal(DateTimeOffset createdAt)
    {
        if (Type == LedgerTransactionType.Reversal)
        {
            throw new InvalidLedgerTransactionException("A reversal cannot be reversed.");
        }

        var drafts = entries
            .Select(entry => new EntryDraft(entry.AccountId, entry.Amount.Negate()))
            .ToList();

        return Post(LedgerTransactionType.Reversal, drafts, createdAt, Id);
    }

    private void AddEntries(IEnumerable<EntryDraft> drafts)
    {
        foreach (var draft in drafts)
        {
            entries.Add(new LedgerEntry(Id, draft.AccountId, draft.Amount, CreatedAt));
        }
    }

    private static void EnsureReversalLinkMatchesType(LedgerTransactionType type, Guid? reversalOfId)
    {
        if ((type == LedgerTransactionType.Reversal) != reversalOfId.HasValue)
        {
            throw new InvalidLedgerTransactionException(
                "Only reversal transactions must reference the transaction they reverse.");
        }
    }

    private static void EnsureAtLeastTwoEntries(IReadOnlyCollection<EntryDraft> drafts)
    {
        if (drafts.Count < 2)
        {
            throw new InvalidLedgerTransactionException("A ledger transaction needs at least two entries.");
        }
    }

    private static void EnsureSingleCurrency(IReadOnlyCollection<EntryDraft> drafts)
    {
        if (drafts.Select(draft => draft.Amount.Currency).Distinct().Count() > 1)
        {
            throw new InvalidLedgerTransactionException("All entries of a transaction must share one currency.");
        }
    }

    private static void EnsureBalanced(IReadOnlyCollection<EntryDraft> drafts)
    {
        if (drafts.Sum(draft => draft.Amount.MinorUnits) != 0)
        {
            throw new InvalidLedgerTransactionException("Ledger entries must sum to zero.");
        }
    }
}