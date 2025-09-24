namespace Fora.Application;

public interface IImporter
{
    Task ImportAsync(IEnumerable<int> ciks, CancellationToken ct);
}
