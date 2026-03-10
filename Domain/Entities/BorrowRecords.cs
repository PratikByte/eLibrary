using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace eLibrary.Domain.Entities;

public class BorrowRecords
{
        [Key]
        public int Id { get; set; }

        // Who borrowed
        public int UserId { get; set; }
        public User User { get; set; }

        // What was borrowed
        public int BookId { get; set; }
        public Book? Book { get; set; }
    

        // When 
        public DateTime BorrowDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        // Status + fine
        public bool IsReturned { get; set; } = false;

        [Precision(10, 2)] // decimal(10,2) in SQL
        public decimal? Fine { get; set; }
    }

