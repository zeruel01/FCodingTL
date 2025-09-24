using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Fora.Application;

public class EdgarClient(HttpClient http, ILogger<EdgarClient> logger, JsonSerializerOptions jsonOptions) : IEdgarClient
{
    public async Task<EdgarCompanyInfo?> GetCompanyFactsAsync(string cik10, CancellationToken ct)
    {
        var url = $"companyfacts/CIK{cik10}.json";
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        using var resp = await http.SendAsync(req, ct);
        if (!resp.IsSuccessStatusCode)
        {
            logger.LogWarning("EDGAR request for {CIK} failed with {Status}", cik10, resp.StatusCode);
            return null;
        }
        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        return await System.Text.Json.JsonSerializer.DeserializeAsync<EdgarCompanyInfo>(stream, jsonOptions, ct);
    }
}
