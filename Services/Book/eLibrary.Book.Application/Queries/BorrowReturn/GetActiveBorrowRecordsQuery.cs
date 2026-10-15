using eLibrary.Book.Application.DTOs;
using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Application.Shared;
using MediatR;

namespace eLibrary.Book.Application.Queries.BorrowReturn;

public record GetActiveBorrowRecordsQuery : IRequest<ApiResponse<List<ActiveBorrowRecordDto>>>;

public class GetActiveBorrowRecordsQueryHandler
    : IRequestHandler<GetActiveBorrowRecordsQuery, ApiResponse<List<ActiveBorrowRecordDto>>>
{
    private readonly IBorrowRepository _borrowRepository;

    public GetActiveBorrowRecordsQueryHandler(IBorrowRepository borrowRepository)
    {
        _borrowRepository = borrowRepository;
    }

    public async Task<ApiResponse<List<ActiveBorrowRecordDto>>> Handle(
        GetActiveBorrowRecordsQuery request,
        CancellationToken cancellationToken)
    {
        var records = await _borrowRepository.GetActiveAsync(cancellationToken);
        return ApiResponse<List<ActiveBorrowRecordDto>>.Ok(records, "Active borrow records fetched successfully.");
    }
}
