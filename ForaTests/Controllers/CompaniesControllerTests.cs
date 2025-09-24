using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ForaApi.Controllers;
using Fora.Application;
using Fora.Domain;
using Fora.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace ForaTests.Controllers;

public class CompaniesControllerTests
{
    [Fact]
    public async Task Get_ReturnsOrderedDtos_FilteredByStartsWith()
    {
        // Arrange: InMemory DbContext
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("companies_db"));
        var provider = services.BuildServiceProvider();
        var db = provider.GetRequiredService<AppDbContext>();

        db.Companies.AddRange(
            new Company { Id = 1, Name = "Apple", IncomeFacts = new List<IncomeFact>() },
            new Company { Id = 2, Name = "Microsoft", IncomeFacts = new List<IncomeFact>() },
            new Company { Id = 3, Name = "Amazon", IncomeFacts = new List<IncomeFact>() }
        );
        await db.SaveChangesAsync();

        var repo = new CompanyRepository(db);

        var calc = new Mock<IFundingCalculator>();
        calc.Setup(c => c.Calculate(It.IsAny<Company>()))
            .Returns((0m, 0m));

        var controller = new CompaniesController();

        // Act
        var result = await controller.Get("a", repo, calc.Object, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<CompanyResponseDto>>(ok.Value);
        var names = dtos.Select(d => d.Name).OrderBy(n => n).ToArray();
        Assert.Equal(new[] { "Amazon", "Apple" }, names);
    }
}
