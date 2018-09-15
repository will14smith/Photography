using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2.DocumentModel;

namespace Toxon.Photography.Data
{
    public class Photograph
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title { get; set; }
        public int? LayoutPosition { get; set; }

        public IReadOnlyCollection<Image> Images { get; set; }

        public DateTime CaptureTime { get; set; }
        public DateTime UploadTime { get; set; }
    }

    public static class PhotographSerialization
    {
        public static class Fields
        {
            public static readonly string Id = "id";
            public static readonly string Title = "title";
            public static readonly string LayoutPosition = "layout";
            public static readonly string Images = "images";
            public static readonly string CaptureTime = "captureTime";
            public static readonly string UploadTime = "uploadTime";
        }

        public static Photograph FromDocument(Document document)
        {
            return new Photograph
            {
                Id = document[Fields.Id].AsGuid(),

                Title = document[Fields.Title].AsString(),
                LayoutPosition = document[Fields.LayoutPosition].AsIntNullable(),

                Images = document[Fields.Images].AsListOfDocument().Select(ImageSerialization.FromDocument).ToList(),

                CaptureTime = document[Fields.CaptureTime].AsDateTime(),
                UploadTime = document[Fields.UploadTime].AsDateTime(),
            };
        }

        public static Document ToDocument(Photograph photograph)
        {
            return new Document
            {
                [Fields.Id] = photograph.Id.ToString(),

                [Fields.Title] = photograph.Title,
                [Fields.LayoutPosition] = photograph.LayoutPosition,

                [Fields.Images] = new DynamoDBList(photograph.Images.Select(ImageSerialization.ToDocument)),

                [Fields.CaptureTime] = photograph.CaptureTime,
                [Fields.UploadTime] = photograph.UploadTime,
            };
        }
    }
}
