using EBook.CQRS.Command;
using MediatR;
using eLibrary.Application.DTOs;
using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Infrastructure.Services;
using eLibrary.Shared;

namespace EBook.CQRS.Command
{
    public record ReturnBookCommand(int BorrowId, int? CurrentUserId = null, bool IsAdmin = false) : IRequest<ApiResponse<BorrowRecordDto>>;
}
public class ReturnBookCommandHandler : IRequestHandler<ReturnBookCommand, ApiResponse<BorrowRecordDto>>
{
    private readonly IBorrowRepository _borrowRepository;
    private readonly ILogger<ReturnBookCommandHandler> _logger;
    private readonly IBookRepository _bookRepository;
    private readonly INotificationService _notificationService;
    private readonly EmailService _emailService;
    private readonly IUserRepository _userRepository;

    public ReturnBookCommandHandler(IBorrowRepository borrowRepository, ILogger<ReturnBookCommandHandler> logger, IBookRepository bookRepository, INotificationService notificationService, EmailService emailService, IUserRepository userRepository)
    {
        _borrowRepository = borrowRepository;
        _logger = logger;
        _bookRepository = bookRepository;
        _notificationService = notificationService;
        _emailService = emailService;
        _userRepository = userRepository;
    }

    public async Task<ApiResponse<BorrowRecordDto>> Handle(ReturnBookCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var borrowRecord = await _borrowRepository.GetByIdAsync(request.BorrowId);
            if (borrowRecord == null)
            {
                return ApiResponse<BorrowRecordDto>.Fail($"Borrow record with ID {request.BorrowId} not found.");
            }

            if (!request.IsAdmin && (!request.CurrentUserId.HasValue || request.CurrentUserId.Value != borrowRecord.UserId))
            {
                return ApiResponse<BorrowRecordDto>.Fail("You are not allowed to return this book.");
            }

            if (borrowRecord.IsReturned)
            {
                return ApiResponse<BorrowRecordDto>.Fail(
                           "This book has already been returned.");
            }
            //mark as return
            borrowRecord.IsReturned = true;
            borrowRecord.ReturnDate = DateTime.Now;

            // ✅ Increment the book's available count
            var incrementResult = await _bookRepository.IncreaseAvailBookCountAsync(borrowRecord.BookId, cancellationToken);

            if (!incrementResult)
            {
                _logger.LogWarning("Book count update failed for BookId {BookId}", borrowRecord.BookId);
                return ApiResponse<BorrowRecordDto>.Fail("Book return failed because count could not be updated.");
            }


            //check if overdue
            if (borrowRecord.ReturnDate > borrowRecord.DueDate)
            {
                var overDueDays = (borrowRecord.ReturnDate.Value - borrowRecord.DueDate).Days;
                borrowRecord.Fine = overDueDays * 10; // e.g., ₹10 per day
            }

            //save changes
            await _borrowRepository.UpdateAsync(borrowRecord);
            await _borrowRepository.SaveChangesAsync();

            //map to dto
            var dto = new BorrowRecordDto
            {
                Id = borrowRecord.Id,
                UserId = borrowRecord.UserId,
                BookId = borrowRecord.BookId,
                BorrowDate = borrowRecord.BorrowDate.ToString("dd-MMM-yyyy"),
                DueDate = borrowRecord.DueDate.ToString("dd-MMM-yyyy"),
                ReturnDate = borrowRecord.ReturnDate?.ToString("dd-MMM-yyyy"),
                IsReturned = borrowRecord.IsReturned,
                Fine = borrowRecord.Fine
            };

            string message = (dto.Fine.HasValue && dto.Fine > 0)
                                     ? $"Book returned with fine ₹{dto.Fine}"
                                                    : "Book returned successfully.";

            //send email notification
            var user = await _userRepository.GetUserByIdAsync(borrowRecord.UserId, cancellationToken);
            var bookResponse = await _bookRepository.GetBookByIdAsync(borrowRecord.BookId,cancellationToken);

            var title= bookResponse?.Title ?? "the book you borrowed";
            var UserName=user?.Username ?? "User";
            var UserEmail = user?.Email??"FailedReturnNotification@gmail.com";//if not fouond sent to fail email

            var (subject, body) = await _notificationService
               .ReturnBookConfirmationEmail(dto, UserName, title);

            await _emailService.SendEmailAsync(UserEmail, subject, body);

            return ApiResponse<BorrowRecordDto>.Ok(dto, message);
        }
        catch (Exception ex)
        {           
            _logger.LogError(ex, "Error returning book with BorrowId {BorrowId}", request.BorrowId);
            return ApiResponse<BorrowRecordDto>.Fail("An error occurred while returning the book.");
        }
    }
}
