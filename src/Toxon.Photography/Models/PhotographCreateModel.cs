using System.ComponentModel.DataAnnotations;
using Toxon.Photography.Data;

namespace Toxon.Photography.Models;

public class PhotographCreateModel
{
    [Required]
    public string Title { get; set; }
    [Required]
    public string ImageKey { get; set; }
    [Required]
    public DateTime CaptureTime { get; set; }

    internal Photograph ToPhotograph()
    {
        return new Photograph
        {
            Title = Title,
            Images = new[] { new Image { Type = ImageType.Full, ObjectKey = ImageKey } },

            CaptureTime = CaptureTime,
            UploadTime = DateTime.UtcNow,
        };
    }

}