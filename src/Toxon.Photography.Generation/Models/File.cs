using System;
using System.Threading.Tasks;

namespace Toxon.Photography.Generation.Models
{
    public abstract class File
    {
        protected File(string name, string contentType)
        {
            Name = name;
            ContentType = contentType;
        }

        public string Name { get; }
        public string ContentType { get; }

        public abstract Task<ReadOnlyMemory<byte>> GenerateAsync();
    }
}
