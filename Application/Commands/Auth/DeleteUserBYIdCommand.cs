using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Shared;
using MediatR;

namespace eLibrary.Application.Commands.Auth;

public class DeleteUserBYIdCommand: IRequest<ApiResponse<bool>>
{

    public int Id { get; set; }
    public DeleteUserBYIdCommand(int id)
    {
        Id = id;
    }
}
public class DeleteUserByIDHandler : IRequestHandler<DeleteUserBYIdCommand, ApiResponse<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DeleteUserBYIdCommand> _logger;

    public DeleteUserByIDHandler(IUserRepository userRepository, ILogger<DeleteUserBYIdCommand> logger)
    {
       _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteUserBYIdCommand request, CancellationToken cancellationToken)
    {
        var deleted = await _userRepository.DeleteByIdAsync(request.Id, cancellationToken);
        if (!deleted)
        {
            return ApiResponse<bool>.Fail($"User not found with {request.Id}");
        }
        return ApiResponse<bool>.Ok(true, "User deleted successfully");
    }
}

