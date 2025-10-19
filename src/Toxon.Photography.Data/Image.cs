using Amazon.DynamoDBv2.DocumentModel;

namespace Toxon.Photography.Data
{
    public class Image
    {
        public required ImageType Type { get; init; }
        public required string ObjectKey { get; init; }
        
        public required int Width { get; init; }
        public required int Height { get; init; }
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
            public const string Type = "type";
            public const string ObjectKey = "objectKey";
            public const string Width = "width";
            public const string Height = "height";
        }

        public static Image FromDocument(Document document) =>
            new()
            {
                Type = document[Fields.Type].AsEnum<ImageType>(),
                ObjectKey = document[Fields.ObjectKey].AsString(),
                Width = document.TryGetNull(Fields.Width).AsIntNullable() ?? 0,
                Height = document.TryGetNull(Fields.Height).AsIntNullable() ?? 0,
            };

        public static Document ToDocument(Image image) =>
            new()
            {
                [Fields.Type] = image.Type.ToString(),
                [Fields.ObjectKey] = image.ObjectKey,
                [Fields.Width] = image.Width,
                [Fields.Height] = image.Height,
            };
    }

}
