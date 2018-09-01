using System.Collections.Generic;

namespace Toxon.Photography.Generation.Models
{
    public class Site
    {
        public Site(IReadOnlyCollection<File> files)
        {
            Files = files;
        }

        public IReadOnlyCollection<File> Files { get; }
    }
}
