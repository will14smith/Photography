using System;
using Amazon.DynamoDBv2.DocumentModel;

namespace Toxon.Photography.Data
{
    public class Layout
    {
        public required int Order { get; init; }

        public int? Width { get; init; }
        public int? Height { get; init; }
    }
    
    public static class LayoutSerialization
    {
        public static class Fields
        {
            public const string Order = "Order";
            public const string Width = "Width";
            public const string Height = "Height";
        }

        public static Layout? FromDocument(DynamoDBEntry entry)
        {
            if (entry is DynamoDBNull)
            {
                return null;
            }

            if (entry is Primitive { Type: DynamoDBEntryType.Numeric } primitive)
            {
                var order = primitive.AsInt();
                return new Layout { Order = order };
            }

            if (entry is Document document)
            {
                return new Layout
                {
                    Order = document[Fields.Order].AsInt(),

                    Width = document.TryGetNull(Fields.Width).AsIntNullable(),
                    Height = document.TryGetNull(Fields.Height).AsIntNullable(),
                };
            }

            throw new Exception("Invalid format for Layout");
        }

        public static Document? ToDocument(Layout? layout)
        {
            if (layout == null)
            {
                return null;
            }

            return new Document
            {
                [Fields.Order] = layout.Order,

                [Fields.Width] = layout.Width,
                [Fields.Height] = layout.Height,
            };
        }
    }
}
