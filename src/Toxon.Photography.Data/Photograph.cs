using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Toxon.Photography.Data.Config;

namespace Toxon.Photography.Data;

public class Photograph
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string? Title { get; init; }
    public Layout? Layout { get; init; }

    public IReadOnlyCollection<Image> Images { get; set; } = [];

    public DateTime? CaptureTime { get; init; }
    public DateTime UploadTime { get; init; }
    
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = [];
}

public static class PhotographSerialization
{
    public static class Fields
    {
        public const string Id = "id";
        public const string Title = "title";
        public const string Layout = "layout";
        public const string Images = "images";
        public const string CaptureTime = "captureTime";
        public const string UploadTime = "uploadTime";
        public const string Metadata = "metadata";
    }

    public static Photograph FromDocument(Document document)
    {
        return new Photograph
        {
            Id = document[Fields.Id].AsGuid(),

            Title = document.TryGetValue(Fields.Title, out var title) ? title.AsString() : null,
            Layout = LayoutSerialization.FromDocument(document.TryGetNull(Fields.Layout)),

            Images = document[Fields.Images].AsListOfDocument().Select(ImageSerialization.FromDocument).ToList(),

            CaptureTime = document.TryGetValue(Fields.CaptureTime, out var captureTime) ? captureTime.AsDateTime() : null,
            UploadTime = document[Fields.UploadTime].AsDateTime(),
            
            Metadata = document.TryGetValue(Fields.Metadata, out var metadataEntry)
                ? metadataEntry.AsDocument().ToDictionary(kv => kv.Key, kv => kv.Value.AsString())
                : new Dictionary<string, string>(),
        };
    }

    public static Document ToDocument(Photograph photograph)
    {
        var document = new Document
        {
            [Fields.Id] = photograph.Id.ToString(),

            [Fields.Title] = photograph.Title,
            [Fields.Layout] = LayoutSerialization.ToDocument(photograph.Layout),

            [Fields.Images] = new DynamoDBList(photograph.Images.Select(ImageSerialization.ToDocument)),

            [Fields.UploadTime] = photograph.UploadTime,
            [Fields.Metadata] = new Document(photograph.Metadata.ToDictionary(kv => kv.Key, kv => (DynamoDBEntry)kv.Value)),
        };

        if (photograph.CaptureTime.HasValue)
        {
            document[Fields.CaptureTime] = photograph.CaptureTime.Value;
        }
        
        return document;
    }
}

public static class PhotographTable
{
    public static ITable Create(IAmazonDynamoDB client) => new TableBuilder(client, TableNames.Photograph)
        .AddHashKey("id", DynamoDBEntryType.String)
        .Build();
}