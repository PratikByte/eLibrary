using Microsoft.AspNetCore.Http;

namespace eLibrary.Book.Application.Interfaces;

public interface IMongoMediaStore
{
    Task<string> SaveAsync(IFormFile file, string category, CancellationToken cancellationToken);
    Task DeleteAsync(string id, CancellationToken cancellationToken);
    Task<(byte[] Content, string ContentType)?> GetAsync(string id, CancellationToken cancellationToken);
}
