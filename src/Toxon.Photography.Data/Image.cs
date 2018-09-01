using Amazon.DynamoDBv2.DocumentModel;

namespace Toxon.Photography.Data
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

    public static class ImageSerialization
    {
        public static class Fields
        {
            public static readonly string Type = "type";
            public static readonly string ObjectKey = "objectKey";
        }

        public static Image FromDocument(Document document)
        {
            return new Image
            {
                Type = document[Fields.Type].AsEnum<ImageType>(),
                ObjectKey = document[Fields.ObjectKey].AsString(),
            };
        }

        public static Document ToDocument(Image image)
        {
            return new Document
            {
                [Fields.Type] = image.Type.ToString(),
                [Fields.ObjectKey] = image.ObjectKey,
            };
        }
    }

}
