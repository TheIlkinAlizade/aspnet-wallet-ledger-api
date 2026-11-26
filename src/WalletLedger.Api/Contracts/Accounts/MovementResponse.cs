using WalletLedger.Application.Ledger;

namespace WalletLedger.Api.Contracts.Accounts;

public sealed record MovementResponse(Guid TransactionId, AccountResponse Account)
{
    public static MovementResponse From(MovementResult result) =>
        new(result.TransactionId, AccountResponse.From(result.Account));
}