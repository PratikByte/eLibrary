using Microsoft.AspNetCore.Http;

namespace eLibrary.Identity.Application.Interfaces.Services;

public interface IMongoMediaStore
{
    Task<string> SaveAsync(IFormFile file, string category, CancellationToken cancellationToken);
    Task DeleteAsync(string id, CancellationToken cancellationToken);
    Task<(byte[] Content, string ContentType)?> GetAsync(string id, CancellationToken cancellationToken);
}
