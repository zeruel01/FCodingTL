namespace Fora.Domain;

public interface IFundingCalculator
{
    (decimal standard, decimal special) Calculate(Company company);
}
