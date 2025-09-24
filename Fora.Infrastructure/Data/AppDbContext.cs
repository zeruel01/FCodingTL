using Fora.Domain;
using Microsoft.EntityFrameworkCore;

namespace Fora.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<IncomeFact> IncomeFacts => Set<IncomeFact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Cik).IsRequired().HasMaxLength(10);
            b.HasIndex(x => x.Cik).IsUnique();
            b.Property(x => x.Name).IsRequired();
        });
        modelBuilder.Entity<IncomeFact>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.ValueUsd).HasColumnType("decimal(18,2)");
            b.HasIndex(x => new { x.CompanyId, x.Year }).IsUnique();
            b.HasOne(x => x.Company)
                .WithMany(c => c.IncomeFacts)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
