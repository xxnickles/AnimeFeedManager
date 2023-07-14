﻿using System.Text;

namespace AnimeFeedManager.Features.Domain.Errors;

public sealed class ValidationError
{
    public KeyValuePair<string, string[]> Error { get; }

    public ValidationError(string field, string[] errors)
    {
        Error = new KeyValuePair<string, string[]>(field, errors);
    }

    public static ValidationError Create(string field, string[] errors) => new(field, errors);
}

public class ValidationErrors : DomainError
{
    public ImmutableDictionary<string, string[]> Errors { get; }

    public ValidationErrors(string correlationId, IEnumerable<ValidationError> errors)
        : base(correlationId, "One or more validations have failed")
    {
        Errors = new Dictionary<string, string[]>(errors.Select(x => x.Error))
            .ToImmutableDictionary();
    }

    public static ValidationErrors Create(string correlationId, IEnumerable<ValidationError> errors) => new(correlationId, errors);

    public override string ToString()
    {

        var builder = new StringBuilder();
        builder.AppendLine($"[{CorrelationId}] - {Message}");
        builder.AppendLine("Validation Errors");
        foreach (var (key, value) in Errors)
        {
            builder.AppendLine($"{key}: {string.Join(", ", value)}" );
        }

        return builder.ToString();
    }
}