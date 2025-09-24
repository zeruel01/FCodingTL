using Fora.Domain;

namespace Fora.Application;

public static class Mapping
{
    public static CompanyResponseDto ToDto(this Company c, IFundingCalculator calc)
    {
        var (standard, special) = calc.Calculate(c);
        return new CompanyResponseDto(c.Id, c.Name, standard, special);
    }
}
