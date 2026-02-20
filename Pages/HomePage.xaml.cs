using Microsoft.UI.Xaml.Controls;
using QuickKit.Services;

namespace QuickKit.Pages;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        CpuList.ItemsSource = DeviceInfoService.GetCpuInfo();
        var mem = DeviceInfoService.GetMemoryInfo();
        if (mem != null)
        {
            MemoryTotalText.Text = mem.TotalGb;
            MemoryAvailableText.Text = mem.AvailableGb;
        }
        else
        {
            MemoryTotalText.Text = "—";
            MemoryAvailableText.Text = "—";
        }
        GpuList.ItemsSource = DeviceInfoService.GetGpuInfo();
        DiskList.ItemsSource = DeviceInfoService.GetDiskInfo();
    }
}
