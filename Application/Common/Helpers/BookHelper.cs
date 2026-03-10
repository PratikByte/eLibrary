using eLibrary.Domain.Entities;
using eLibrary.Application.DTOs;

namespace BookStoreAPI_updated.Model;

    //list of books to dto
    public class BookHelper
    {
    private static IHttpContextAccessor? _httpContextAccessor;

    public static void Configure(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private static string GetBaseUrl()
    {
        var request = _httpContextAccessor?.HttpContext?.Request;
        return request != null ? $"{request.Scheme}://{request.Host}" : "";
    }
    public static List<BookDto> MapBooksToDto(List<Book> books)
        {
            return books.Select(b => MapBookToDto(b)).ToList();

        }

        // ✅ For Single Book to DTO 
        public static BookDto MapBookToDto(Book book)
        {
            if (book == null) return null!; // handle null case safely
    
            var baseUrl = GetBaseUrl();

        return new BookDto
            {   Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                //Price = book.Price,
                AvailableCount = book.AvailableCount,
                CoverImageUrl = string.IsNullOrEmpty(book.CoverImagePath)
            ? null
            : $"{baseUrl}/{book.CoverImagePath.Replace("\\", "/")}"
            };
        }
        public static List<BookWithPriceDto> MapPriceBooksToDto(List<Book> books)
        {
            return books.Select(b => MapPriceBookToDto(b)).ToList();

        }

        // ✅ For Single Book to DTO 
        public static BookWithPriceDto MapPriceBookToDto(Book book)
        {
            if (book == null) return null!; // handle null case safely

            return new BookWithPriceDto
            {   Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Price = book.Price,
                AvailableCount = book.AvailableCount
            };
        }

    }

