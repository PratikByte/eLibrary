using System.Security.Claims;
using eLibrary.Book.Application.Commands.BorrowReturn;
using eLibrary.Book.Application.Queries.BorrowReturn;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eLibrary.Book.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class BorrowReturnBookController : ControllerBase
{
    private readonly IMediator _mediator;

    public BorrowReturnBookController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveBorrowRecords(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetActiveBorrowRecordsQuery(), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("active/paged")]
    public async Task<IActionResult> GetActiveBorrowRecordsPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize is < 1 or > 100)
            return BadRequest(new { message = "Page number must be positive and page size must be between 1 and 100." });

        var result = await _mediator.Send(
            new GetPagedActiveBorrowRecordsQuery(pageNumber, pageSize),
            cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("transactions/paged")]
    public async Task<IActionResult> GetRecentTransactionsPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize is < 1 or > 100)
            return BadRequest(new { message = "Page number must be positive and page size must be between 1 and 100." });

        var result = await _mediator.Send(
            new GetPagedBorrowTransactionsQuery(pageNumber, pageSize),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("borrow")]
    public async Task<IActionResult> BorrowBook([FromBody] BorrowBookCommand command)
    {
        var userId = GetEffectiveUserId(command.UserId);
        if (userId is null)
            return Forbid();

        var result = await _mediator.Send(command with { UserId = userId.Value, UserName = GetUserName() });
        return Ok(result);
    }

    [HttpPost("return")]
    public async Task<IActionResult> ReturnBook([FromBody] ReturnBookCommand command)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null && !User.IsInRole("Admin"))
            return Forbid();

        var result = await _mediator.Send(command with
        {
            CurrentUserId = currentUserId,
            IsAdmin = User.IsInRole("Admin"),
            UserName = GetUserName()
        });

        return Ok(result);
    }

    private Guid? GetEffectiveUserId(Guid requestedUserId) =>
        User.IsInRole("Admin") ? requestedUserId : GetCurrentUserId();

    private Guid? GetCurrentUserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;

    private string GetUserName() => User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Name) ?? "User";
}
