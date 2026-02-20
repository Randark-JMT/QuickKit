using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Windowing;
using QuickKit.Pages;
using WinRT.Interop;
using System;

namespace QuickKit;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(TitleBarArea);
        ContentFrame.Navigate(typeof(HomePage));
        MainNav.SelectedItem = NavItemHome;
        Activated += OnActivated;
    }

    private void OnActivated(object sender, WindowActivatedEventArgs e)
    {
        if (e.WindowActivationState != WindowActivationState.Deactivated)
            TrySetWindowBounds();
        Activated -= OnActivated;
    }

    private void TrySetWindowBounds()
    {
        try
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(960, 720));
            appWindow.MoveInZOrderAtTop();
        }
        catch { /* 忽略无法设置时的错误 */ }
    }

    private void MainNav_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer?.Tag is not string tag)
            return;

        Type? pageType = tag switch
        {
            "Home" => typeof(HomePage),
            "JavaVersion" => typeof(JavaVersionPage),
            _ => null
        };

        if (pageType != null)
            ContentFrame.Navigate(pageType, null, args.RecommendedNavigationTransitionInfo);
    }
}
