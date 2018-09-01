using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using RazorLight;
using Toxon.Photography.Generation.Models;
using File = Toxon.Photography.Generation.Models.File;

namespace Toxon.Photography.Generation
{
    public class SiteGenerator
    {
        private readonly IImageProvider _imageProvider;
        private readonly IRazorLightEngine _razor;
        private readonly IFileProvider _assetsProvider;

        public SiteGenerator(IImageProvider imageProvider)
        {
            _imageProvider = imageProvider;

            _razor = new RazorLightEngineBuilder()
                .UseFilesystemProject(Path.Combine(Environment.CurrentDirectory, "Views"))
                .UseMemoryCachingProvider()
                .Build();

            _assetsProvider = new PhysicalFileProvider(Path.Combine(Environment.CurrentDirectory, "Assets"));
        }

        public async Task<Site> GenerateAsync()
        {
            var files = new List<File>
            {
                new RazorFile(_razor, "Index.cshtml", await _imageProvider.GetPrimaryPhotographsAsync(), "index.html"),
                new RazorFile(_razor, "About.cshtml", null, "about.html"),
                new RazorFile(_razor, "Gear.cshtml", null, "gear.html"),
            };

            files.AddRange(GenerateAssets());

            return new Site(files);
        }

        private IEnumerable<File> GenerateAssets()
        {
            foreach (var asset in _assetsProvider.GetDirectoryContents("/"))
            {
                if (asset.IsDirectory)
                {
                    throw new NotImplementedException("TODO implement recursive asset inclusion");
                }

                yield return new StaticFile(asset, "assets/" + asset.Name);
            }
        }
    }
}
