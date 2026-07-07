using KairuFocus.Domain.Common;

namespace KairuFocus.Application.Settings.Commands.SaveAlarmVolume;

public sealed class SaveAlarmVolumeResult : Result
{
    private SaveAlarmVolumeResult(bool isSuccess, string error) : base(isSuccess, error) { }

    public static new SaveAlarmVolumeResult Success() => new(true, string.Empty);
    public static new SaveAlarmVolumeResult Failure(string error) => new(false, error);
}
