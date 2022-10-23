// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Media.Core;
using Xunkong.GenshinData;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Controls;

public sealed partial class CutsceneViewer : UserControl
{


    public CutsceneViewer(Cutscene cutscene)
    {
        this.InitializeComponent();
        PlaterElement.Source = MediaSource.CreateFromUri(new Uri(cutscene.Source));
        PlaterElement.PosterSource = new BitmapImage(new Uri(cutscene.Poster));
    }



    private void Button_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Current.CloseFullWindowContent();
    }


    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        PlaterElement.MediaPlayer.Dispose();
    }

}
