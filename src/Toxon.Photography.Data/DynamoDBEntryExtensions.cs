using System;
using Amazon.DynamoDBv2.DocumentModel;

namespace Toxon.Photography.Data
{
    // ReSharper disable once InconsistentNaming same as original typre
    public static class DynamoDBEntryExtensions
    {
        public static DynamoDBEntry TryGetNull(this Document document, string attributeName)
        {
            return document.TryGetValue(attributeName, out var entry) ? entry : new DynamoDBNull();
        }

        public static T AsEnum<T>(this DynamoDBEntry entry)
        {
            return (T)Enum.Parse(typeof(T), entry.AsString());
        }

        public static int? AsIntNullable(this DynamoDBEntry entry)
        {
            if (entry is DynamoDBNull)
            {
                return null;
            }

            return entry.AsInt();
        }
    }
}
