namespace eLibrary.Book.Application.Common.Helpers;

public static class BorrowFineCalculator
{
    public const decimal FinePerOverdueDay = 10m;

    public static int GetDaysOverdue(DateTime dueDate, DateTime asOfDate)
    {
        var days = DateOnly.FromDateTime(asOfDate).DayNumber -
                   DateOnly.FromDateTime(dueDate).DayNumber;
        return Math.Max(days, 0);
    }

    public static decimal Calculate(DateTime dueDate, DateTime asOfDate) =>
        GetDaysOverdue(dueDate, asOfDate) * FinePerOverdueDay;
}
