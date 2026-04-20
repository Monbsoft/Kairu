using Microsoft.JSInterop;

namespace KairuFocus.Web.Services;

public sealed class SoundService : ISoundService
{
    private readonly IJSRuntime _jsRuntime;

    private static readonly Dictionary<string, string> SoundFiles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AlarmClock"] = "sounds/alarm-clock.mp3",
        ["Bird"]       = "sounds/bird.mp3"
    };

    public SoundService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task PlayRingtoneAsync(string ringtonePreference)
    {
        if (ringtonePreference == "None") return;

        if (!SoundFiles.TryGetValue(ringtonePreference, out var soundFile)) return;

        await _jsRuntime.InvokeVoidAsync("kairufocusSound.play", soundFile);
    }
}
