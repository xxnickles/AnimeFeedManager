﻿namespace AnimeFeedManager.Storage.Domain;

public class MoviesSubscriptionStorage : ITableEntity
{
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public DateTimeOffset? DateToNotify { get; set; }
    public ETag ETag { get; set; }
    public bool Processed { get; set; }
}