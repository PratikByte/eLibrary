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

    // Endpoint to get all books with pagination
    [HttpGet("paged")]
    public async Task<ActionResult<PagedResponse<BookDto>>> GetBooksPaged(
                                                                  [FromQuery] int pageNumber = 1,
                                                                        [FromQuery] int pageSize = 5,
                                                                             CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetBooksQuery(pageNumber, pageSize));
        return Ok(result);
    }

    // Endpoint to get only available books with pagination
    [HttpGet("availBook")]
    public async Task<ActionResult<PagedResponse<BookDto>>> GetAvailBook(
                                                                  [FromQuery] int pageNumber = 1,
                                                                        [FromQuery] int pageSize = 5,
                                                                             CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAvailableBooksQuery(pageNumber, pageSize));
        return Ok(result);
    }




   // Endpoint to add a new book 
    [HttpPost("AddBook")]
    public async Task<ActionResult<ApiResponse<int>>> AddBook([FromForm] CreateBookDto bookDto)
    {
        var command = new AddBookCommand(bookDto);
        var response = await _mediator.Send(command);

        return Ok(response);
    }

   
    // Endpoint to update book cover image
    [HttpPut("{id}/cover")]
    public async Task<ActionResult<ApiResponse<BookDto>>> UpdateCover(int id, IFormFile coverImage)
    {
        var response=await _mediator.Send(new UpdateBookCoverCommand(id, coverImage));
        return Ok(response);
    }

   
    // Endpoint to delete book cover image
    [HttpDelete("{id}/cover")]
    public async Task<ActionResult<ApiResponse<bool>>>DeleteCover(int id)
    {
        var response = await _mediator.Send(new DeleteBookCoverCommand(id));
        return Ok(response);
    }

   
    // Endpoint to get books by author with pagination
    [HttpGet("author")]
    public async Task<ActionResult<ApiResponse<List<BookDto>>>> GetBookByAuthor(string author,[FromQuery] int pageNumber = 1,
                                                                        [FromQuery] int pageSize = 5)
    {
        var response = await _mediator.Send(new GetBookByAuthorQuery(author,pageNumber,pageSize));
        return Ok(response);
    }

   
    // Endpoint to get books by ID
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

    // Endpoint to get books by price range
    [HttpGet("Get_Betn_range")]
    public async Task<ActionResult<ApiResponse<List<BookWithPriceDto>>>> GetBooksByPriceRange([FromQuery] GetBooksByPriceRangeQuery query)               

    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // Endpoint to update book price
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


    // Endpoint to delete a book by ID
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
}

