using System;
using System.Collections.Generic;

namespace Toxon.Photography.Models
{
    public class Photograph
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title { get; set; }

        public IReadOnlyCollection<Image> Images { get; set; }
    }
}
