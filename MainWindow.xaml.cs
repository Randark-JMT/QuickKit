using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Windowing;
using QuickKit.Pages;
using WinRT.Interop;
using System;
using System.Runtime.InteropServices;

namespace QuickKit;

public sealed partial class MainWindow : Window
{
    private const int MinWidth = 800;
    private const int MinHeight = 600;

    private IntPtr _originalWndProc;
    private readonly WndProcDelegate _wndProcDelegate;
    private bool _minSizeHooked;

    public MainWindow()
    {
        InitializeComponent();
        _wndProcDelegate = SubclassWndProc;
        var hwnd = WindowNative.GetWindowHandle(this);
        HookMinSize(hwnd);
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

    private void HookMinSize(IntPtr hwnd)
    {
        if (_minSizeHooked) return;
        _originalWndProc = GetWindowLongPtr(hwnd, GWLP_WNDPROC);
        if (_originalWndProc == IntPtr.Zero) return;
        SetWindowLongPtr(hwnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_wndProcDelegate));
        _minSizeHooked = true;
    }

    private IntPtr SubclassWndProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        const uint WM_GETMINMAXINFO = 0x24;
        if (msg == WM_GETMINMAXINFO)
        {
            IntPtr result = CallWindowProc(_originalWndProc, hwnd, msg, wParam, lParam);
            var info = Marshal.PtrToStructure<MINMAXINFO>(lParam);
            uint dpi = GetDpiForWindow(hwnd);
            float scale = dpi / 96f;
            info.ptMinTrackSize.x = (int)(MinWidth * scale);
            info.ptMinTrackSize.y = (int)(MinHeight * scale);
            Marshal.StructureToPtr(info, lParam, false);
            return result;
        }
        return CallWindowProc(_originalWndProc, hwnd, msg, wParam, lParam);
    }

    #region Win32 最小尺寸

    private const int GWLP_WNDPROC = -4;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate IntPtr WndProcDelegate(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)]
    private static extern IntPtr GetWindowLong32(IntPtr hwnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hwnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
    private static extern IntPtr SetWindowLong32(IntPtr hwnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hwnd, int nIndex, IntPtr dwNewLong);

    private static IntPtr GetWindowLongPtr(IntPtr hwnd, int nIndex) =>
        IntPtr.Size == 8 ? GetWindowLongPtr64(hwnd, nIndex) : GetWindowLong32(hwnd, nIndex);

    private static IntPtr SetWindowLongPtr(IntPtr hwnd, int nIndex, IntPtr value) =>
        IntPtr.Size == 8 ? SetWindowLongPtr64(hwnd, nIndex, value) : SetWindowLong32(hwnd, nIndex, value);

    [DllImport("user32.dll", EntryPoint = "CallWindowProcW")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hwnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hwnd);

    #endregion

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
