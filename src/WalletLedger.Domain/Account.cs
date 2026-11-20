namespace WalletLedger.Domain;

public sealed class Account
{
    private Account()
    {
        Currency = null!;
    }

    private Account(Guid id, Guid? ownerUserId, Currency currency, AccountKind kind)
    {
        Id = id;
        OwnerUserId = ownerUserId;
        Currency = currency;
        Kind = kind;
    }

    public Guid Id { get; private set; }
    public Guid? OwnerUserId { get; private set; }
    public Currency Currency { get; private set; }
    public AccountKind Kind { get; private set; }
    public long BalanceMinorUnits { get; private set; }
    public Money Balance => Money.Of(BalanceMinorUnits, Currency);
    public bool AllowsNegativeBalance => Kind == AccountKind.SystemClearing;

    public static Account OpenForUser(Guid ownerUserId, Currency currency) =>
        new(Guid.CreateVersion7(), ownerUserId, currency, AccountKind.User);

    public static Account OpenSystem(AccountKind kind, Currency currency)
    {
        if (kind == AccountKind.User)
        {
            throw new InvalidAccountException("A system account cannot have the User kind.");
        }

        return new Account(Guid.CreateVersion7(), null, currency, kind);
    }

    public void Apply(Money amount)
    {
        var newBalance = Balance.Add(amount);

        if (newBalance.IsNegative && !AllowsNegativeBalance)
        {
            throw new InsufficientFundsException(Id);
        }

        BalanceMinorUnits = newBalance.MinorUnits;
    }
}