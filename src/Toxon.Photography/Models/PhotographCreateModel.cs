using System.ComponentModel.DataAnnotations;
using Toxon.Photography.Data;

namespace Toxon.Photography.Models;

public class PhotographCreateModel
{
    public string? Title { get; init; }
    public required string ImageKey { get; init; }
    public DateTime? CaptureTime { get; init; }
}