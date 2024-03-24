namespace Toxon.Photography.Generation;

public class Site(IReadOnlyCollection<Site.File> files)
{
    public IReadOnlyCollection<File> Files { get; } = files;

    public class File(string name, string contentType, ReadOnlyMemory<byte> content)
    {
        public string Name { get; } = name;
        public string ContentType { get; } = contentType;
        public ReadOnlyMemory<byte> Content { get; } = content;
    }
}