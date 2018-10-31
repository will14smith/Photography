using System;
using Amazon.DynamoDBv2.DocumentModel;

namespace Toxon.Photography.Data
{
    public class Layout
    {
        public int Order { get; set; }

        public int? Width { get; set; }
        public int? Height { get; set; }
    }


    public static class LayoutSerialization
    {
        public static class Fields
        {
            public static readonly string Order = "Order";
            public static readonly string Width = "Width";
            public static readonly string Height = "Height";
        }

        public static Layout FromDocument(DynamoDBEntry entry)
        {
            if (entry is DynamoDBNull)
            {
                return null;
            }

            if (entry is Primitive primitive && primitive.Type == DynamoDBEntryType.Numeric)
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

        public static Document ToDocument(Layout layout)
        {
            return new Document
            {
                [Fields.Order] = layout.Order,

                [Fields.Width] = layout.Width,
                [Fields.Height] = layout.Height,
            };
        }
    }
}
