using System.Threading;
using System.Threading.Tasks;
using Fora.Application;
using Fora.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;

namespace ForaTests.Services;

public class ImporterTests
{
    [Fact]
    public async Task ImportAsync_UpsertsCompany_AndIncome_For10K_CYFrames()
    {
        // Arrange
        var sample = new EdgarCompanyInfo
        {
            Cik = 1234567890,
            EntityName = "  Test   Corp  ",
            Facts = new EdgarCompanyInfo.InfoFact
            {
                UsGaap = new EdgarCompanyInfo.InfoFactUsGaap
                {
                    NetIncomeLoss = new EdgarCompanyInfo.InfoFactUsGaapNetIncomeLoss
                    {
                        Units = new EdgarCompanyInfo.InfoFactUsGaapIncomeLossUnits
                        {
                            Usd = new[]
                            {
                                new EdgarCompanyInfo.InfoFactUsGaapIncomeLossUnitsUsd { Form = "10-K", Frame = "CY2020", Val = 100m },
                                new EdgarCompanyInfo.InfoFactUsGaapIncomeLossUnitsUsd { Form = "10-K", Frame = "CY2019", Val = 200m },
                                new EdgarCompanyInfo.InfoFactUsGaapIncomeLossUnitsUsd { Form = "10-Q", Frame = "CY2020", Val = 300m },
                                new EdgarCompanyInfo.InfoFactUsGaapIncomeLossUnitsUsd { Form = "10-K", Frame = "FY2020", Val = 400m },
                                new EdgarCompanyInfo.InfoFactUsGaapIncomeLossUnitsUsd { Form = "10-K", Frame = null, Val = 500m },
                            }
                        }
                    }
                }
            }
        };

        var repo = new Mock<ICompanyRepository>(MockBehavior.Strict);
        var client = new Mock<IEdgarClient>(MockBehavior.Strict);
        var logger = new Mock<ILogger<Importer>>();

        client.Setup(c => c.GetCompanyFactsAsync("0000000123", It.IsAny<CancellationToken>()))
              .ReturnsAsync(sample);

        repo.Setup(r => r.UpsertCompanyAsync("0000000123", "Test Corp", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Fora.Domain.Company { Id = 1, Name = "Test Corp" });

        repo.Setup(r => r.UpsertIncomeAsync(1, 2020, 100m, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        repo.Setup(r => r.UpsertIncomeAsync(1, 2019, 200m, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = new Importer(client.Object, repo.Object, logger.Object);

        // Act
        await sut.ImportAsync(new[] { 123 }, CancellationToken.None);

        // Assert
        client.VerifyAll();
        repo.VerifyAll();
    }

    [Fact]
    public async Task ImportAsync_Skips_When_ApiThrows()
    {
        var repo = new Mock<ICompanyRepository>(MockBehavior.Strict);
        var client = new Mock<IEdgarClient>(MockBehavior.Strict);
        var logger = new Mock<ILogger<Importer>>();

        client.Setup(c => c.GetCompanyFactsAsync("0000000123", It.IsAny<CancellationToken>()))
              .ThrowsAsync(new HttpRequestException("boom"));

        var sut = new Importer(client.Object, repo.Object, logger.Object);

        await sut.ImportAsync(new[] { 123 }, CancellationToken.None);

        // No repo interactions
        repo.VerifyNoOtherCalls();
    }
}
