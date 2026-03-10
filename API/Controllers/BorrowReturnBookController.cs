using EBook.CQRS.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace eLibrary.API.Controllers;

// [Authorize]
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
        var result = await _mediator.Send(command);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    // Endpoint to return a book
    [HttpPost("return")]
    public async Task<IActionResult> ReturnBook([FromBody] ReturnBookCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

}

