using WalletLedger.Application.Accounts;

namespace WalletLedger.Application.Ledger;

public sealed record MovementResult(Guid TransactionId, AccountView Account);