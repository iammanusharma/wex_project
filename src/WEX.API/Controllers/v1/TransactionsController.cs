using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WEX.Application.Features.Transactions.Commands.StorePurchaseTransaction;
using WEX.Application.Features.Transactions.Queries.GetPurchaseInCurrency;

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
            actionName: nameof(GetTransactionInCurrency),
            routeValues: new { id },
            value: new StorePurchaseTransactionResponse(id));
    }

    /// <summary>
    /// Retrieves a stored purchase transaction converted to the specified currency.
    /// Uses the Treasury Reporting Rates of Exchange API to find the rate active
    /// at the time of purchase (within the last 6 months).
    /// </summary>
    /// <param name="id">The transaction unique identifier.</param>
    /// <param name="currency">3-letter ISO 4217 currency code (e.g. EUR, GBP, CAD).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Transaction details with converted amount.</returns>
    /// <response code="200">Transaction retrieved and converted successfully.</response>
    /// <response code="400">Invalid transaction ID or currency code.</response>
    /// <response code="404">Transaction not found.</response>
    /// <response code="422">Exchange rate not available for the given currency and purchase date.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetPurchaseInCurrencyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetTransactionInCurrency(
        [FromRoute] Guid id,
        [FromQuery] string currency,
        CancellationToken cancellationToken)
    {
        var query = new GetPurchaseInCurrencyQuery(id, currency);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}

/// <summary>Request model for storing a purchase transaction.</summary>
public sealed record StorePurchaseTransactionRequest(
    string Description,
    DateOnly TransactionDate,
    decimal AmountUsd);

/// <summary>Response model returned after storing a purchase transaction.</summary>
public sealed record StorePurchaseTransactionResponse(Guid Id);
