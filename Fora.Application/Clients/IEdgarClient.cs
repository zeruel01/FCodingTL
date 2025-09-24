namespace Fora.Application;

public interface IEdgarClient
{
    Task<EdgarCompanyInfo?> GetCompanyFactsAsync(string cik10, CancellationToken ct);
}
