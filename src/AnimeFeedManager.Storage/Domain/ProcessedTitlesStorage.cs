﻿namespace AnimeFeedManager.Storage.Domain;

public class ProcessedTitlesStorage : ITableEntity
{
    public string? Title { get; set; }
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}