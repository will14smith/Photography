namespace Toxon.Photography.Models;

public class StoryCreateModel
{
    public string StoryTitle { get; set; } = string.Empty;
    public string Journal { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

