using eLibrary.Book.Application.DTOs;
using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Application.Shared;
using MediatR;

namespace eLibrary.Book.Application.Queries.BorrowReturn;

public record GetRecentBorrowTransactionsQuery(int Count = 20)
    : IRequest<ApiResponse<List<BorrowTransactionDto>>>;

public class GetRecentBorrowTransactionsQueryHandler
    : IRequestHandler<GetRecentBorrowTransactionsQuery, ApiResponse<List<BorrowTransactionDto>>>
{
    private readonly IBorrowRepository _borrowRepository;

    public GetRecentBorrowTransactionsQueryHandler(IBorrowRepository borrowRepository)
    {
        _borrowRepository = borrowRepository;
    }

    public async Task<ApiResponse<List<BorrowTransactionDto>>> Handle(
        GetRecentBorrowTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var transactions = await _borrowRepository.GetRecentAsync(request.Count, cancellationToken);
        return ApiResponse<List<BorrowTransactionDto>>.Ok(
            transactions,
            "Recent borrow transactions fetched successfully.");
    }
}
