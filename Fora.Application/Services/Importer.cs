using Fora.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Fora.Application;

public class Importer(IEdgarClient client, ICompanyRepository repo, ILogger<Importer> logger) : IImporter
{
    public async Task ImportAsync(IEnumerable<int> ciks, CancellationToken ct)
    {
        foreach (var cikNum in ciks)
        {
            var cik10 = cikNum.ToString("D10");
            EdgarCompanyInfo? data = null;
            try
            {
                data = await client.GetCompanyFactsAsync(cik10, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching CIK {CIK}", cik10);
            }
            if (data is null) continue;

            var name = NormalizeWhitespace(data.EntityName);
            var company = await repo.UpsertCompanyAsync(cik10, name, ct);

            var usd = data.Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd;
            if (usd is null) continue;

            var byYear = new Dictionary<int, EdgarCompanyInfo.InfoFactUsGaapIncomeLossUnitsUsd>();
            foreach (var item in usd)
            {
                if (!string.Equals(item.Form, "10-K", StringComparison.Ordinal)) continue;
                if (string.IsNullOrWhiteSpace(item.Frame)) continue;
                if (!item.Frame.StartsWith("CY")) continue;
                if (!int.TryParse(item.Frame.AsSpan(2), out int year)) continue;
                if (year < 2018 || year > 2022) continue;
                byYear[year] = item;
            }

            foreach (var kv in byYear)
            {
                await repo.UpsertIncomeAsync(company.Id, kv.Key, kv.Value.Val, ct);
            }
        }
    }

    private static string NormalizeWhitespace(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        return string.Join(' ', s.Split((char[])null!, StringSplitOptions.RemoveEmptyEntries));
    }
}
