using Kairudev.Application.Journal.Common;
using Kairudev.Application.Journal.GetTodayJournal;
using Microsoft.AspNetCore.Mvc;

namespace Kairudev.Api.Journal.Presenters;

public sealed class GetTodayJournalHttpPresenter : IGetTodayJournalPresenter
{
    public IActionResult? Result { get; private set; }

    public void PresentSuccess(IReadOnlyList<JournalEntryViewModel> entries) =>
        Result = new OkObjectResult(entries);

    public void PresentEmpty() =>
        Result = new OkObjectResult(Array.Empty<JournalEntryViewModel>());
}
