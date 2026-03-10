namespace eLibrary.Application.Interfaces.Services;

public interface IFileStorage
{
    Task<string> SaveAsync(string scope, string safeFileName, Stream content, CancellationToken ct);
    Task DeleteAsync(string relativePath, CancellationToken ct);
}

