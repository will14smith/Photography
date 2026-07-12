namespace Toxon.Photography.Models
{
    public class StoryAnalysis
    {
        public required DateRange DateRange { get; set; }
        public required StoryMetadata Metadata { get; set; }
        public required List<StorySectionAnalysis> Sections { get; set; }
    }
    
    public class DateRange
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
    }
    
    public class StoryMetadata
    {
        public required List<string> PrimaryLocations { get; set; }
        public required string OverallTheme { get; set; }
    }
    
    public class StorySectionAnalysis
    {
        public required string Title { get; set; }
        public required DateRange DateRange { get; set; }
        public required string Summary { get; set; }
        public required string Theme { get; set; }
    }
}

