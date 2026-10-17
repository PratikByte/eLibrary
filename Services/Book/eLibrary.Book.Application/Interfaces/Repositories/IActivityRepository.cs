using eLibrary.Book.Application.DTOs;

namespace eLibrary.Book.Application.Interfaces.Repositories;

public interface IActivityRepository
{
    Task<List<ActivityEventDto>> GetRecentAsync(int count, CancellationToken cancellationToken);
    Task BackfillHistoryAsync(CancellationToken cancellationToken);
    Task RecordNewOverdueAsync(CancellationToken cancellationToken);
}
