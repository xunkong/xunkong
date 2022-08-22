namespace Xunkong.Desktop;

public static class Program
{
    [global::System.Runtime.InteropServices.DllImport("Microsoft.ui.xaml.dll")]
    private static extern void XamlCheckProcessRequirements();

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler", " 1.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.STAThreadAttribute]
    static async Task Main(string[] args)
    {

        Environment.CurrentDirectory = AppContext.BaseDirectory;

        if (args.FirstOrDefault() == "DoNotClickToast")
        {
            return;
        }

        if (args.FirstOrDefault() == "StartGame")
        {
            if (await InvokeService.StartGameAsync())
            {
                await InvokeService.CheckTransformerReachedAndHomeCoinFullAsync();
            }
            return;
        }

        if (args.FirstOrDefault() == "DailyCheckIn")
        {
            await InvokeService.SignInAllAccountAsync();
            return;
        }

        if (args.FirstOrDefault() == "RefreshTile")
        {
            await InvokeService.RefreshDailyNoteTilesAsync();
            return;
        }

        if (args.FirstOrDefault() == "dailynote")
        {
            await InvokeService.RefreshDailyNoteTilesAsync();
            return;
        }

        if (args.FirstOrDefault() == "/InvokerPRAID:")
        {
            if (args[2] == "DailyNoteTask")
            {
                await InvokeService.RefreshDailyNoteTilesAsync();
            }
            if (args[2] == "HoyolabCheckInTask")
            {
                await InvokeService.SignInAllAccountAsync();
            }
            return;
        }

        XamlCheckProcessRequirements();

        global::WinRT.ComWrappersSupport.InitializeComWrappers();
        global::Microsoft.UI.Xaml.Application.Start((p) =>
        {
            var context = new global::Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(global::Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
            global::System.Threading.SynchronizationContext.SetSynchronizationContext(context);
            new App();
        });
    }
}
