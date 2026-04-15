using KairuFocus.Domain.Tasks;

namespace KairuFocus.Domain.Tests.Tasks;

public sealed class JiraTicketKeyTests
{
    [Theory]
    [InlineData("PROJ-123")]
    [InlineData("ABC-1")]
    [InlineData("KAIRUFOCUS-456")]
    public void Should_CreateKey_When_ValueIsValid(string value)
    {
        var result = JiraTicketKey.Create(value);

        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Should_ReturnFailure_When_KeyIsEmpty(string? value)
    {
        var result = JiraTicketKey.Create(value!);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Tasks.EmptyJiraTicketKey, result.Error);
    }

    [Theory]
    [InlineData("proj-123")]
    [InlineData("PROJ123")]
    [InlineData("123")]
    [InlineData("PROJ-")]
    [InlineData("-123")]
    [InlineData("PROJ-12A")]
    public void Should_ReturnFailure_When_FormatIsInvalid(string value)
    {
        var result = JiraTicketKey.Create(value);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Tasks.InvalidJiraTicketKeyFormat, result.Error);
    }

    [Fact]
    public void Should_ReturnFailure_When_KeyExceedsMaxLength()
    {
        var longKey = new string('A', 47) + "-123"; // 51 chars > MaxLength(50)

        var result = JiraTicketKey.Create(longKey);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Tasks.JiraTicketKeyTooLong, result.Error);
    }

    [Fact]
    public void Should_ReturnStringValue_When_ToString()
    {
        var key = JiraTicketKey.Create("PROJ-123").Value;

        Assert.Equal("PROJ-123", key.ToString());
    }
}
