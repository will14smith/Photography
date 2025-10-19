using Toxon.Photography.Data;

namespace Toxon.Photography.Generation.Models;

public class PhotographViewModel(Photograph photograph, Layout layout, string? thumbnailUrl)
{
    public Photograph Photograph { get; } = photograph;
    public Layout Layout { get; } = layout;
    public string? ThumbnailUrl { get; } = thumbnailUrl;
}