using eLibrary.Book.Application.DTOs;
using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Application.Shared;
using MediatR;

namespace eLibrary.Book.Application.Queries.Reports;

public record GetRecentActivityQuery(int Count = 10)
    : IRequest<ApiResponse<List<ActivityEventDto>>>;

public class GetRecentActivityQueryHandler
    : IRequestHandler<GetRecentActivityQuery, ApiResponse<List<ActivityEventDto>>>
{
    private readonly IActivityRepository _activityRepository;

    public GetRecentActivityQueryHandler(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task<ApiResponse<List<ActivityEventDto>>> Handle(
        GetRecentActivityQuery request,
        CancellationToken cancellationToken)
    {
        var events = await _activityRepository.GetRecentAsync(request.Count, cancellationToken);
        return ApiResponse<List<ActivityEventDto>>.Ok(events, "Recent activity retrieved successfully.");
    }
}
