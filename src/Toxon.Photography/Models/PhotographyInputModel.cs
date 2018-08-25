using System;
using Newtonsoft.Json;

namespace Toxon.Photography.Models
{
    public class PhotographyInputModel
    {
        public string Title { get; set; }

        public string ImageBase64 { get; set; }
        public string ImageContentType { get; set; }

        [JsonIgnore]
        public byte[] Image => Convert.FromBase64String(ImageBase64);
    }
}
