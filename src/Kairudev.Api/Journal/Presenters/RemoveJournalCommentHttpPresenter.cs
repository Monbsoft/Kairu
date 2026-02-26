using Kairudev.Application.Journal.RemoveJournalComment;
using Microsoft.AspNetCore.Mvc;

namespace Kairudev.Api.Journal.Presenters;

public sealed class RemoveJournalCommentHttpPresenter : IRemoveJournalCommentPresenter
{
    public IActionResult? Result { get; private set; }

    public void PresentSuccess() =>
        Result = new NoContentResult();

    public void PresentNotFound() =>
        Result = new NotFoundResult();

    public void PresentFailure(string reason) =>
        Result = new ConflictObjectResult(new { error = reason });
}
