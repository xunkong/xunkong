using System;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace Xunkong.Desktop.Background;

public sealed class UpdateTask : IBackgroundTask
{
    public void Run(IBackgroundTaskInstance taskInstance)
    {
        ApplicationData.Current.LocalSettings.Values["ShowUpdateContentOnLoaded"] = true;
        if (Version.TryParse(ApplicationData.Current.LocalSettings.Values["LastVersion"]?.ToString(), out var version))
        {
            if (version < new Version("1.2.0.0"))
            {
                ApplicationData.Current.LocalSettings.Values["HasShownWelcomePage"] = false;
            }
        }
    }
}
