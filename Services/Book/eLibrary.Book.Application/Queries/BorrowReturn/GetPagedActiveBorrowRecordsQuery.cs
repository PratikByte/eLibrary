using eLibrary.Book.Application.DTOs;
using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Domain.Pagination;
using MediatR;

namespace eLibrary.Book.Application.Queries.BorrowReturn;

public record GetPagedActiveBorrowRecordsQuery(int PageNumber, int PageSize)
    : IRequest<PagedResponse<ActiveBorrowRecordDto>>;

public class GetPagedActiveBorrowRecordsQueryHandler
    : IRequestHandler<GetPagedActiveBorrowRecordsQuery, PagedResponse<ActiveBorrowRecordDto>>
{
    private readonly IBorrowRepository _borrowRepository;

    public GetPagedActiveBorrowRecordsQueryHandler(IBorrowRepository borrowRepository)
    {
        _borrowRepository = borrowRepository;
    }

    public async Task<PagedResponse<ActiveBorrowRecordDto>> Handle(
        GetPagedActiveBorrowRecordsQuery request,
        CancellationToken cancellationToken)
    {
        var (records, totalRecords) = await _borrowRepository.GetActivePagedAsync(
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return new PagedResponse<ActiveBorrowRecordDto>(
            records,
            totalRecords,
            request.PageNumber,
            request.PageSize,
            "Active borrow records fetched successfully.");
    }
}
