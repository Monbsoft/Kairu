using System.Globalization;
using Kairudev.Application.Journal.Commands.AddComment;
using Kairudev.Application.Journal.Commands.RemoveComment;
using Kairudev.Application.Journal.Commands.UpdateComment;
using Kairudev.Application.Journal.Queries.GetJournalByDate;
using Kairudev.Application.Journal.Queries.GetTodayJournal;
using Microsoft.AspNetCore.Mvc;

namespace Kairudev.Api.Journal;

[ApiController]
[Route("api/journal")]
public sealed class JournalController : ControllerBase
{
    private readonly GetTodayJournalQueryHandler _getTodayJournal;
    private readonly GetJournalByDateQueryHandler _getJournalByDate;
    private readonly AddCommentCommandHandler _addComment;
    private readonly UpdateCommentCommandHandler _updateComment;
    private readonly RemoveCommentCommandHandler _removeComment;

    public JournalController(
        GetTodayJournalQueryHandler getTodayJournal,
        GetJournalByDateQueryHandler getJournalByDate,
        AddCommentCommandHandler addComment,
        UpdateCommentCommandHandler updateComment,
        RemoveCommentCommandHandler removeComment)
    {
        _getTodayJournal = getTodayJournal;
        _getJournalByDate = getJournalByDate;
        _addComment = addComment;
        _updateComment = updateComment;
        _removeComment = removeComment;
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetToday(CancellationToken ct)
    {
        var result = await _getTodayJournal.HandleAsync(new GetTodayJournalQuery(), ct);
        return Ok(result.Entries);
    }

    [HttpGet("date/{date}")]
    public async Task<IActionResult> GetByDate(string date, CancellationToken ct)
    {
        if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            return BadRequest("Format de date invalide. Utiliser yyyy-MM-dd.");

        var result = await _getJournalByDate.HandleAsync(new GetJournalByDateQuery(parsedDate), ct);
        return Ok(result.Entries);
    }

    [HttpPost("{entryId:guid}/comments")]
    public async Task<IActionResult> AddComment(
        Guid entryId,
        [FromBody] AddCommentBody body,
        CancellationToken ct)
    {
        var result = await _addComment.HandleAsync(new AddCommentCommand(entryId, body.Text), ct);

        return result switch
        {
            { IsSuccess: true } => Ok(result.Entry),
            { IsNotFound: true } => NotFound(),
            { ValidationError: not null } => BadRequest(new { error = result.ValidationError }),
            _ => StatusCode(500)
        };
    }

    [HttpPut("{entryId:guid}/comments/{commentId:guid}")]
    public async Task<IActionResult> UpdateComment(
        Guid entryId,
        Guid commentId,
        [FromBody] UpdateCommentBody body,
        CancellationToken ct)
    {
        var result = await _updateComment.HandleAsync(
            new UpdateCommentCommand(entryId, commentId, body.Text), ct);

        return result switch
        {
            { IsSuccess: true } => Ok(result.Entry),
            { IsNotFound: true } => NotFound(),
            { ValidationError: not null } => BadRequest(new { error = result.ValidationError }),
            _ => StatusCode(500)
        };
    }

    [HttpDelete("{entryId:guid}/comments/{commentId:guid}")]
    public async Task<IActionResult> RemoveComment(
        Guid entryId,
        Guid commentId,
        CancellationToken ct)
    {
        var result = await _removeComment.HandleAsync(
            new RemoveCommentCommand(entryId, commentId), ct);

        return result switch
        {
            { IsSuccess: true } => NoContent(),
            { IsNotFound: true } => NotFound(),
            _ => StatusCode(500)
        };
    }
}

public sealed record AddCommentBody(string Text);
public sealed record UpdateCommentBody(string Text);
