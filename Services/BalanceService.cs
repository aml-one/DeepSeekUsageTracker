using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeepSeekUsageTracker.Services;

public class BalanceInfo
{
    [JsonPropertyName("is_available")]
    public bool IsAvailable { get; set; }

    [JsonPropertyName("balance_infos")]
    public List<BalanceItem> BalanceInfos { get; set; } = [];
}

public class BalanceItem
{
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "USD";

    [JsonPropertyName("total_balance")]
    public string TotalBalance { get; set; } = "0.00";

    [JsonPropertyName("granted_balance")]
    public string GrantedBalance { get; set; } = "0.00";

    [JsonPropertyName("topped_up_balance")]
    public string ToppedUpBalance { get; set; } = "0.00";
}

public static class BalanceService
{
    private static readonly HttpClient _http = new()
    {
        BaseAddress = new Uri("https://api.deepseek.com/"),
        Timeout = TimeSpan.FromSeconds(15)
    };

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public static async Task<BalanceItem?> FetchBalanceAsync(string apiKey)
    {
        _http.DefaultRequestHeaders.Remove("Authorization");
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        _http.DefaultRequestHeaders.Accept.Clear();
        _http.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _http.GetAsync("user/balance");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<BalanceInfo>(JsonOpts);
        return body?.BalanceInfos?.FirstOrDefault();
    }
}
