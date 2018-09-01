﻿using System;
using Amazon.DynamoDBv2.DocumentModel;

namespace Toxon.Photography.Data
{
    // ReSharper disable once InconsistentNaming same as original typre
    public static class DynamoDBEntryExtensions
    {
        public static T AsEnum<T>(this DynamoDBEntry entry)
        {
            return (T) Enum.Parse(typeof(T), entry.AsString());
        }
    }
}