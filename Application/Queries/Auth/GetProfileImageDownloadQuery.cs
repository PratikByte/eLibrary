using eLibrary.Application.DTOs;
using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Shared;
using MediatR;
using Microsoft.AspNetCore.StaticFiles;

namespace eLibrary.Application.Queries.Auth;

public record GetProfileImageDownloadQuery(int UserId) : IRequest<ApiResponse<FileDownloadDto>>;

public class GetProfileImageDownloadQueryHandler : IRequestHandler<GetProfileImageDownloadQuery, ApiResponse<FileDownloadDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<GetProfileImageDownloadQueryHandler> _logger;

    public GetProfileImageDownloadQueryHandler(
        IUserRepository userRepository,
        IWebHostEnvironment environment,
        ILogger<GetProfileImageDownloadQueryHandler> logger)
    {
        _userRepository = userRepository;
        _environment = environment;
        _logger = logger;
    }

    public async Task<ApiResponse<FileDownloadDto>> Handle(GetProfileImageDownloadQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Step 1: load the user so we can read the stored profile image path.
            var user = await _userRepository.GetUserByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return ApiResponse<FileDownloadDto>.Fail("User not found");
            }

            // Step 2: ensure a profile picture exists for this user.
            if (string.IsNullOrWhiteSpace(user.ProfileImagePath))
            {
                return ApiResponse<FileDownloadDto>.Fail("Profile image not found.");
            }

            // Step 3: map the relative path from DB to the physical file under wwwroot.
            var relativePath = user.ProfileImagePath.Replace("/", Path.DirectorySeparatorChar.ToString());
            var fullPath = Path.Combine(_environment.WebRootPath, relativePath);
            if (!File.Exists(fullPath))
            {
                return ApiResponse<FileDownloadDto>.Fail("Profile image file not found on server.");
            }

            // Step 4: return the file metadata the controller needs to force a download.
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fullPath, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var download = new FileDownloadDto
            {
                FilePath = fullPath,
                FileName = Path.GetFileName(fullPath),
                ContentType = contentType
            };

            return ApiResponse<FileDownloadDto>.Ok(download);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while preparing profile download for UserId {UserId}", request.UserId);
            return ApiResponse<FileDownloadDto>.Fail("An error occurred while preparing the profile image download.");
        }
    }
}
