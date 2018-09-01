using System;

namespace Toxon.Photography.Models
{
    public class PhotographyInputModel
    {
        public string Title { get; set; }
        
        public string ImageKey { get; set; }

        public DateTime CaptureTime { get; }
    }
}
