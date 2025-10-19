using System.ComponentModel.DataAnnotations;
using Toxon.Photography.Data;

namespace Toxon.Photography.Models;

public class PhotographCreateModel
{
    public string? Title { get; init; }
    public required string ImageKey { get; init; }
    public DateTime? CaptureTime { get; init; }

    internal Photograph ToPhotograph()
    {
        return new Photograph
        {
            Title = Title,
            Images = [new Image { Type = ImageType.Full, ObjectKey = ImageKey }],

            CaptureTime = CaptureTime,
            UploadTime = DateTime.UtcNow,
        };
    }

}