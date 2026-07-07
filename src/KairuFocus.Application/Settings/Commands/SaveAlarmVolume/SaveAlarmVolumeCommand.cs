using Monbsoft.BrilliantMediator.Abstractions.Commands;

namespace KairuFocus.Application.Settings.Commands.SaveAlarmVolume;

public sealed record SaveAlarmVolumeCommand(int Volume) : ICommand<SaveAlarmVolumeResult>;
