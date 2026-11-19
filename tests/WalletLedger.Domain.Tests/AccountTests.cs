namespace WalletLedger.Domain.Tests;

public class AccountTests
{
    private static Account NewUserAccount(Currency? currency = null) =>
        Account.OpenForUser(Guid.NewGuid(), currency ?? Currency.Usd);

    [Fact]
    public void NewAccount_StartsWithZeroBalance()
    {
        Assert.True(NewUserAccount().Balance.IsZero);
    }

    [Fact]
    public void Apply_CreditThenDebitWithinBalance_UpdatesBalance()
    {
        var account = NewUserAccount();

        account.Apply(Money.Of(1000, Currency.Usd));
        account.Apply(Money.Of(-400, Currency.Usd));

        Assert.Equal(Money.Of(600, Currency.Usd), account.Balance);
    }

    [Fact]
    public void Apply_DebitAboveBalance_ThrowsAndKeepsBalance()
    {
        var account = NewUserAccount();
        account.Apply(Money.Of(1000, Currency.Usd));

        Assert.Throws<InsufficientFundsException>(() => account.Apply(Money.Of(-1001, Currency.Usd)));
        Assert.Equal(Money.Of(1000, Currency.Usd), account.Balance);
    }

    [Fact]
    public void Apply_DebitOnClearingAccount_AllowsNegativeBalance()
    {
        var clearing = Account.OpenSystem(AccountKind.SystemClearing, Currency.Usd);

        clearing.Apply(Money.Of(-500, Currency.Usd));

        Assert.Equal(Money.Of(-500, Currency.Usd), clearing.Balance);
    }

    [Fact]
    public void Apply_DebitOnHoldAccount_Throws()
    {
        var hold = Account.OpenSystem(AccountKind.SystemHold, Currency.Usd);

        Assert.Throws<InsufficientFundsException>(() => hold.Apply(Money.Of(-1, Currency.Usd)));
    }

    [Fact]
    public void Apply_DifferentCurrency_Throws()
    {
        var account = NewUserAccount(Currency.Usd);

        Assert.Throws<CurrencyMismatchException>(() => account.Apply(Money.Of(100, Currency.Eur)));
    }

    [Fact]
    public void OpenSystem_WithUserKind_Throws()
    {
        Assert.Throws<InvalidAccountException>(() => Account.OpenSystem(AccountKind.User, Currency.Usd));
    }
}