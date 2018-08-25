namespace Toxon.Photography.Models
{
    public class Image
    {
        public ImageType Type { get; set; }
        public string ObjectKey { get; set; }
    }

    public enum ImageType
    {
        Full,
        Thumbnail,
    }
}
