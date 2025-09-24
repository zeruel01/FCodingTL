namespace Fora.Domain;

public class FundingCalculator : IFundingCalculator
{
    public (decimal standard, decimal special) Calculate(Company company)
    {
        var years = company.IncomeFacts.ToDictionary(f => f.Year, f => f.ValueUsd);
        for (int y = 2018; y <= 2022; y++)
        {
            if (!years.ContainsKey(y)) return (0m, 0m);
        }
        if (years[2021] <= 0 || years[2022] <= 0) return (0m, 0m);

        var maxIncome = years.Values.Max();
        decimal standard = maxIncome >= 10_000_000_000m
            ? 0.1233m * maxIncome
            : 0.2151m * maxIncome;

        decimal special = standard;
        var name = company.Name?.TrimStart();
        if (!string.IsNullOrEmpty(name) && "AEIOUaeiou".IndexOf(name[0]) >= 0)
        {
            special += 0.15m * standard;
        }
        if (years[2022] < years[2021])
        {
            special -= 0.25m * standard;
        }

        standard = Math.Round(standard, 2, MidpointRounding.AwayFromZero);
        special = Math.Round(special, 2, MidpointRounding.AwayFromZero);
        return (standard, special);
    }
}
