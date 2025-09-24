using Fora.Application;
using Microsoft.AspNetCore.Mvc;
using Fora.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ForaApi.Controllers;

[ApiController]
[Route("admin")]
public class AdminController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IImporter _importer;
    private readonly AppDbContext _db;

    public AdminController(IConfiguration configuration, IImporter importer, AppDbContext db)
    {
        _configuration = configuration;
        _importer = importer;
        _db = db;
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import([FromHeader(Name = "x-api-key")] string? apiKey, CancellationToken ct)
    {
        //var expected = _configuration.GetValue<string>("Admin:ApiKey") ?? "dev-key";
        //if (apiKey != expected)
        //{
        //    return Unauthorized();
        //}

        await _importer.ImportAsync(CikSeed.All, ct);
        return Ok(new { imported = CikSeed.All.Length });
    }

    [HttpPost("reset")]
    public async Task<IActionResult> Reset([FromHeader(Name = "x-api-key")] string? apiKey, [FromQuery] bool import = false, CancellationToken ct = default)
    {
        //var expected = _configuration.GetValue<string>("Admin:ApiKey") ?? "dev-key";
        //if (apiKey != expected)
        //{
        //    return Unauthorized();
        //}

        // Drop and recreate the SQLite database
        await _db.Database.EnsureDeletedAsync(ct);
        await _db.Database.MigrateAsync(ct);

        int imported = 0;
        if (import)
        {
            await _importer.ImportAsync(CikSeed.All, ct);
            imported = CikSeed.All.Length;
        }

        return Ok(new { reset = true, imported });
    }
}
