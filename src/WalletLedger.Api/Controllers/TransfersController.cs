using Microsoft.AspNetCore.Mvc;
using WalletLedger.Api.Authentication;
using WalletLedger.Api.Contracts.Transfers;
using WalletLedger.Application.Transfers;

namespace WalletLedger.Api.Controllers;

[ApiController]
[Route("api/transfers")]
public sealed class TransfersController(ITransferService transfers) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TransferResponse>> Create(
        TransferRequest request, CancellationToken cancellationToken)
    {
        var transfer = await transfers.TransferAsync(
            User.GetUserId(),
            request.SourceAccountId!.Value,
            request.DestinationAccountId!.Value,
            request.Amount,
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, TransferResponse.From(transfer));
    }
}