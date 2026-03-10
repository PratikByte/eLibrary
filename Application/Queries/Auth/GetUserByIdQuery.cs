using EBook.Model;
using eLibrary.Application.DTOs;
using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Shared;
using MediatR;


namespace eLibrary.Application.Queries.Auth;

public class GetUserByIdQuery : IRequest<ApiResponse<UserDto>>
{
    public GetUserByIdQuery(int id)
    {
        Id = id;
    }

    public int Id { get; set; }
}

public class GetUserByIDHandler : IRequestHandler<GetUserByIdQuery, ApiResponse<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUserByIDHandler> _logger;

    public GetUserByIDHandler(IUserRepository userRepository, ILogger<GetUserByIDHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user =
                await _userRepository.GetUserByIdAsync(request.Id, cancellationToken);
            if (user == null)
                return ApiResponse<UserDto>.Fail($"User not found with {request.Id}");
                var userDto=UserHelper.MapUserDto(user);
               return ApiResponse<UserDto>.Ok(userDto, "User fetched successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while handling GetUserById for ID {UserId}", request.Id);
            throw;
        }
    }
}

