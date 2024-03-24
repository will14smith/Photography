using System.ComponentModel.DataAnnotations;
using Amazon.DynamoDBv2.DocumentModel;
using Toxon.Photography.Data;

namespace Toxon.Photography.Models;

public class PhotographyEditModel
{
    [Required]
    public string Title { get; set; }
    [Required]
    public DateTime CaptureTime { get; set; }
    
    internal Document ToDocument()
    {
        return new Document
        {
            [PhotographSerialization.Fields.Title] = Title,
            [PhotographSerialization.Fields.CaptureTime] = CaptureTime,
        };
    }

}