using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WEX.Application.Features.Transactions.Commands.StorePurchaseTransaction;

namespace WEX.API.Controllers.v1;

/// <summary>
/// Manages purchase transaction operations.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/transactions")]
public sealed class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Stores a new purchase transaction.
    /// </summary>
    /// <param name="request">The transaction details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 Created with Location header pointing to the new transaction.</returns>
    /// <response code="201">Transaction stored successfully.</response>
    /// <response code="400">Validation error — one or more fields are invalid.</response>
    [HttpPost]
    [ProducesResponseType(typeof(StorePurchaseTransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StoreTransaction(
        [FromBody] StorePurchaseTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new StorePurchaseTransactionCommand(
            request.Description,
            request.TransactionDate,
            request.AmountUsd);

        var id = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            actionName: null,
            routeValues: new { id },
            value: new StorePurchaseTransactionResponse(id));
    }
}

/// <summary>Request model for storing a purchase transaction.</summary>
public sealed record StorePurchaseTransactionRequest(
    string Description,
    DateOnly TransactionDate,
    decimal AmountUsd);

/// <summary>Response model returned after storing a purchase transaction.</summary>
public sealed record StorePurchaseTransactionResponse(Guid Id);
