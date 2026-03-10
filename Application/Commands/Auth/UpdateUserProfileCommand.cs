using MediatR;
using eLibrary.Application.DTOs;
using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Shared;

namespace eLibrary.Application.Commands.Auth;

// Command
public class UpdateUserProfileCommand : IRequest<ApiResponse<UserProfileResponseDto>>
{
    public int UserId { get; }
    public UserProfileDto UserDto { get; }

    public UpdateUserProfileCommand(int userId, UserProfileDto userDto)
    {
        UserId = userId;
        UserDto = userDto;
    }
}

// Handler
public class UpdateUserProfileHandler : IRequestHandler<UpdateUserProfileCommand, ApiResponse<UserProfileResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IFileService _fileService;
    private readonly ILogger<UpdateUserProfileHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdateUserProfileHandler(
        IUserRepository userRepository,
        IFileService fileService,
        ILogger<UpdateUserProfileHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _fileService = fileService;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse<UserProfileResponseDto>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var dto = request.UserDto;

            // 1. Find existing user (trust request.UserId, not dto.UserId)
            var user = await _userRepository.GetUserByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User not found for UserId {UserId}", request.UserId);
                return ApiResponse<UserProfileResponseDto>.Fail("User not found");
            }

            // 2. Check for duplicates (email & username)
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                bool isEmailDuplicate = await _userRepository.IsEmailDuplicate(dto.Email, request.UserId);
                if (isEmailDuplicate)
                {
                    _logger.LogWarning("Email {Email} already in use for UserId {UserId}", dto.Email, request.UserId);
                    return ApiResponse<UserProfileResponseDto>.Fail("Email is already in use by another user.");
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Username))
            {
                bool isUsernameDuplicate = await _userRepository.IsUsernameDuplicate(dto.Username, request.UserId);
                if (isUsernameDuplicate)
                {
                    _logger.LogWarning("Username {Username} already in use for UserId {UserId}", dto.Username, request.UserId);
                    return ApiResponse<UserProfileResponseDto>.Fail("Username is already in use by another user.");
                }
            }

            // 3. Handle profile image update (only if new image provided)
            if (dto.ProfileImage != null)
            {
                if (!string.IsNullOrEmpty(user.ProfileImagePath))
                {
                    await _fileService.DeleteFileAsync(user.ProfileImagePath);
                }

                user.ProfileImagePath = await _fileService.SaveFileAsync(dto.ProfileImage);
            }

            // 4. Update profile fields
            //if (dto.Email is not null && !string.IsNullOrWhiteSpace(dto.Email))
            //    user.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                user.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.Username))
                user.Username = dto.Username;

            await _userRepository.SaveChangesAsync();

            // 5. Build full profile image URL
            string? profileImageUrlFull = null;
            if (!string.IsNullOrEmpty(user.ProfileImagePath))
            {
                var req = _httpContextAccessor.HttpContext?.Request;
                var baseUrl = $"{req?.Scheme}://{req?.Host}";
                profileImageUrlFull = $"{baseUrl}/{user.ProfileImagePath.Replace("\\", "/")}";
            }

            // 6. Prepare response DTO
            var responseDto = new UserProfileResponseDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                ProfileImageUrl = profileImageUrlFull
            };

            _logger.LogInformation("User profile updated successfully for UserId {UserId}", user.UserId);

            return ApiResponse<UserProfileResponseDto>.Ok(responseDto, "User profile updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating profile for UserId {UserId}", request.UserId);
            return ApiResponse<UserProfileResponseDto>.Fail("Update failed", ex.Message);
        }
    }
}

