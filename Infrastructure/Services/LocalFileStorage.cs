using eLibrary.Application.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace eLibrary.Infrastructure.Services;

public class FileStorageOptions
{
    // Root on disk where files are stored (served via StaticFiles)
    public string RootPath { get; set; } = "wwwroot/uploads";
}
public class LocalFileStorage: IFileStorage
{
    private readonly string _root;

    public LocalFileStorage(IOptions<FileStorageOptions> options)
    {
        _root = options.Value.RootPath;
        Directory.CreateDirectory(_root);
    }

    public Task DeleteAsync(string relativePath, CancellationToken ct)
    {
         var fullPath = Path.Combine(_root, relativePath);
    if (File.Exists(fullPath))
    File.Delete(fullPath);

    return Task.CompletedTask;
    }

    public async Task<string> SaveAsync(string scope, string safeFileName, Stream content, CancellationToken cancellationToken)
    {
        // scope groups files per book, e.g. "books/123"
        var folder = Path.Combine(_root, scope);
        Directory.CreateDirectory(folder);

        var baseName = Path.GetFileNameWithoutExtension(safeFileName);
        var ext = Path.GetExtension(safeFileName);
        var unique = $"{baseName}_{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(folder, unique);

        using var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fs, cancellationToken);

        // return path relative to _root, so you can map to "/uploads/<rel>"
        var rel = Path.GetRelativePath(_root, fullPath).Replace('\\', '/');
        return rel;
    }
}

