using eLibrary.Application.DTOs;
using MediatR;
using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Domain.Pagination;
using eLibrary.Application.Common.Helpers;

namespace eLibrary.Application.Queries.Books;

public class GetBookByAuthorQuery: IRequest<PagedResponse<BookDto>>
{
    public string Author { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }

    public GetBookByAuthorQuery(string author, int pageNumber, int pageSize)
    {
        Author = author;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

}
public class GetBookByAuthorHandler : IRequestHandler<GetBookByAuthorQuery, PagedResponse<BookDto>>
{   
    public readonly IBookRepository _bookRepository;
    private readonly ILogger<GetBookByAuthorHandler> _logger;

    public GetBookByAuthorHandler(IBookRepository bookRepository, ILogger<GetBookByAuthorHandler> logger)
    {
        _bookRepository = bookRepository;
        _logger = logger;
    }

    public async Task<PagedResponse<BookDto>> Handle(GetBookByAuthorQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (books, totalRecords) = await _bookRepository.PagedGetBookByAuthorAsync(request.PageNumber,request.PageSize,request.Author, cancellationToken);
            if (books == null || books.Count == 0)
            {
                return  new PagedResponse<BookDto>(
                    new List<BookDto>(),
                    0, request.PageNumber, request.PageSize,
                    "No books found.");
            }
            var bookDtos = BookHelper.MapBooksToDto(books);
            return  new PagedResponse<BookDto>(
                bookDtos,
                totalRecords,
                request.PageNumber,
                request.PageSize,
                "Books retrieved successfully.");

        }
        catch (Exception ex)
        {   
            return new PagedResponse<BookDto>(
                new List<BookDto>(),
                0, request.PageNumber, request.PageSize,
                $"An error occurred: {ex.Message}")
            {
                Success = false,
                Errors = new[] { ex.Message }
            };
        }
    }
}

