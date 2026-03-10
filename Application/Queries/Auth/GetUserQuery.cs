using MediatR;
using EBook.Model;
using eLibrary.Application.DTOs;
using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Domain.Pagination;

namespace eLibrary.Application.Queries.Auth;

public class GetUserQuery : IRequest<PagedResponse<UserDto>>
{
    public GetUserQuery(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}

public class GetUserHandler : IRequestHandler<GetUserQuery, PagedResponse<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUserHandler> _logger;

    public GetUserHandler(IUserRepository userRepository, ILogger<GetUserHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }
    public async Task<PagedResponse<UserDto>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (user, totalRecords) = await _userRepository.GetUsersAsync(request.PageNumber, request.PageSize, cancellationToken);
            if (user == null || user.Count == 0)
            {
                return new PagedResponse<UserDto>(
                    new List<UserDto>(),
                    0, request.PageNumber, request.PageSize,
                    "No user found.");
            }
            var userDtos=UserHelper.MapUsersDto(user);
            return new PagedResponse<UserDto>(
                userDtos,
                totalRecords, request.PageNumber, request.PageSize,
                "Users fetched successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while handling GetUserQuery for Page {PageNumber} with size {PageSize}", request.PageNumber, request.PageSize);
            throw;
        }
    }
}

