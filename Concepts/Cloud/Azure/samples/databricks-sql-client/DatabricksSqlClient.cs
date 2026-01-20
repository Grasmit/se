using System.Net.Http.Headers;
using Azure.Core;
using Azure.Identity;
using System.Text.Json;

public class DatabricksSqlClient
{
    private readonly HttpClient _http;
    private readonly TokenCredential _credential;
    private readonly string _workspaceUrl;
    private readonly string _warehouseId;
    private readonly string? _patFallback;

    public DatabricksSqlClient(string workspaceUrl, string warehouseId, string? patFallback = null)
    {
        _http = new HttpClient();
        _credential = new DefaultAzureCredential();
        _workspaceUrl = workspaceUrl.TrimEnd('/');
        _warehouseId = warehouseId;
        _patFallback = patFallback;
    }

    private async Task AuthenticateAsync()
    {
        if (!string.IsNullOrEmpty(_patFallback))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _patFallback);
            return;
        }

        var token = await _credential.GetTokenAsync(
            new TokenRequestContext(new[] { "https://databricks.azure.net/.default" }), CancellationToken.None);

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
    }

    public async Task<JsonElement> ExecuteSqlAsync(string sql, CancellationToken cancellationToken = default)
    {
        await AuthenticateAsync();

        var payload = new Dictionary<string, object?>
        {
            ["statement"] = sql,
            ["warehouse_id"] = _warehouseId
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

        var submitUrl = $"{_workspaceUrl}/api/2.0/sql/statements";
        using var submitResp = await _http.PostAsync(submitUrl, content, cancellationToken);
        submitResp.EnsureSuccessStatusCode();

        using var sr = await submitResp.Content.ReadAsStreamAsync(cancellationToken);
        using var submitJson = await JsonDocument.ParseAsync(sr, cancellationToken: cancellationToken);
        var statementId = submitJson.RootElement.GetProperty("statement_id").GetString();
        if (string.IsNullOrEmpty(statementId)) throw new Exception("Failed to obtain statement id");

        var statusUrl = $"{_workspaceUrl}/api/2.0/sql/statements/{statementId}";

        while (!cancellationToken.IsCancellationRequested)
        {
            using var statusResp = await _http.GetAsync(statusUrl, cancellationToken);
            statusResp.EnsureSuccessStatusCode();
            using var st = await statusResp.Content.ReadAsStreamAsync(cancellationToken);
            using var statusJson = await JsonDocument.ParseAsync(st, cancellationToken: cancellationToken);

            var state = statusJson.RootElement.GetProperty("status").GetProperty("state").GetString();
            if (state == "SUCCEEDED")
            {
                var resultUrl = $"{statusUrl}/result";
                using var resultResp = await _http.GetAsync(resultUrl, cancellationToken);
                resultResp.EnsureSuccessStatusCode();
                using var rr = await resultResp.Content.ReadAsStreamAsync(cancellationToken);
                using var resultJson = await JsonDocument.ParseAsync(rr, cancellationToken: cancellationToken);
                return resultJson.RootElement.Clone();
            }
            if (state == "FAILED" || state == "CANCELED")
            {
                var err = statusJson.RootElement.GetProperty("status").GetProperty("error_message").GetString();
                throw new Exception($"Statement {statementId} ended with state={state}: {err}");
            }

            await Task.Delay(1000, cancellationToken);
        }

        throw new OperationCanceledException("Polling cancelled");
    }
}
