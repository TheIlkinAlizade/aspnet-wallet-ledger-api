using Microsoft.AspNetCore.Mvc;
using WalletLedger.Api.Authentication;
using WalletLedger.Api.Contracts.Accounts;
using WalletLedger.Application.Accounts;

namespace WalletLedger.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public sealed class AccountsController(IAccountService accountService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AccountResponse>> Open(
        OpenAccountRequest request, CancellationToken cancellationToken)
    {
        var account = await accountService.OpenAsync(User.GetUserId(), request.Currency, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = account.Id }, AccountResponse.From(account));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AccountResponse>>> List(CancellationToken cancellationToken)
    {
        var accounts = await accountService.ListAsync(User.GetUserId(), cancellationToken);
        return Ok(accounts.Select(AccountResponse.From).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AccountResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var account = await accountService.GetAsync(User.GetUserId(), id, cancellationToken);
        return Ok(AccountResponse.From(account));
    }
}