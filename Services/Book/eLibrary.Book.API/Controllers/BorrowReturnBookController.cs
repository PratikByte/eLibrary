using System.Security.Claims;
using eLibrary.Book.Application.Commands.BorrowReturn;
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

    private int? GetEffectiveUserId(int requestedUserId) =>
        User.IsInRole("Admin") ? requestedUserId : GetCurrentUserId();

    private int? GetCurrentUserId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;

    private string GetUserName() => User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Name) ?? "User";
}