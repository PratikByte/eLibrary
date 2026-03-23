using eLibrary.Application.DTOs;
using eLibrary.Application.Queries.Books;
using eLibrary.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eLibrary.API.Controllers;

[Route("api/[controller]")]
[ApiController]
 [Authorize(Roles = "Admin")]
public class ReportController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Endpoint to get available books with pagination
    [HttpGet("Avail-books")]
    public async Task<ActionResult<ApiResponse<List<BookDto>>>> GetAvailBooks(
                                                                  [FromQuery] int pageNumber = 1,
                                                                        [FromQuery] int pageSize = 5,
                                                                             CancellationToken cancellationToken = default)
    {
        var report = await _mediator.Send(new GetAvailableBooksQuery(pageNumber, pageSize));

        return Ok(report);
    }

    
    // Endpoint to get overdue books report
    [HttpGet("OverDue-books")]
    public async Task<ActionResult<ApiResponse<List<BorrowRecordDto>>>> GetOverDueReport()
    {
        var report= await _mediator.Send(new GetBorrowReportQuery());
        return Ok(report);
    }
}

