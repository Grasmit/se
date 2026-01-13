using DotNet.DtoVsDomain.Api.Contracts.Responses;
using DotNet.DtoVsDomain.Api.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotNet.DtoVsDomain.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ProductsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductResponse>>> GetAsync(CancellationToken cancellationToken)
    {
        var products = await _dbContext.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new ProductResponse { Id = p.Id, Name = p.Name, UnitPrice = p.UnitPrice })
            .ToListAsync(cancellationToken);

        return Ok(products);
    }
}
