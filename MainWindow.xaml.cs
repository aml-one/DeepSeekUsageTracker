using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DeepSeekUsageTracker.Services;

namespace DeepSeekUsageTracker;

public partial class MainWindow : Window
{
    private readonly AppConfig _config;
    private readonly DispatcherTimer _refreshTimer;
    private readonly DispatcherTimer _countdownTimer;
    private int _secondsUntilRefresh;

    private static readonly SolidColorBrush GrayBrush  = new(System.Windows.Media.Color.FromRgb(0xA0, 0xA0, 0xB0));
    private static readonly SolidColorBrush GreenBrush = new(System.Windows.Media.Color.FromRgb(0x00, 0xD2, 0xA0));
    private static readonly SolidColorBrush YellowBrush = new(System.Windows.Media.Color.FromRgb(0xF8, 0xE0, 0x1A));
    private static readonly SolidColorBrush RedBrush   = new(System.Windows.Media.Color.FromRgb(0xE9, 0x45, 0x60));

    public MainWindow()
    {
        InitializeComponent();

        _config = ConfigManager.Load();

        // Main refresh timer
        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(_config.RefreshIntervalMinutes)
        };
        _refreshTimer.Tick += async (_, _) =>
        {
            await RefreshBalanceAsync();
            ResetCountdown();
        };

        // Per-second countdown timer
        _countdownTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _countdownTimer.Tick += (_, _) =>
        {
            _secondsUntilRefresh--;
            if (_secondsUntilRefresh <= 0) _secondsUntilRefresh = 0;
            UpdateCountdownText();
        };
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var wa = SystemParameters.WorkArea;
        Left = wa.Right - Width - 20;
        Top = wa.Top + 80;

        BalancePanel.MouseDown += Balance_MouseDown;
        PercentPanel.MouseDown += Balance_MouseDown;

        SwitchDisplayMode();

        await RefreshBalanceAsync();
        ResetCountdown();
        _refreshTimer.Start();
        _countdownTimer.Start();
    }

    // ── Display mode ──────────────────────────────────────────────────────

    private void SwitchDisplayMode()
    {
        bool isPercent = _config.DisplayMode == "percentage";
        BalancePanel.Visibility = isPercent ? Visibility.Collapsed : Visibility.Visible;
        PercentPanel.Visibility = isPercent ? Visibility.Visible : Visibility.Collapsed;
    }

    // ── Countdown ─────────────────────────────────────────────────────────

    private void ResetCountdown()
    {
        _secondsUntilRefresh = _config.RefreshIntervalMinutes * 60;
        UpdateCountdownText();
    }

    private void UpdateCountdownText()
    {
        if (_secondsUntilRefresh >= 60)
            StatusCountdown.Text = $"{_secondsUntilRefresh / 60:D2}:{_secondsUntilRefresh % 60:D2}";
        else
            StatusCountdown.Text = $"{_secondsUntilRefresh}s";

        StatusDot2.Text = string.IsNullOrEmpty(StatusCountdown.Text) ? "" : "·";
    }

    // ── Balance fetch ─────────────────────────────────────────────────────

    private async Task RefreshBalanceAsync()
    {
        if (string.IsNullOrWhiteSpace(_config.ApiKey))
        {
            ClearDisplay();
            SetStatus("No API key", GrayBrush);
            StatusCountdown.Text = "";
            return;
        }

        try
        {
            SetStatus("Fetching...", GrayBrush);
            var balance = await BalanceService.FetchBalanceAsync(_config.ApiKey);

            if (balance is not null)
            {
                var leftover = decimal.Parse(balance.TotalBalance);
                var hasBalance = leftover > 0;

                if (_config.DisplayMode == "percentage" && _config.ToppedUpAmount > 0)
                {
                    RenderPercentageMode(leftover);
                }
                else
                {
                    RenderBalanceMode(leftover, balance.Currency);
                }

                SetStatus(hasBalance ? "Available" : "Depleted",
                          hasBalance ? GreenBrush : RedBrush);
            }
            else
            {
                ClearDisplay();
                SetStatus("No data", RedBrush);
                StatusCountdown.Text = "";
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            ClearDisplay();
            SetStatus("Invalid key", RedBrush);
            StatusCountdown.Text = "";
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message.Contains("DNS") ? "No internet" : "Connection error", RedBrush);
            StatusCountdown.Text = "";
        }
    }

    private void RenderBalanceMode(decimal leftover, string currency)
    {
        BalanceText.Text = $"{leftover:F2}";
        CurrencyText.Text = currency;
    }

    private void RenderPercentageMode(decimal leftover)
    {
        var topped = _config.ToppedUpAmount;
        var used = topped - leftover;
        if (used < 0) used = 0;
        if (used > topped) used = topped;

        var pct = topped > 0 ? used / topped * 100m : 0m;

        PercentText.Text = $"{pct:F1}%";
        PercentDetail.Text = $"${used:F2} / ${topped:F2}";

        // Fill the 4px progress bar
        double ratio = (double)(topped > 0 ? used / topped : 0);
        ratio = Math.Clamp(ratio, 0, 1);
        var parentWidth = PercentPanel.ActualWidth - 24; // padding
        if (parentWidth <= 0) parentWidth = 176;
        ProgressFill.Width = ratio * parentWidth;

        // Color: green → yellow → red
        if (pct > 80m)
            ProgressFill.Background = RedBrush;
        else if (pct > 50m)
            ProgressFill.Background = YellowBrush;
        else
            ProgressFill.Background = GreenBrush;
    }

    private void ClearDisplay()
    {
        BalanceText.Text = "--.--";
        CurrencyText.Text = "---";
        PercentText.Text = "--%";
        PercentDetail.Text = "";
        ProgressFill.Width = 0;
    }

    private void SetStatus(string text, SolidColorBrush color)
    {
        StatusLabel.Text = text;
        StatusLabel.Foreground = color;
        StatusDot.Fill = color;
    }

    // ── Double-click refresh ──────────────────────────────────────────────

    private void Balance_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            TriggerImmediateRefresh();
    }

    // ── Dragging ──────────────────────────────────────────────────────────

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    // ── Settings ──────────────────────────────────────────────────────────

    private void SettingsBtn_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SettingsWindow(_config)
        {
            Owner = this,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        if (dialog.ShowDialog() == true)
        {
            ConfigManager.Save(_config);
            _refreshTimer.Interval = TimeSpan.FromMinutes(_config.RefreshIntervalMinutes);
            SwitchDisplayMode();
            TriggerImmediateRefresh();
        }
    }

    private void TriggerImmediateRefresh()
    {
        _refreshTimer.Stop();
        _countdownTimer.Stop();
        _ = RefreshBalanceAsync().ContinueWith(_ =>
        {
            Dispatcher.Invoke(() =>
            {
                ResetCountdown();
                _refreshTimer.Start();
                _countdownTimer.Start();
            });
        });
    }
}
