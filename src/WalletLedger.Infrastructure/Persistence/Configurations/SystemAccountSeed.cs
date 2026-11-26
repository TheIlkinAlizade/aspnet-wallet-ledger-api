using WalletLedger.Domain;

namespace WalletLedger.Infrastructure.Persistence.Configurations;

internal static class SystemAccountSeed
{
    private static readonly Currency[] Currencies = [Currency.Azn, Currency.Usd, Currency.Eur];
    private static readonly AccountKind[] Kinds = [AccountKind.SystemClearing, AccountKind.SystemHold];

    public static IReadOnlyList<object> Rows() =>
        Currencies
            .SelectMany(currency => Kinds, (currency, kind) => (currency, kind))
            .Select((pair, index) => (object)new
            {
                Id = Guid.Parse($"00000000-0000-0000-0001-{index + 1:D12}"),
                OwnerUserId = (Guid?)null,
                Currency = pair.currency,
                Kind = pair.kind,
                BalanceMinorUnits = 0L
            })
            .ToList();
}