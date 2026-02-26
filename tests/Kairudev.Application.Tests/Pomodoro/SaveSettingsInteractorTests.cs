using Kairudev.Application.Pomodoro.SaveSettings;
using Kairudev.Domain.Pomodoro;

namespace Kairudev.Application.Tests.Pomodoro;

public sealed class SaveSettingsInteractorTests
{
    private sealed class FakePresenter : ISaveSettingsPresenter
    {
        public bool SuccessCalled { get; private set; }
        public string? ValidationError { get; private set; }

        public void PresentSuccess() => SuccessCalled = true;

        public void PresentValidationError(string error) => ValidationError = error;
    }

    [Fact]
    public async Task Should_SaveSettings_When_ValidDurations()
    {
        var repository = new FakePomodoroSettingsRepository();
        var presenter = new FakePresenter();
        var interactor = new SaveSettingsInteractor(repository, presenter);
        var request = new SaveSettingsRequest(25, 5, 15);

        await interactor.Execute(request);

        Assert.True(presenter.SuccessCalled);
        Assert.Equal(25, repository.Settings.SprintDurationMinutes);
        Assert.Equal(5, repository.Settings.ShortBreakDurationMinutes);
        Assert.Equal(15, repository.Settings.LongBreakDurationMinutes);
    }

    [Theory]
    [InlineData(0, 5, 15)]
    [InlineData(-1, 5, 15)]
    [InlineData(121, 5, 15)]
    public async Task Should_PresentValidationError_When_InvalidSprintDuration(int sprint, int shortBreak, int longBreak)
    {
        var repository = new FakePomodoroSettingsRepository();
        var presenter = new FakePresenter();
        var interactor = new SaveSettingsInteractor(repository, presenter);
        var request = new SaveSettingsRequest(sprint, shortBreak, longBreak);

        await interactor.Execute(request);

        Assert.False(presenter.SuccessCalled);
        Assert.NotNull(presenter.ValidationError);
        Assert.Equal(PomodoroSettings.Default, repository.Settings);
    }

    [Theory]
    [InlineData(25, 0, 15)]
    [InlineData(25, -1, 15)]
    [InlineData(25, 121, 15)]
    public async Task Should_PresentValidationError_When_InvalidShortBreakDuration(int sprint, int shortBreak, int longBreak)
    {
        var repository = new FakePomodoroSettingsRepository();
        var presenter = new FakePresenter();
        var interactor = new SaveSettingsInteractor(repository, presenter);
        var request = new SaveSettingsRequest(sprint, shortBreak, longBreak);

        await interactor.Execute(request);

        Assert.False(presenter.SuccessCalled);
        Assert.NotNull(presenter.ValidationError);
        Assert.Equal(PomodoroSettings.Default, repository.Settings);
    }

    [Theory]
    [InlineData(25, 5, 0)]
    [InlineData(25, 5, -1)]
    [InlineData(25, 5, 121)]
    public async Task Should_PresentValidationError_When_InvalidLongBreakDuration(int sprint, int shortBreak, int longBreak)
    {
        var repository = new FakePomodoroSettingsRepository();
        var presenter = new FakePresenter();
        var interactor = new SaveSettingsInteractor(repository, presenter);
        var request = new SaveSettingsRequest(sprint, shortBreak, longBreak);

        await interactor.Execute(request);

        Assert.False(presenter.SuccessCalled);
        Assert.NotNull(presenter.ValidationError);
        Assert.Equal(PomodoroSettings.Default, repository.Settings);
    }

    [Fact]
    public async Task Should_OverwriteExistingSettings()
    {
        var repository = new FakePomodoroSettingsRepository();
        repository.Settings = PomodoroSettings.Create(30, 10, 20).Value;
        var presenter = new FakePresenter();
        var interactor = new SaveSettingsInteractor(repository, presenter);
        var request = new SaveSettingsRequest(45, 7, 25);

        await interactor.Execute(request);

        Assert.True(presenter.SuccessCalled);
        Assert.Equal(45, repository.Settings.SprintDurationMinutes);
        Assert.Equal(7, repository.Settings.ShortBreakDurationMinutes);
        Assert.Equal(25, repository.Settings.LongBreakDurationMinutes);
    }

    [Fact]
    public async Task Should_AcceptMinimumValidDurations()
    {
        var repository = new FakePomodoroSettingsRepository();
        var presenter = new FakePresenter();
        var interactor = new SaveSettingsInteractor(repository, presenter);
        var request = new SaveSettingsRequest(1, 1, 1);

        await interactor.Execute(request);

        Assert.True(presenter.SuccessCalled);
        Assert.Equal(1, repository.Settings.SprintDurationMinutes);
    }

    [Fact]
    public async Task Should_AcceptMaximumValidDurations()
    {
        var repository = new FakePomodoroSettingsRepository();
        var presenter = new FakePresenter();
        var interactor = new SaveSettingsInteractor(repository, presenter);
        var request = new SaveSettingsRequest(120, 120, 120);

        await interactor.Execute(request);

        Assert.True(presenter.SuccessCalled);
        Assert.Equal(120, repository.Settings.SprintDurationMinutes);
        Assert.Equal(120, repository.Settings.ShortBreakDurationMinutes);
        Assert.Equal(120, repository.Settings.LongBreakDurationMinutes);
    }
}
