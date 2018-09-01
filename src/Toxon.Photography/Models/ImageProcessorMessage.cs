using System;
using Toxon.Photography.Data;

namespace Toxon.Photography.Models
{
    public class ImageProcessorMessage
    {
        public Guid PhotographId { get; set; }
        public Image Image { get; set; } 
    }
}
