// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Summaries;

public sealed partial class SummaryCard : UserControl
{
    public SummaryCard()
    {
        this.InitializeComponent();
    }





    public string Title
    {
        get { return (string)GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(SummaryCard), new PropertyMetadata(null));





    public string Message
    {
        get { return (string)GetValue(MessageProperty); }
        set { SetValue(MessageProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Message.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register("Message", typeof(string), typeof(SummaryCard), new PropertyMetadata(null));




    public string Unit
    {
        get { return (string)GetValue(UnitProperty); }
        set { SetValue(UnitProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Unit.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty UnitProperty =
        DependencyProperty.Register("Unit", typeof(string), typeof(SummaryCard), new PropertyMetadata(null));






}
