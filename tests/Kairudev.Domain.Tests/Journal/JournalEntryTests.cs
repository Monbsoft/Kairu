using Kairudev.Domain.Identity;
using Kairudev.Domain.Journal;

namespace Kairudev.Domain.Tests.Journal;

public sealed class JournalEntryTests
{
    private static readonly UserId OwnerId = UserId.From("test-user");

    private static JournalEntry CreateEntry(
        JournalEventType eventType = JournalEventType.SprintStarted,
        Guid? resourceId = null) =>
        JournalEntry.Create(eventType, resourceId ?? Guid.NewGuid(), DateTime.UtcNow, OwnerId);

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Should_HaveCorrectProperties_When_Created()
    {
        var resourceId = Guid.NewGuid();
        var before = DateTime.UtcNow;
        var entry = JournalEntry.Create(JournalEventType.TaskCompleted, resourceId, DateTime.UtcNow, OwnerId);
        var after = DateTime.UtcNow;

        Assert.Equal(JournalEventType.TaskCompleted, entry.EventType);
        Assert.Equal(resourceId, entry.ResourceId);
        Assert.InRange(entry.OccurredAt, before, after);
        Assert.Empty(entry.Comments);
    }

    [Fact]
    public void Should_HaveEmptyComments_When_Created()
    {
        var entry = CreateEntry();

        Assert.Empty(entry.Comments);
    }

    // ── AddComment ────────────────────────────────────────────────────────────

    [Fact]
    public void Should_AddComment_When_TextIsValid()
    {
        var entry = CreateEntry();

        var result = entry.AddComment("Sprint review done.");

        Assert.True(result.IsSuccess);
        Assert.Single(entry.Comments);
        Assert.Equal("Sprint review done.", entry.Comments[0].Text);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ReturnFailure_When_AddingEmptyComment(string text)
    {
        var entry = CreateEntry();

        var result = entry.AddComment(text);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Journal.EmptyComment, result.Error);
        Assert.Empty(entry.Comments);
    }

    [Fact]
    public void Should_ReturnFailure_When_AddingCommentTooLong()
    {
        var entry = CreateEntry();
        var longText = new string('x', 1001);

        var result = entry.AddComment(longText);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Journal.CommentTooLong, result.Error);
        Assert.Empty(entry.Comments);
    }

    [Fact]
    public void Should_ReturnCommentId_When_AddingValidComment()
    {
        var entry = CreateEntry();

        var result = entry.AddComment("My comment");

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Value);
    }

    // ── UpdateComment ─────────────────────────────────────────────────────────

    [Fact]
    public void Should_UpdateComment_When_CommentExistsAndTextIsValid()
    {
        var entry = CreateEntry();
        var addResult = entry.AddComment("Original text");
        var commentId = addResult.Value;

        var result = entry.UpdateComment(commentId, "Updated text");

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated text", entry.Comments[0].Text);
    }

    [Fact]
    public void Should_ReturnFailure_When_UpdatingNonExistentComment()
    {
        var entry = CreateEntry();
        var unknownId = JournalCommentId.New();

        var result = entry.UpdateComment(unknownId, "text");

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Journal.CommentNotFound, result.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ReturnFailure_When_UpdatingCommentWithEmptyText(string text)
    {
        var entry = CreateEntry();
        var addResult = entry.AddComment("Initial");
        var commentId = addResult.Value;

        var result = entry.UpdateComment(commentId, text);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Journal.EmptyComment, result.Error);
        Assert.Equal("Initial", entry.Comments[0].Text);
    }

    [Fact]
    public void Should_ReturnFailure_When_UpdatingCommentWithTooLongText()
    {
        var entry = CreateEntry();
        var addResult = entry.AddComment("Initial");
        var commentId = addResult.Value;
        var longText = new string('x', 1001);

        var result = entry.UpdateComment(commentId, longText);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Journal.CommentTooLong, result.Error);
    }

    // ── RemoveComment ─────────────────────────────────────────────────────────

    [Fact]
    public void Should_RemoveComment_When_CommentExists()
    {
        var entry = CreateEntry();
        var addResult = entry.AddComment("To remove");
        var commentId = addResult.Value;

        var result = entry.RemoveComment(commentId);

        Assert.True(result.IsSuccess);
        Assert.Empty(entry.Comments);
    }

    [Fact]
    public void Should_ReturnFailure_When_RemovingNonExistentComment()
    {
        var entry = CreateEntry();
        var unknownId = JournalCommentId.New();

        var result = entry.RemoveComment(unknownId);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Journal.CommentNotFound, result.Error);
    }
}
