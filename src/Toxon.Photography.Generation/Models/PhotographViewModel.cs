using Toxon.Photography.Data;

namespace Toxon.Photography.Generation.Models
{
    public class PhotographViewModel
    {
        public PhotographViewModel(Photograph photograph, string thumbnailUrl)
        {
            Photograph = photograph;
            ThumbnailUrl = thumbnailUrl;
        }

        public Photograph Photograph { get; }
        public string ThumbnailUrl { get; }
    }
}