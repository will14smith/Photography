
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Toxon.Photography.Generation.Views;

namespace Toxon.Photography.Generation;

public sealed class SiteGenerator : IAsyncDisposable
{

    private static readonly IReadOnlyDictionary<string, string> ContentTypes = new Dictionary<string, string> {
        {".css", "text/css"}
    };
    
    private readonly ServiceProvider _serviceProvider;
    private readonly ILoggerFactory _loggerFactory;
    private readonly HtmlRenderer _htmlRenderer;
    private readonly IFileProvider _assetsProvider;
    private readonly DynamoDbImageProvider _imageProvider;

    public SiteGenerator(DynamoDbImageProvider imageProvider)
    {
        _imageProvider = imageProvider;
        var services = new ServiceCollection();
        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();
        _loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();

        _htmlRenderer = new HtmlRenderer(_serviceProvider, _loggerFactory);
        _assetsProvider = new PhysicalFileProvider(Path.Combine(Environment.CurrentDirectory, "Assets"));
    }

    public async Task<Site> GenerateAsync()
    {
        var files = new List<Site.File>
        {
            await RenderPage<IndexPage>("index.html", new Dictionary<string, object?> { { nameof(IndexPage.Photographs), await _imageProvider.GetPrimaryPhotographsAsync() } }),
            await RenderPage<AboutPage>("about.html", new Dictionary<string, object?>()),
            await RenderPage<GearPage>("gear.html", new Dictionary<string, object?>()),
        };

        await foreach (var asset in GenerateAssets())
        {
            files.Add(asset);
        }
        
        return new Site(files);
    }

    private async IAsyncEnumerable<Site.File> GenerateAssets()
    {
        foreach (var asset in _assetsProvider.GetDirectoryContents("/"))
        {
            if (asset.IsDirectory)
            {
                throw new NotImplementedException("TODO implement recursive asset inclusion");
            }

            ReadOnlyMemory<byte> content;
            await using (var stream = asset.CreateReadStream())
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);

                content = ms.ToArray();
            }

            yield return new Site.File("assets/" + asset.Name, ContentTypes[Path.GetExtension(asset.Name)], content);
        }
    }

    private async Task<Site.File> RenderPage<TPage>(string name, IDictionary<string, object?> parameters) where TPage : IComponent
    {
        var parametersView = ParameterView.FromDictionary(parameters);

        var content = await _htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await _htmlRenderer.RenderComponentAsync<TPage>(parametersView);
            return output.ToHtmlString();
        });

        return new Site.File(name, "text/html", Encoding.UTF8.GetBytes(content));
    }

    public async ValueTask DisposeAsync()
    {
        await _htmlRenderer.DisposeAsync();
        _loggerFactory.Dispose();
        await _serviceProvider.DisposeAsync();
    }
}