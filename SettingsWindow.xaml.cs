using System.Windows;
using System.Windows.Input;
using DeepSeekUsageTracker.Services;

namespace DeepSeekUsageTracker;

public partial class SettingsWindow : Window
{
    private readonly AppConfig _config;

    public SettingsWindow(AppConfig config)
    {
        InitializeComponent();
        _config = config;

        if (!string.IsNullOrEmpty(_config.ApiKey))
        {
            ApiKeyBox.Password = _config.ApiKey;
            HintText.Text = "Key loaded. Enter a new one to replace.";
        }

        // Select the saved interval radio
        var selected = _config.RefreshIntervalMinutes switch
        {
            1 => Rb1m, 5 => Rb5m, 30 => Rb30m, 60 => Rb60m, _ => Rb10m
        };
        selected.IsChecked = true;
    }

    private void Interval_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.RadioButton rb || rb.IsChecked != true) return;

        _config.RefreshIntervalMinutes = rb.Name switch
        {
            "Rb1m" => 1,
            "Rb5m" => 5,
            "Rb30m" => 30,
            "Rb60m" => 60,
            _ => 10
        };
    }

    private void SaveBtn_Click(object sender, RoutedEventArgs e)
    {
        var key = ApiKeyBox.Password.Trim();
        if (string.IsNullOrEmpty(key))
        {
            HintText.Text = "Please enter your API key.";
            return;
        }

        _config.ApiKey = key;
        DialogResult = true;
        Close();
    }

    private void CancelBtn_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }
}
