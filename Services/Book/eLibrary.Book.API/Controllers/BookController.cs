using eLibrary.Book.Application.Commands.Books;
using eLibrary.Book.Application.DTOs;
using eLibrary.Book.Application.Queries.Books;
using eLibrary.Book.Application.Shared;
using eLibrary.Book.Domain.Pagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eLibrary.Book.API.Controllers;

/// <summary>
/// Book Service API endpoints.
/// This controller owns all operations related to books only.
/// </summary>
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class BookController : ControllerBase
{
    private readonly IMediator _mediator;

    public BookController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get books with pagination.
    /// </summary>
    [Authorize(Roles = "User,Admin")]
    [HttpGet("paged")]
    public async Task<IActionResult> GetBooksPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        var result = await _mediator.Send(new GetBooksQuery(pageNumber, pageSize));
        return Ok(result);
    }

    /// <summary>
    /// Get a single book by id.
    /// </summary>
    [Authorize(Roles = "User,Admin")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetBookById(int id)
    {
        var result = await _mediator.Send(new GetBookByIdQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// Get books by author with pagination.
    /// </summary>
    [Authorize(Roles = "User,Admin")]
    [HttpGet("author")]
    public async Task<IActionResult> GetBookByAuthor([FromQuery] string author, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        var result = await _mediator.Send(new GetBookByAuthorQuery(author, pageNumber, pageSize));
        return Ok(result);
    }

    /// <summary>
    /// Get books by price range.
    /// </summary>
    [Authorize(Roles = "User,Admin")]
    [HttpGet("price-range")]
    public async Task<IActionResult> GetBooksByPriceRange([FromQuery] float minPrice, [FromQuery] float maxPrice)
    {
        var result = await _mediator.Send(new GetBooksByPriceRangeQuery(minPrice, maxPrice));
        return Ok(result);
    }

    /// <summary>
    /// Create a new book.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("add-book")]
    public async Task<IActionResult> AddBook([FromForm] CreateBookDto bookDto)
    {
        var result = await _mediator.Send(new AddBookCommand(bookDto));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Update the price of an existing book.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("update-price")]
    public async Task<IActionResult> UpdateBookPrice([FromBody] UpdateBookPriceCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Update the cover image of an existing book.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/cover")]
    public async Task<IActionResult> UpdateBookCover(int id, IFormFile? coverImage)
    {
        if (coverImage is null)
        {
            return BadRequest(ApiResponse<bool>.Fail("A cover image is required."));
        }

        var result = await _mediator.Send(new UpdateBookCoverCommand(id, coverImage));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete the cover image for a book.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}/cover")]
    public async Task<IActionResult> DeleteBookCover(int id)
    {
        var result = await _mediator.Send(new DeleteBookCoverCommand(id));
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Delete a book record.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var result = await _mediator.Send(new DeleteBookCommand(id));
        return result.Success ? Ok(result) : NotFound(result);
    }
}
