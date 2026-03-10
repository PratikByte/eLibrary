using eLibrary.Application.DTOs;

namespace eLibrary.Infrastructure.Services;

public interface INotificationService
{
    Task<(string Subject, string Body)> GenerateBorrowConfirmationEmail(
        BorrowRecordDto borrowRecord, string username, string bookTitle);
    Task<(string Subject, string Body)> ReturnBookConfirmationEmail(
        BorrowRecordDto borrowRecord, string username, string bookTitle);
}
public class NotificationService : INotificationService
{

    public Task<(string Subject, string Body)> GenerateBorrowConfirmationEmail(BorrowRecordDto borrowRecord, string username, string bookTitle)
    {
        string subject = $"📚 Borrow Confirmation – Your Book Borrow Record #{borrowRecord.Id}";

        string body = $@" <p>Hello <b>{username}</b>,</p>
        
                <p>Thank you for borrowing a book from our library! Here are the details of your borrow record:</p>
        
        <ul>
            
            <li><b>Book:</b> {bookTitle}</li>
            <li><b>Borrow Date:</b> {borrowRecord.BorrowDate:dd-MMM-yyyy}</li>
            <li><b>Due Date:</b> {borrowRecord.DueDate:dd-MMM-yyyy}</li>
        </ul>
        
        <p>Please make sure to return the book on or before <b>{borrowRecord.DueDate:dd-MMM-yyyy}</b> to avoid any late fines.</p>
        <p>If you have already returned the book, kindly ignore this message.</p>
        
         <p>Happy Reading! 📖<br/>— The E-Library Team</p>
                ";
        return Task.FromResult((subject, body));
    }

    public Task<(string Subject, string Body)> ReturnBookConfirmationEmail(BorrowRecordDto borrowRecord, string username, string bookTitle)
    {
        string subject = $"📚 Return Book Confirmation – Your Book Return Record #{borrowRecord.Id}";

        string body = $@"

                  <p>Hello <b>{username}</b>,</p>
    
        <p>Thank you for returning your book to the library! Here are the details of your return:</p>
    
    <ul>
        <li><b>Book:</b> {bookTitle}</li>
        <li><b>Borrow Date:</b> {borrowRecord.BorrowDate:dd-MMM-yyyy}</li>
        <li><b>Due Date:</b> {borrowRecord.DueDate:dd-MMM-yyyy}</li>
        <li><b>Return Date:</b> {borrowRecord.ReturnDate:dd-MMM-yyyy}</li>
        <li><b>Fine:</b> ₹{borrowRecord.Fine ?? 0}</li>
    </ul>
    
    {(borrowRecord.Fine > 0
     ? $"<p style='color:red;'><b>Action Required:</b> Please pay the pending fine of ₹{borrowRecord.Fine} at the earliest to avoid further penalties.</p>"
     : "<p>No fines are due. Thank you for returning on time! ✅</p>")}
    
    <p>We appreciate your continued reading! 📖<br/>— The E-Library Team</p>
    ";

        return Task.FromResult((subject, body));

    }
}

