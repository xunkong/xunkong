using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace Xunkong.Desktop.Background;

public sealed class UpdateTask : IBackgroundTask
{
    public void Run(IBackgroundTaskInstance taskInstance)
    {
        ApplicationData.Current.LocalSettings.Values["ShowUpdateContentOnLoaded"] = true;
        ApplicationData.Current.LocalSettings.Values["HasShownWelcomePage"] = false;
    }
}
