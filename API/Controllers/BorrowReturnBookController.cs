using EBook.CQRS.Command;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eLibrary.API.Controllers;

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
    // Endpoint to borrow a book
    [HttpPost("borrow")]
    public async Task<IActionResult> BorrowBook([FromBody] BorrowBookCommand command)
    {
        var effectiveUserId = GetEffectiveUserId(command.UserId);
        if (effectiveUserId is null)
        {
            return Forbid();
        }

        var result = await _mediator.Send(new BorrowBookCommand(effectiveUserId.Value, command.BookId));
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    // Endpoint to return a book
    [HttpPost("return")]
    public async Task<IActionResult> ReturnBook([FromBody] ReturnBookCommand command)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var result = await _mediator.Send(
            new ReturnBookCommand(command.BorrowId, currentUserId, User.IsInRole("Admin")));

        if (!result.Success)
        {
            var message = result.Message ?? string.Empty;

            if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(result);

            if (message.Contains("not allowed", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            return BadRequest(result);
        }

        return Ok(result);
    }

    private int? GetEffectiveUserId(int requestedUserId)
    {
        if (User.IsInRole("Admin"))
        {
            return requestedUserId;
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(currentUserId, out var parsedUserId) ? parsedUserId : null;
    }

    private int? GetCurrentUserId()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(currentUserId, out var parsedUserId) ? parsedUserId : null;
    }

}
