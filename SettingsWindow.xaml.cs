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

        // Display mode toggle
        bool isPercentage = _config.DisplayMode == "percentage";
        DisplayToggle.IsChecked = isPercentage;
        ToppedUpPanel.Visibility = isPercentage ? Visibility.Visible : Visibility.Collapsed;
        ModeLabel.Text = isPercentage ? "Percentage" : "Balance";
        ToppedUpBox.Text = _config.ToppedUpAmount.ToString("F2");

        // Select the saved interval radio
        var selected = _config.RefreshIntervalMinutes switch
        {
            1 => Rb1m, 5 => Rb5m, 30 => Rb30m, 60 => Rb60m, _ => Rb10m
        };
        selected.IsChecked = true;
    }

    private void DisplayToggle_Changed(object sender, RoutedEventArgs e)
    {
        if (DisplayToggle.IsChecked == true)
        {
            _config.DisplayMode = "percentage";
            ToppedUpPanel.Visibility = Visibility.Visible;
            ModeLabel.Text = "Percentage";
        }
        else
        {
            _config.DisplayMode = "balance";
            ToppedUpPanel.Visibility = Visibility.Collapsed;
            ModeLabel.Text = "Balance";
        }
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

        // Save topped-up amount if in percentage mode
        if (_config.DisplayMode == "percentage")
        {
            if (decimal.TryParse(ToppedUpBox.Text.Trim(), out var amount) && amount > 0)
                _config.ToppedUpAmount = amount;
            else
            {
                HintText.Text = "Please enter a valid Topped Up amount.";
                return;
            }
        }

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
