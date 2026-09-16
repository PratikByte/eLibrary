using eLibrary.Identity.Application.Interfaces.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace eLibrary.Identity.Infrastructure.Services;

public sealed class MongoMediaStore : IMongoMediaStore
{
    private readonly IMongoCollection<MediaDocument> _media;

    public MongoMediaStore(IConfiguration configuration)
    {
        var client = new MongoClient(configuration["MongoDB:ConnectionString"]);
        var database = client.GetDatabase(configuration["MongoDB:Database"]);
        _media = database.GetCollection<MediaDocument>(configuration["MongoDB:Collection"] ?? "media");
    }

    public async Task<string> SaveAsync(IFormFile file, string category, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);
        var image = new MediaDocument
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            Category = category,
            Content = buffer.ToArray()
        };
        await _media.InsertOneAsync(image, cancellationToken: cancellationToken);
        return image.Id.ToString();
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken) =>
        ObjectId.TryParse(id, out var objectId)
            ? _media.DeleteOneAsync(x => x.Id == objectId, cancellationToken)
            : Task.CompletedTask;

    public async Task<(byte[] Content, string ContentType)?> GetAsync(string id, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(id, out var objectId)) return null;
        var image = await _media.Find(x => x.Id == objectId).FirstOrDefaultAsync(cancellationToken);
        return image is null ? null : (image.Content, image.ContentType);
    }
}

internal sealed class MediaDocument
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public string Category { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
}
