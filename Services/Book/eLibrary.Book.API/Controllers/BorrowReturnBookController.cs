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
    [HttpGet("transactions")]
    public async Task<IActionResult> GetRecentTransactions(
        [FromQuery] int count = 20,
        CancellationToken cancellationToken = default)
    {
        if (count is < 1 or > 100)
            return BadRequest(new { message = "Count must be between 1 and 100." });

        var result = await _mediator.Send(
            new GetRecentBorrowTransactionsQuery(count),
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
        return result.Success ? Ok(result) : BadRequest(result);
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

        if (result.Success)
            return Ok(result);
        if (result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
            return NotFound(result);
        if (result.Message?.Contains("not allowed", StringComparison.OrdinalIgnoreCase) == true)
            return Forbid();
        return BadRequest(result);
    }

    private Guid? GetEffectiveUserId(Guid requestedUserId) =>
        User.IsInRole("Admin") ? requestedUserId : GetCurrentUserId();

    private Guid? GetCurrentUserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;

    private string GetUserName() => User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Name) ?? "User";
}
