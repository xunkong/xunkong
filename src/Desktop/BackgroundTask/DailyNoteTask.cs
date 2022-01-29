using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace Xunkong.Desktop.BackgroundTask
{
    public sealed class DailyNoteTask : IBackgroundTask
    {


        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            var path = ApplicationData.Current.LocalSettings.Values["UserDataPath"] as string;
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }
            await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("DailyNoteTask");
            deferral.Complete();
        }


    }
}
