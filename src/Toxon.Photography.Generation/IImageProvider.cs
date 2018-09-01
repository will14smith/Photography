using System.Collections.Generic;
using System.Threading.Tasks;
using Toxon.Photography.Generation.Models;

namespace Toxon.Photography.Generation
{
    public interface IImageProvider
    {
        /// <summary>
        /// Ordered list of photographs to appear on the front page
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<PhotographViewModel>> GetPrimaryPhotographsAsync();
    }
}