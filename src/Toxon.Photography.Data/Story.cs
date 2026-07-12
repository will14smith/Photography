using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Toxon.Photography.Data.Config;
using System.Text.Json.Serialization;

namespace Toxon.Photography.Data;

public class Story
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;
    
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public string Journal { get; set; } = string.Empty;

    public IReadOnlyList<Section> Sections { get; set; } = [];

    public DateTime CreatedAt { get; set; }
}

public class Section
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public IReadOnlyList<Block> Blocks { get; set; } = [];
}

[JsonDerivedType(typeof(TextBlock), "text")]
[JsonDerivedType(typeof(ImageBlock), "image")]
[JsonDerivedType(typeof(SuggestionBlock), "suggestion")]
public abstract class Block
{
    public abstract string Type { get; }
}

public class TextBlock : Block
{
    public override string Type => "text";
    public string Content { get; set; } = string.Empty;
}

public class ImageBlock : Block
{
    public override string Type => "image";
    public string? PhotographId { get; set; }
    public string? Caption { get; set; }
}

public class SuggestionBlock : Block
{
    public override string Type => "suggestion";
    public string Prompt { get; set; } = string.Empty;
}

public static class StorySerialization
{
    public static class Fields
    {
        public const string Id = "id";
        public const string Title = "title";
        public const string StartDate = "startDate";
        public const string EndDate = "endDate";
        public const string Journal = "journal";
        public const string Sections = "sections";
        public const string CreatedAt = "createdAt";
    }

    public static Story FromDocument(Document document)
    {
        return new Story
        {
            Id = document[Fields.Id].AsGuid(),
            Title = document[Fields.Title].AsString(),
            StartDate = document[Fields.StartDate].AsDateTimeNullable(),
            EndDate = document[Fields.EndDate].AsDateTimeNullable(),
            Journal = document[Fields.Journal].AsString(),
            Sections = document[Fields.Sections].AsListOfDocument().Select(SectionSerialization.FromDocument).ToList(),
            CreatedAt = document[Fields.CreatedAt].AsDateTime(),
        };
    }

    public static Document ToDocument(Story story)
    {
        return new Document
        {
            [Fields.Id] = story.Id.ToString(),
            [Fields.Title] = story.Title,
            [Fields.StartDate] = story.StartDate,
            [Fields.EndDate] = story.EndDate,
            [Fields.Journal] = story.Journal,
            [Fields.Sections] = new DynamoDBList(story.Sections.Select(SectionSerialization.ToDocument)),
            [Fields.CreatedAt] = story.CreatedAt,
        };
    }
}

public static class SectionSerialization
{
    public static class Fields
    {
        public const string Id = "id";
        public const string Title = "title";
        public const string Description = "description";
        public const string StartDate = "startDate";
        public const string EndDate = "endDate";
        public const string Blocks = "blocks";
    }

    public static Section FromDocument(Document document)
    {
        return new Section
        {
            Id = document[Fields.Id].AsGuid(),
            Title = document.TryGetValue(Fields.Title, out var title) ? title.AsString() : null,
            Description = document.TryGetValue(Fields.Description, out var summary) ? summary.AsString() : null,
            StartDate = document[Fields.StartDate].AsDateTimeNullable(),
            EndDate = document[Fields.EndDate].AsDateTimeNullable(),
            Blocks = document[Fields.Blocks].AsListOfDocument().Select(BlockSerialization.FromDocument).ToList(),
        };
    }

    public static Document ToDocument(Section section)
    {
        return new Document
        {
            [Fields.Id] = section.Id.ToString(),
            [Fields.Title] = section.Title,
            [Fields.Description] = section.Description,
            [Fields.StartDate] = section.StartDate,
            [Fields.EndDate] = section.EndDate,
            [Fields.Blocks] = new DynamoDBList(section.Blocks.Select(BlockSerialization.ToDocument)),
        };
    }
}

public static class BlockSerialization
{
    public static class Fields
    {
        public const string Type = "type";
    }

    public static Block FromDocument(Document document)
    {
        return document[Fields.Type].AsString() switch
        {
            "text" => TextBlockSerialization.FromDocument(document),
            "image" => ImageBlockSerialization.FromDocument(document),
            "suggestion" => SuggestionBlockSerialization.FromDocument(document),
            _ => throw new NotSupportedException("Unsupported block type")
        };
    }

    public static Document ToDocument(Block block)
    {
        return block switch
        {
            TextBlock textBlock => TextBlockSerialization.ToDocument(textBlock),
            ImageBlock imageBlock => ImageBlockSerialization.ToDocument(imageBlock),
            SuggestionBlock suggestionBlock => SuggestionBlockSerialization.ToDocument(suggestionBlock),
            _ => throw new NotSupportedException("Unsupported block type")
        };
    }
}

public static class TextBlockSerialization
{
    public static class Fields
    {
        public const string Content = "content";
    }

    public static TextBlock FromDocument(Document document)
    {
        return new TextBlock
        {
            Content = document[Fields.Content].AsString(),
        };
    }

    public static Document ToDocument(TextBlock textBlock)
    {
        return new Document
        {
            [BlockSerialization.Fields.Type] = "text",
            [Fields.Content] = textBlock.Content,
        };
    }
}

public static class ImageBlockSerialization
{
    public static class Fields
    {
        public const string PhotographId = "photographId";
        public const string Caption = "caption";
    }

    public static ImageBlock FromDocument(Document document)
    {
        return new ImageBlock
        {
            PhotographId = document[Fields.PhotographId].AsString(),
            Caption = document.TryGetValue(Fields.Caption, out var caption) ? caption.AsString() : null,
        };
    }

    public static Document ToDocument(ImageBlock imageBlock)
    {
        return new Document
        {
            [BlockSerialization.Fields.Type] = "image",
            [Fields.PhotographId] = imageBlock.PhotographId,
            [Fields.Caption] = imageBlock.Caption,
        };
    }
}

public static class SuggestionBlockSerialization
{
    public static class Fields
    {
        public const string Prompt = "prompt";
    }

    public static SuggestionBlock FromDocument(Document document)
    {
        return new SuggestionBlock
        {
            Prompt = document[Fields.Prompt].AsString(),
        };
    }

    public static Document ToDocument(SuggestionBlock suggestionBlock)
    {
        return new Document
        {
            [BlockSerialization.Fields.Type] = "suggestion",
            [Fields.Prompt] = suggestionBlock.Prompt,
        };
    }
}

public static class StoryTable
{
    public static ITable Create(IAmazonDynamoDB client) => new TableBuilder(client, TableNames.Story)
        .AddHashKey(StorySerialization.Fields.Id, DynamoDBEntryType.String)
        .Build();
}