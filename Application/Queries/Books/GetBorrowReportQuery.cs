using MediatR;
using eLibrary.Application.DTOs;
using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Shared;

namespace eLibrary.Application.Queries.Books;

//public class GetBorrowReportQuery: IRequest<ApiResponse<List<BorrowRecordsDto>>>
//{

//}

public record GetBorrowReportQuery() : IRequest<ApiResponse<List<BorrowRecordDto>>>;

public class GetBorrowReportQueryHandler : IRequestHandler<GetBorrowReportQuery, ApiResponse<List<BorrowRecordDto>>>
{

    private readonly IBorrowRepository _borrowRepository;

    public GetBorrowReportQueryHandler(IBorrowRepository borrowRepository)
    {
        _borrowRepository = borrowRepository;
    }

    public async Task<ApiResponse<List<BorrowRecordDto>>> Handle(GetBorrowReportQuery request, CancellationToken cancellationToken)
    {
        var borrowRecords = await _borrowRepository.GetOverdueAsync();
        var borrowRecordsDto = new List<BorrowRecordDto>();

        foreach (var br in borrowRecords)
        {
            var dto = new BorrowRecordDto
            {
                Id = br.Id,
                UserId = br.UserId,
                UserName = br.User?.Username ?? "Unknown User",
                BookId = br.BookId,
                BookName = br.Book?.Title ?? "Unknown Book",
                BorrowDate = br.BorrowDate.ToString("yyyy-MM-dd"),
                DueDate = br.DueDate.ToString("yyyy-MM-dd"),
                ReturnDate = br.ReturnDate?.ToString("yyyy-MM-dd") ?? "Not Returned",
                IsReturned = br.IsReturned,
                Fine = br.Fine
            };
            borrowRecordsDto.Add(dto);
        }


    return ApiResponse<List<BorrowRecordDto>>.Ok(borrowRecordsDto, "Overdue  records fetched successfully");
    }
}

