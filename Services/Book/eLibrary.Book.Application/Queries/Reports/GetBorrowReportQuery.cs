using eLibrary.Book.Application.Common.Helpers;
using eLibrary.Book.Application.DTOs;
using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Application.Shared;
using MediatR;

namespace eLibrary.Book.Application.Queries.Reports;

public record GetBorrowReportQuery : IRequest<ApiResponse<List<BorrowRecordDto>>>;

public class GetBorrowReportQueryHandler : IRequestHandler<GetBorrowReportQuery, ApiResponse<List<BorrowRecordDto>>>
{
    private readonly IBorrowRepository _borrowRepository;

    public GetBorrowReportQueryHandler(IBorrowRepository borrowRepository)
    {
        _borrowRepository = borrowRepository;
    }

    public async Task<ApiResponse<List<BorrowRecordDto>>> Handle(GetBorrowReportQuery request, CancellationToken cancellationToken)
    {
        var records = await _borrowRepository.GetOverdueAsync(cancellationToken);
        var dtos = new List<BorrowRecordDto>();
        var asOfDate = DateTime.UtcNow;

        foreach (var record in records)
        {
            dtos.Add(new BorrowRecordDto
            {
                Id = record.Id,
                UserId = record.UserId,
            UserName = record.UserName,
                BookId = record.BookId,
            BookName = record.BookName,
                BorrowDate = record.BorrowDate.ToString("yyyy-MM-dd"),
                DueDate = record.DueDate.ToString("yyyy-MM-dd"),
                ReturnDate = record.ReturnDate?.ToString("yyyy-MM-dd") ?? "Not Returned",
                IsReturned = record.IsReturned,
                Fine = BorrowFineCalculator.Calculate(record.DueDate, asOfDate)
            });
        }

        return ApiResponse<List<BorrowRecordDto>>.Ok(dtos, "Overdue records fetched successfully.");
    }
}