using System.Threading;
using System.Threading.Tasks;
using ForaApi.Controllers;
using Fora.Application;
using Fora.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.Extensions.Configuration;

namespace ForaTests.Controllers;

public class AdminControllerTests
{
    [Fact]
    public async Task Import_CallsImporter_AndReturnsCount()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("admin_import_db"));
        var provider = services.BuildServiceProvider();
        var db = provider.GetRequiredService<AppDbContext>();

        var importer = new Mock<IImporter>();
        importer.Setup(i => i.ImportAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        var controller = new AdminController(new ConfigurationBuilder().Build(), importer.Object, db);

        var result = await controller.Import(null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        dynamic val = ok.Value!;
        Assert.True(val.imported > 0);
        importer.Verify(i => i.ImportAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Once);
    }


}
