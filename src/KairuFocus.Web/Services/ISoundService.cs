namespace KairuFocus.Web.Services;

public interface ISoundService
{
    Task PlayRingtoneAsync(string ringtonePreference, int volumePercent);
}
