namespace eLibrary.Shared;

public interface IFileService

{   /// <summary>
    /// Save file inside wwwroot/{folder} and return relative path.
    /// Example return: "users/abc123.png"
    /// </summary>
    Task<string> SaveFileAsync(IFormFile file);

    /// <summary>
    /// Delete file using its relative path from wwwroot.
    /// Example input: "users/abc123.png"
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
        var uploadsFolder = Path.Combine(_env.WebRootPath);

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        return Path.Combine("uploads", uniqueFileName).Replace("\\", "/");
    }

    public Task DeleteFileAsync(string  filePath)
        {
        if(File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        return Task.CompletedTask;
    }
}

