using System.ComponentModel.DataAnnotations;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Toxon.Photography.Data;

namespace Toxon.Photography.Models;

public class PhotographyEditModel
{
    public string? Title { get; init; }
    public DateTime? CaptureTime { get; init; }
    
    internal Document ToDocument() =>
        new()
        {
            [PhotographSerialization.Fields.Title] = Title is not null ? new Primitive(Title) : new DynamoDBNull(),
            [PhotographSerialization.Fields.CaptureTime] = CaptureTime is not null ? DynamoDBEntryConversion.V2.ConvertToEntry(CaptureTime.Value) : new DynamoDBNull(),
        };
}