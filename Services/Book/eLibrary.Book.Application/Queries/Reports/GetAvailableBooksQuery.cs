using eLibrary.Book.Application.DTOs;
using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Domain.Pagination;
using MediatR;

namespace eLibrary.Book.Application.Queries.Reports;

public record GetAvailableBooksQuery(int PageNumber, int PageSize) : IRequest<PagedResponse<BookDto>>;

public class GetAvailableBooksQueryHandler : IRequestHandler<GetAvailableBooksQuery, PagedResponse<BookDto>>
{
    private readonly IBookRepository _bookRepository;

    public GetAvailableBooksQueryHandler(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<PagedResponse<BookDto>> Handle(GetAvailableBooksQuery request, CancellationToken cancellationToken)
    {
        var (books, totalRecords) = await _bookRepository.GetAvailBookAsync(
            request.PageNumber, request.PageSize, cancellationToken);

        var items = books.Select(book => new BookDto
        {
            Id = book.Id,
            Title = book.Title ?? string.Empty,
            Author = book.Author ?? string.Empty,
            Price = book.Price,
            AvailableCount = book.AvailableCount,
            CoverImageUrl = book.CoverImageMongoId == null ? null : $"/api/Book/{book.Id}/cover"
        }).ToList();

        return new PagedResponse<BookDto>(items, totalRecords, request.PageNumber, request.PageSize,
            items.Count == 0 ? "No books found." : "Books retrieved successfully.");
    }
}