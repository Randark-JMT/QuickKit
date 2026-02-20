using Microsoft.UI.Xaml.Controls;
using QuickKit.Models;
using QuickKit.Services;
using System.Collections.Generic;
using System.Linq;

namespace QuickKit.Pages;

public sealed partial class JavaVersionPage : Page
{
    public JavaVersionPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        RefreshJavaList();
    }

    private void RefreshJavaList()
    {
        var distributions = JavaDetectionService.DiscoverDistributions();
        JavaList.ItemsSource = distributions;

        var active = JavaDetectionService.GetActiveJavaHome();
        var activeDist = distributions.FirstOrDefault(d => d.IsActive);

        if (activeDist != null)
        {
            ActiveVersionPanel.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            NoActiveVersionText.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            ActiveVersionText.Text = $"{activeDist.DisplayName}（{activeDist.Version}）";
            ActivePathText.Text = activeDist.HomePath;
        }
        else if (!string.IsNullOrWhiteSpace(active))
        {
            ActiveVersionPanel.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            NoActiveVersionText.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            ActiveVersionText.Text = "JAVA_HOME 已设置，但未在已安装列表中识别";
            ActivePathText.Text = active;
        }
        else
        {
            ActiveVersionPanel.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            NoActiveVersionText.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        }
    }
}
