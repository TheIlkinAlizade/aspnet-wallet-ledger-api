namespace WalletLedger.Application.Ledger;

public interface IMoneyMovementService
{
    Task<MovementResult> DepositAsync(Guid userId, Guid accountId, string amount, CancellationToken cancellationToken);
    Task<MovementResult> WithdrawAsync(Guid userId, Guid accountId, string amount, CancellationToken cancellationToken);
}