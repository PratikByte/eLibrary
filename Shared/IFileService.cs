namespace eLibrary.Shared;

public interface IFileService

{   /// <summary>
    /// Save file inside wwwroot/{folder} and return relative path.
    /// Example return: "profile-pictures/abc123.png"
    /// </summary>
    Task<string> SaveFileAsync(IFormFile file);

    /// <summary>
    /// Delete file using its relative path from wwwroot.
    /// Example input: "profile-pictures/abc123.png"
    /// </summary>
    Task DeleteFileAsync(string filePath);
}

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _env;


    public FileService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveFileAsync(IFormFile file)
    {
        var profilePicturesFolder = Path.Combine(_env.WebRootPath, "profile-pictures");

        if (!Directory.Exists(profilePicturesFolder))
            Directory.CreateDirectory(profilePicturesFolder);

        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(profilePicturesFolder, uniqueFileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        return Path.Combine("profile-pictures", uniqueFileName).Replace("\\", "/");
    }

    public Task DeleteFileAsync(string  filePath)
    {
        var fullPath = Path.Combine(_env.WebRootPath, filePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }
}

