using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using BaseFile = Toxon.Photography.Generation.Models.File;

namespace Toxon.Photography.Generation
{
    internal class StaticFile : BaseFile
    {
        private readonly IFileInfo _file;

        private static readonly IReadOnlyDictionary<string, string> ContentTypes = new Dictionary<string, string> {
            {".css", "text/css"}
        };

        public StaticFile(IFileInfo file, string outputPath) : base(outputPath, ContentTypes[Path.GetExtension(file.Name)])
        {
            _file = file;
        }

        public override async Task<ReadOnlyMemory<byte>> GenerateAsync()
        {
            using (var stream = _file.CreateReadStream())
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);

                return ms.ToArray();
            }
        }
    }
}