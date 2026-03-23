using eLibrary.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using eLibrary.Application.Commands.Books;
using eLibrary.Application.Queries.Books;
using eLibrary.Domain.Pagination;
using eLibrary.Shared;
using Microsoft.AspNetCore.Authorization;
namespace eLibrary.API.Controllers;

[Authorize]
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
    [Authorize(Roles = "User,Admin")]
    [HttpGet("paged")] 
    public async Task<ActionResult<PagedResponse<BookDto>>> GetBooksPaged(
                                                                  [FromQuery] int pageNumber = 1,
                                                                        [FromQuery] int pageSize = 5,
                                                                             CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetBooksQuery(pageNumber, pageSize));
        return Ok(result);
    }

   // Endpoint to add a new book 
   [Authorize(Roles = "Admin")]
    [HttpPost("add-book")]
    public async Task<ActionResult<ApiResponse<int>>> AddBook([FromForm] CreateBookDto bookDto)
    {
        var command = new AddBookCommand(bookDto);
        var response = await _mediator.Send(command);

        return Ok(response);
    }

   
    // Endpoint to update book cover image
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/cover")]
    public async Task<ActionResult<ApiResponse<BookDto>>> UpdateCover(int id, IFormFile coverImage)
    {
        var response=await _mediator.Send(new UpdateBookCoverCommand(id, coverImage));
        return Ok(response);
    }

   
    // Endpoint to delete book cover image
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}/cover")]
    public async Task<ActionResult<ApiResponse<bool>>>DeleteCover(int id)
    {
        var response = await _mediator.Send(new DeleteBookCoverCommand(id));
        return Ok(response);
    }

   
    // Endpoint to get books by author with pagination
    [Authorize(Roles = "User,Admin")]
    [HttpGet("author")]
    public async Task<ActionResult<ApiResponse<List<BookDto>>>> GetBookByAuthor([FromQuery]string author,[FromQuery] int pageNumber = 1,
                                                                        [FromQuery] int pageSize = 5)
    {
        var response = await _mediator.Send(new GetBookByAuthorQuery(author,pageNumber,pageSize));
        return Ok(response);
    }

   
    // Endpoint to get books by ID
    [Authorize(Roles = "User,Admin")]
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

    // Step 1: ask the application layer for the cover file details.
    // Step 2: return PhysicalFile so the browser downloads the image instead of previewing it.
    
    //[Authorize(Roles = "User,Admin")]
    // [HttpGet("{bookId}/cover/download")] //user
    // public async Task<IActionResult> DownloadCover(int bookId, CancellationToken cancellationToken)
    // {
    //     var response = await _mediator.Send(new GetBookCoverDownloadQuery(bookId), cancellationToken);
    //     if (!response.Success || response.Data == null)
    //     {
    //         return NotFound(response);
    //     }

    //     return PhysicalFile(response.Data.FilePath, response.Data.ContentType, response.Data.FileName);
    // }

    // Endpoint to get books by price range
    [Authorize(Roles = "User,Admin")]
    [HttpGet("price-range")] 
    public async Task<ActionResult<ApiResponse<List<BookWithPriceDto>>>> GetBooksByPriceRange([FromQuery] GetBooksByPriceRangeQuery query)               

    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // Endpoint to update book price
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
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

