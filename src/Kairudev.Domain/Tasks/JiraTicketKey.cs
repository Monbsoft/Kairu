using System.Text.RegularExpressions;
using Kairudev.Domain.Common;

namespace Kairudev.Domain.Tasks;

public sealed record JiraTicketKey
{
    public const int MaxLength = 50;
    private static readonly Regex ValidFormat = new(@"^[A-Z]+-\d+$", RegexOptions.Compiled);

    public string Value { get; }
    private JiraTicketKey(string value) => Value = value;

    public static Result<JiraTicketKey> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<JiraTicketKey>(DomainErrors.Tasks.EmptyJiraTicketKey);
        if (value.Length > MaxLength)
            return Result.Failure<JiraTicketKey>(DomainErrors.Tasks.JiraTicketKeyTooLong);
        if (!ValidFormat.IsMatch(value))
            return Result.Failure<JiraTicketKey>(DomainErrors.Tasks.InvalidJiraTicketKeyFormat);
        return Result.Success(new JiraTicketKey(value));
    }

    public override string ToString() => Value;
}
