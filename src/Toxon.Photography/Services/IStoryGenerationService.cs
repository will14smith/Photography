using Toxon.Photography.Data;
using Toxon.Photography.Models;

namespace Toxon.Photography.Services;

public interface IStoryGenerationService
{
    Task<StoryAnalysis> AnalyseStoryForSectionsAsync(Story story);
    Task<IEnumerable<Block>> GenerateSectionBlocksAsync(Story story, StorySectionAnalysis sectionAnalysis);
}

