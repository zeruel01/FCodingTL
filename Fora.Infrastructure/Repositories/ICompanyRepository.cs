using Fora.Domain;

namespace Fora.Infrastructure;

public interface ICompanyRepository
{
    Task<Company> UpsertCompanyAsync(string cik, string name, CancellationToken ct);
    Task UpsertIncomeAsync(int companyId, int year, decimal valueUsd, CancellationToken ct);
    IQueryable<Company> QueryCompanies();
    Task<int> SaveChangesAsync(CancellationToken ct);
}
