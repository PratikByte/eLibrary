using eLibrary.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using eLibrary.Application.Commands.Books;
using eLibrary.Application.Queries.Books;
using eLibrary.Domain.Pagination;
using eLibrary.Shared;
namespace eLibrary.API.Controllers;


[Route("api/[controller]")]
[ApiController]

public class BookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger _logger;

        public BookController(IMediator mediator, ILogger<BookController> logger)
        {
            _mediator = mediator;
            _logger = logger;
       
        }

    //correct cqrs
    [HttpGet("paged")]
    public async Task<ActionResult<PagedResponse<BookDto>>> GetBooksPaged(
                                                                  [FromQuery] int pageNumber = 1,
                                                                        [FromQuery] int pageSize = 5,
                                                                             CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetBooksQuery(pageNumber, pageSize));
        return Ok(result);
    }

//correct cqrs
    [HttpGet("availBook")]
    public async Task<ActionResult<PagedResponse<BookDto>>> GetAvailBook(
                                                                  [FromQuery] int pageNumber = 1,
                                                                        [FromQuery] int pageSize = 5,
                                                                             CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAvailableBooksQuery(pageNumber, pageSize));
        return Ok(result);
    }




    //correct cqrs
    [HttpPost("AddBook")]
    public async Task<ActionResult<ApiResponse<int>>> AddBook([FromForm] CreateBookDto bookDto)
    {
        var command = new AddBookCommand(bookDto);
        var response = await _mediator.Send(command);

        return Ok(response);
    }

    //correct cqrs
    //update cover image
    [HttpPut("{id}/cover")]
    public async Task<ActionResult<ApiResponse<BookDto>>> UpdateCover(int id, IFormFile coverImage)
    {
        var response=await _mediator.Send(new UpdateBookCoverCommand(id, coverImage));
        return Ok(response);
    }

    //correct cqrs
    //delete cover image
    [HttpDelete("{id}/cover")]
    public async Task<ActionResult<ApiResponse<bool>>>DeleteCover(int id)
    {
        var response = await _mediator.Send(new DeleteBookCoverCommand(id));
        return Ok(response);
    }

    //correct cqrs
    //controller for getting data BY author
    [HttpGet("author")]


    public async Task<ActionResult<ApiResponse<List<BookDto>>>> GetBookByAuthor(string author,[FromQuery] int pageNumber = 1,
                                                                        [FromQuery] int pageSize = 5)
    {
        var response = await _mediator.Send(new GetBookByAuthorQuery(author,pageNumber,pageSize));
        return Ok(response);
    }

    //correct cqrs
    //controller for getting book data BY Id  
    [HttpGet("{Bookid}")]

    public async Task<ActionResult<ApiResponse<BookDto>>> GetBookById(int Bookid)
    {
        var resonse = await _mediator.Send(new GetBookByIdQuery(Bookid));
        if (!resonse.Success)
        {
            return NotFound(resonse);
        }
        return Ok(resonse);
    }



    //i have confusued with this code
    //getdata book bet range price

    // [ServiceFilter(typeof(LogActionFilter))]  // Applied filter to this specific action
    [HttpGet("Get_Betn_range")]
    public async Task<ActionResult<ApiResponse<List<BookWithPriceDto>>>> GetBooksByPriceRange([FromQuery] GetBooksByPriceRangeQuery query)               //why used from query here?? istead of FromBody

    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    //check for implementing cqrs correctly resume endpoint

    [HttpPut("update-price")]
    public async Task<ActionResult<ApiResponse<BookDto>>> UpdateBookPrice([FromBody] UpdateBookPriceCommand command)
    {
        var response = await _mediator.Send(command);
        if (!response.Success)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }



    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteBook(int id)
    {
        var response = await _mediator.Send(new DeleteBookByIDCommand(id));
        if (!response.Success)
        {
            return NotFound(response);
        }
        return Ok(response);
    }

    //Correct wrokin confiremed pratik
    //where we can use it correclty i need to refractro logic in borrow and return book
    //
    // [HttpPut("increment")]
    // public async Task<ActionResult<ApiResponse<bool>>> IncrementAvailableCount(int id)
    // {
    //     return await _bookRepository.IncreaseAvailBookCountAsync(id, CancellationToken.None) ?
    //         Ok(ApiResponse<bool>.Ok(true, "Available count incremented.")) :
    //         NotFound(ApiResponse<bool>.Fail("Book not found."));
    // }
    // [HttpPut("decrement")]
    // public async Task<ActionResult<ApiResponse<bool>>> DecrementAvailableCount(int id)
    // {
    //     return await _bookRepository.DecreaseAvailBookCountAsync(id, CancellationToken.None) ?
    //         Ok(ApiResponse<bool>.Ok(true, "Available count Decremented.")) :
    //         NotFound(ApiResponse<bool>.Fail("Book not found."));
    // }


}

