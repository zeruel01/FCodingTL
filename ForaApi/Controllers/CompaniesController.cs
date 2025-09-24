using Microsoft.AspNetCore.Mvc;
using Fora.Infrastructure;
using Fora.Domain;
using Microsoft.EntityFrameworkCore;
using Fora.Application;

namespace ForaApi.Controllers
{
    [ApiController]
    [Route("api/companies")]
    public class CompaniesController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? startsWith, [FromServices] ICompanyRepository repo, [FromServices] IFundingCalculator calc, CancellationToken ct)
        {
            var query = repo.QueryCompanies();
            if (!string.IsNullOrWhiteSpace(startsWith))
            {
                var s = startsWith[0].ToString().ToUpperInvariant();
                query = query.Where(c => c.Name != null && c.Name.ToUpper().StartsWith(s));
            }

            var list = await query
                .OrderBy(c => c.Name)
                .Select(c => new Company { Id = c.Id, Name = c.Name, IncomeFacts = c.IncomeFacts })
                .ToListAsync(ct);

            var result = list.Select(c => c.ToDto(calc));
            return Ok(result);
        }
    }
}
