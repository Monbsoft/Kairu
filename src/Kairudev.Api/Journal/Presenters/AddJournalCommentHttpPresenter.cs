using Kairudev.Application.Journal.AddJournalComment;
using Kairudev.Application.Journal.Common;
using Microsoft.AspNetCore.Mvc;

namespace Kairudev.Api.Journal.Presenters;

public sealed class AddJournalCommentHttpPresenter : IAddJournalCommentPresenter
{
    public IActionResult? Result { get; private set; }

    public void PresentSuccess(JournalEntryViewModel entry) =>
        Result = new OkObjectResult(entry);

    public void PresentNotFound() =>
        Result = new NotFoundResult();

    public void PresentValidationError(string error) =>
        Result = new BadRequestObjectResult(new { error });

    public void PresentFailure(string reason) =>
        Result = new ConflictObjectResult(new { error = reason });
}
