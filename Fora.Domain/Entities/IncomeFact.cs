namespace Fora.Domain;

public class IncomeFact
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public int Year { get; set; }
    public decimal ValueUsd { get; set; }
}
