namespace Fora.Domain;

public class Company
{
    public int Id { get; set; }
    public string Cik { get; set; } = null!; // 10-char string
    public string Name { get; set; } = null!;
    public ICollection<IncomeFact> IncomeFacts { get; set; } = new List<IncomeFact>();
}
