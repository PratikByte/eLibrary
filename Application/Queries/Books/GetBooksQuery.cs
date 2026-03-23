using eLibrary.Application.DTOs;
using MediatR;
using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Domain.Pagination;
using eLibrary.Application.Common.Helpers;

namespace eLibrary.Application.Queries.Books;


public class GetBooksQuery : IRequest<PagedResponse<BookDto>>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }

    public GetBooksQuery(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}


public class GetBooksHandler : IRequestHandler<GetBooksQuery, PagedResponse<BookDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<GetBooksHandler> _logger;

    public GetBooksHandler(IBookRepository bookRepository, ILogger<GetBooksHandler> logger)
    {
        _bookRepository = bookRepository;
        _logger = logger;
    }


    public async Task<PagedResponse<BookDto>> Handle(GetBooksQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (books, totalRecords) = await _bookRepository.GetAllAsync(
                request.PageNumber, request.PageSize, cancellationToken);

            if (books == null || books.Count == 0)
            {
                return new PagedResponse<BookDto>(
                    new List<BookDto>(),
                    0, request.PageNumber, request.PageSize,
                    "No books found.");
            
            }

            var bookDtos = BookHelper.MapBooksToDto(books);

            return new PagedResponse<BookDto>(
                bookDtos,
                totalRecords,
                request.PageNumber,
                request.PageSize,
                "Books retrieved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving books.");

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

