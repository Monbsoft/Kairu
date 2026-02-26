using Kairudev.Api.Journal.Presenters;
using Kairudev.Application.Journal.AddJournalComment;
using Kairudev.Application.Journal.GetTodayJournal;
using Kairudev.Application.Journal.RemoveJournalComment;
using Kairudev.Application.Journal.UpdateJournalComment;
using Kairudev.Domain.Journal;
using Microsoft.AspNetCore.Mvc;

namespace Kairudev.Api.Journal;

[ApiController]
[Route("api/journal")]
public sealed class JournalController : ControllerBase
{
    private readonly IJournalEntryRepository _repository;

    public JournalController(IJournalEntryRepository repository) => _repository = repository;

    // GET api/journal/today
    [HttpGet("today")]
    public async Task<IActionResult> GetToday(CancellationToken cancellationToken)
    {
        var presenter = new GetTodayJournalHttpPresenter();
        await new GetTodayJournalInteractor(_repository, presenter)
            .Execute(new GetTodayJournalRequest(), cancellationToken);
        return presenter.Result!;
    }

    // POST api/journal/{entryId}/comments
    [HttpPost("{entryId:guid}/comments")]
    public async Task<IActionResult> AddComment(
        Guid entryId,
        [FromBody] AddCommentBody body,
        CancellationToken cancellationToken)
    {
        var presenter = new AddJournalCommentHttpPresenter();
        await new AddJournalCommentInteractor(_repository, presenter)
            .Execute(new AddJournalCommentRequest(entryId, body.Text), cancellationToken);
        return presenter.Result!;
    }

    // PUT api/journal/{entryId}/comments/{commentId}
    [HttpPut("{entryId:guid}/comments/{commentId:guid}")]
    public async Task<IActionResult> UpdateComment(
        Guid entryId,
        Guid commentId,
        [FromBody] UpdateCommentBody body,
        CancellationToken cancellationToken)
    {
        var presenter = new UpdateJournalCommentHttpPresenter();
        await new UpdateJournalCommentInteractor(_repository, presenter)
            .Execute(new UpdateJournalCommentRequest(entryId, commentId, body.Text), cancellationToken);
        return presenter.Result!;
    }

    // DELETE api/journal/{entryId}/comments/{commentId}
    [HttpDelete("{entryId:guid}/comments/{commentId:guid}")]
    public async Task<IActionResult> RemoveComment(
        Guid entryId,
        Guid commentId,
        CancellationToken cancellationToken)
    {
        var presenter = new RemoveJournalCommentHttpPresenter();
        await new RemoveJournalCommentInteractor(_repository, presenter)
            .Execute(new RemoveJournalCommentRequest(entryId, commentId), cancellationToken);
        return presenter.Result!;
    }
}

public sealed record AddCommentBody(string Text);
public sealed record UpdateCommentBody(string Text);
