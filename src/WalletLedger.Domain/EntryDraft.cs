namespace WalletLedger.Domain;

public sealed record EntryDraft(Guid AccountId, Money Amount);