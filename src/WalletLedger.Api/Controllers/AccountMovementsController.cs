using Microsoft.AspNetCore.Mvc;
using WalletLedger.Api.Authentication;
using WalletLedger.Api.Contracts.Accounts;
using WalletLedger.Application.Ledger;

namespace WalletLedger.Api.Controllers;

[ApiController]
[Route("api/accounts/{accountId:guid}")]
public sealed class AccountMovementsController(IMoneyMovementService movements) : ControllerBase
{
    [HttpPost("deposits")]
    public async Task<ActionResult<MovementResponse>> Deposit(
        Guid accountId, AmountRequest request, CancellationToken cancellationToken)
    {
        var result = await movements.DepositAsync(User.GetUserId(), accountId, request.Amount, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, MovementResponse.From(result));
    }

    [HttpPost("withdrawals")]
    public async Task<ActionResult<MovementResponse>> Withdraw(
        Guid accountId, AmountRequest request, CancellationToken cancellationToken)
    {
        var result = await movements.WithdrawAsync(User.GetUserId(), accountId, request.Amount, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, MovementResponse.From(result));
    }
}