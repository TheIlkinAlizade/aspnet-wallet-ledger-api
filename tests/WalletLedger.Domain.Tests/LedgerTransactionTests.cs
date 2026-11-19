namespace WalletLedger.Domain.Tests;

public class LedgerTransactionTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 19, 12, 0, 0, TimeSpan.Zero);

    private static EntryDraft Draft(Guid accountId, long minorUnits, Currency? currency = null) =>
        new(accountId, Money.Of(minorUnits, currency ?? Currency.Usd));

    [Fact]
    public void Post_BalancedEntries_CreatesTransactionWithEntries()
    {
        var alice = Guid.NewGuid();
        var bob = Guid.NewGuid();

        var transaction = LedgerTransaction.Post(
            LedgerTransactionType.Transfer,
            [Draft(alice, -3000), Draft(bob, 3000)],
            Now);

        Assert.Equal(2, transaction.Entries.Count);
        Assert.All(transaction.Entries, entry => Assert.Equal(transaction.Id, entry.TransactionId));
        Assert.Equal(0, transaction.Entries.Sum(entry => entry.Amount.MinorUnits));
    }

    [Fact]
    public void Post_EntriesNotSummingToZero_Throws()
    {
        Assert.Throws<InvalidLedgerTransactionException>(() => LedgerTransaction.Post(
            LedgerTransactionType.Deposit,
            [Draft(Guid.NewGuid(), 100), Draft(Guid.NewGuid(), -99)],
            Now));
    }

    [Fact]
    public void Post_SingleEntry_Throws()
    {
        Assert.Throws<InvalidLedgerTransactionException>(() => LedgerTransaction.Post(
            LedgerTransactionType.Deposit,
            [Draft(Guid.NewGuid(), 0)],
            Now));
    }

    [Fact]
    public void Post_MixedCurrencies_Throws()
    {
        Assert.Throws<InvalidLedgerTransactionException>(() => LedgerTransaction.Post(
            LedgerTransactionType.Transfer,
            [Draft(Guid.NewGuid(), 100, Currency.Usd), Draft(Guid.NewGuid(), -100, Currency.Eur)],
            Now));
    }

    [Fact]
    public void Post_ZeroAmountEntry_Throws()
    {
        Assert.Throws<InvalidLedgerTransactionException>(() => LedgerTransaction.Post(
            LedgerTransactionType.Transfer,
            [Draft(Guid.NewGuid(), 100), Draft(Guid.NewGuid(), -100), Draft(Guid.NewGuid(), 0)],
            Now));
    }

    [Fact]
    public void Post_ReversalWithoutOriginalId_Throws()
    {
        Assert.Throws<InvalidLedgerTransactionException>(() => LedgerTransaction.Post(
            LedgerTransactionType.Reversal,
            [Draft(Guid.NewGuid(), 100), Draft(Guid.NewGuid(), -100)],
            Now));
    }

    [Fact]
    public void Post_NonReversalWithOriginalId_Throws()
    {
        Assert.Throws<InvalidLedgerTransactionException>(() => LedgerTransaction.Post(
            LedgerTransactionType.Transfer,
            [Draft(Guid.NewGuid(), 100), Draft(Guid.NewGuid(), -100)],
            Now,
            Guid.NewGuid()));
    }

    [Fact]
    public void CreateReversal_NegatesEveryEntryAndLinksOriginal()
    {
        var alice = Guid.NewGuid();
        var bob = Guid.NewGuid();
        var original = LedgerTransaction.Post(
            LedgerTransactionType.Transfer,
            [Draft(alice, -3000), Draft(bob, 3000)],
            Now);

        var reversal = original.CreateReversal(Now.AddMinutes(5));

        Assert.Equal(LedgerTransactionType.Reversal, reversal.Type);
        Assert.Equal(original.Id, reversal.ReversalOfId);
        Assert.Equal(3000, reversal.Entries.Single(entry => entry.AccountId == alice).Amount.MinorUnits);
        Assert.Equal(-3000, reversal.Entries.Single(entry => entry.AccountId == bob).Amount.MinorUnits);
    }

    [Fact]
    public void CreateReversal_OfReversal_Throws()
    {
        var original = LedgerTransaction.Post(
            LedgerTransactionType.Transfer,
            [Draft(Guid.NewGuid(), -100), Draft(Guid.NewGuid(), 100)],
            Now);
        var reversal = original.CreateReversal(Now);

        Assert.Throws<InvalidLedgerTransactionException>(() => reversal.CreateReversal(Now));
    }
}