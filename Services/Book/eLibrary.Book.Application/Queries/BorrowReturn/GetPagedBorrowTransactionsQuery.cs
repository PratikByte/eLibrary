using eLibrary.Book.Application.DTOs;
using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Domain.Pagination;
using MediatR;

namespace eLibrary.Book.Application.Queries.BorrowReturn;

public record GetPagedBorrowTransactionsQuery(int PageNumber, int PageSize)
    : IRequest<PagedResponse<BorrowTransactionDto>>;

public class GetPagedBorrowTransactionsQueryHandler
    : IRequestHandler<GetPagedBorrowTransactionsQuery, PagedResponse<BorrowTransactionDto>>
{
    private readonly IBorrowRepository _borrowRepository;

    public GetPagedBorrowTransactionsQueryHandler(IBorrowRepository borrowRepository)
    {
        _borrowRepository = borrowRepository;
    }

    public async Task<PagedResponse<BorrowTransactionDto>> Handle(
        GetPagedBorrowTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var (transactions, totalRecords) = await _borrowRepository.GetRecentPagedAsync(
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return new PagedResponse<BorrowTransactionDto>(
            transactions,
            totalRecords,
            request.PageNumber,
            request.PageSize,
            "Recent borrow transactions fetched successfully.");
    }
}
