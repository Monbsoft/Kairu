using Kairudev.Domain.Identity;

namespace Kairudev.Domain.Tests.Identity;

public sealed class UserIdTests
{
    [Fact]
    public void Should_CreateUserId_When_ValidStringProvided()
    {
        var id = UserId.From("github-12345");

        Assert.Equal("github-12345", id.Value);
    }

    [Fact]
    public void Should_BeEqual_When_SameValue()
    {
        var id1 = UserId.From("github-12345");
        var id2 = UserId.From("github-12345");

        Assert.Equal(id1, id2);
    }

    [Fact]
    public void Should_NotBeEqual_When_DifferentValues()
    {
        var id1 = UserId.From("github-12345");
        var id2 = UserId.From("github-99999");

        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void Should_ReturnValue_When_ToStringCalled()
    {
        var id = UserId.From("github-42");

        Assert.Equal("github-42", id.ToString());
    }

    [Fact]
    public void Should_ThrowArgumentException_When_EmptyString()
    {
        Assert.Throws<ArgumentException>(() => UserId.From(string.Empty));
    }

    [Theory]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Should_ThrowArgumentException_When_WhitespaceString(string value)
    {
        Assert.Throws<ArgumentException>(() => UserId.From(value));
    }

    [Fact]
    public void Should_TrimValue_When_ValueHasSurroundingWhitespace()
    {
        var id = UserId.From("  github-42  ");

        Assert.Equal("github-42", id.Value);
    }
}
