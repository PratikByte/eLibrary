using eLibrary.Book.Application.Queries.Reports;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eLibrary.Book.API.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
[ApiController]
public class ReportController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("Avail-books")]
    public async Task<IActionResult> GetAvailableBooks([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        return Ok(await _mediator.Send(new GetAvailableBooksQuery(pageNumber, pageSize)));
    }

    [HttpGet("OverDue-books")]
    public async Task<IActionResult> GetOverdueBooks()
    {
        return Ok(await _mediator.Send(new GetBorrowReportQuery()));
    }
}