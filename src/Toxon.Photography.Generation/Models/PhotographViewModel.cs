using Toxon.Photography.Data;

namespace Toxon.Photography.Generation.Models;

public class PhotographViewModel(Photograph photograph, string? thumbnailUrl)
{
    public Photograph Photograph { get; } = photograph;
    public string? ThumbnailUrl { get; } = thumbnailUrl;
}