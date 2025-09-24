using Fora.Domain;
using Microsoft.EntityFrameworkCore;

namespace Fora.Infrastructure;

public class CompanyRepository(AppDbContext db) : ICompanyRepository
{
    public async Task<Company> UpsertCompanyAsync(string cik, string name, CancellationToken ct)
    {
        var existing = await db.Companies.FirstOrDefaultAsync(c => c.Cik == cik, ct);
        if (existing is null)
        {
            existing = new Company { Cik = cik, Name = name.Trim() };
            db.Companies.Add(existing);
        }
        else
        {
            existing.Name = name.Trim();
        }
        await db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task UpsertIncomeAsync(int companyId, int year, decimal valueUsd, CancellationToken ct)
    {
        var existing = await db.IncomeFacts.FirstOrDefaultAsync(i => i.CompanyId == companyId && i.Year == year, ct);
        if (existing is null)
        {
            existing = new IncomeFact { CompanyId = companyId, Year = year, ValueUsd = valueUsd };
            db.IncomeFacts.Add(existing);
        }
        else
        {
            existing.ValueUsd = valueUsd;
        }
        await db.SaveChangesAsync(ct);
    }

    public IQueryable<Company> QueryCompanies() => db.Companies.AsNoTracking().Include(c => c.IncomeFacts);

    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
