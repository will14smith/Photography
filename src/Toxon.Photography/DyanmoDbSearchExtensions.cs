using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;

namespace Toxon.Photography
{
    public static class DyanmoDbSearchExtensions
    {
        public static async Task<IEnumerable<Document>> GetAllAsync(this Search search)
        {
            var documents = new List<Document>();

            do
            {
                documents.AddRange(await search.GetNextSetAsync());
            } while (!search.IsDone);

            return documents;
        }
    }
}