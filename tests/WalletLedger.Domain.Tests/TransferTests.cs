namespace WalletLedger.Domain.Tests;

public class TransferTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 19, 12, 0, 0, TimeSpan.Zero);

    private static Account NewUserAccount(Currency? currency = null) =>
        Account.OpenForUser(Guid.NewGuid(), currency ?? Currency.Usd);

    [Fact]
    public void Create_ValidInput_ReturnsCompletedTransfer()
    {
        var source = NewUserAccount();
        var destination = NewUserAccount();
        var ledgerTransactionId = Guid.NewGuid();

        var transfer = Transfer.Create(
            source, destination, Money.Of(2500, Currency.Usd), ledgerTransactionId, Now);

        Assert.Equal(TransferStatus.Completed, transfer.Status);
        Assert.Equal(source.Id, transfer.SourceAccountId);
        Assert.Equal(destination.Id, transfer.DestinationAccountId);
        Assert.Equal(ledgerTransactionId, transfer.LedgerTransactionId);
    }

    [Fact]
    public void Create_SameAccount_Throws()
    {
        var account = NewUserAccount();

        Assert.Throws<InvalidTransferException>(() => Transfer.Create(
            account, account, Money.Of(100, Currency.Usd), Guid.NewGuid(), Now));
    }

    [Fact]
    public void Create_ZeroAmount_Throws()
    {
        Assert.Throws<InvalidAmountException>(() => Transfer.Create(
            NewUserAccount(), NewUserAccount(), Money.Zero(Currency.Usd), Guid.NewGuid(), Now));
    }

    [Fact]
    public void Create_NegativeAmount_Throws()
    {
        Assert.Throws<InvalidAmountException>(() => Transfer.Create(
            NewUserAccount(), NewUserAccount(), Money.Of(-1, Currency.Usd), Guid.NewGuid(), Now));
    }

    [Fact]
    public void Create_AmountCurrencyDiffersFromSource_Throws()
    {
        Assert.Throws<CurrencyMismatchException>(() => Transfer.Create(
            NewUserAccount(Currency.Usd),
            NewUserAccount(Currency.Eur),
            Money.Of(100, Currency.Eur),
            Guid.NewGuid(),
            Now));
    }

    [Fact]
    public void Create_DestinationInDifferentCurrency_Throws()
    {
        Assert.Throws<CurrencyMismatchException>(() => Transfer.Create(
            NewUserAccount(Currency.Usd),
            NewUserAccount(Currency.Eur),
            Money.Of(100, Currency.Usd),
            Guid.NewGuid(),
            Now));
    }
}