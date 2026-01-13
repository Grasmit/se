using DotNet.DtoVsDomain.Api.Contracts.Responses;
using DotNet.DtoVsDomain.Api.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotNet.DtoVsDomain.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly Persistence.Repositories.ICustomerRepository _repo;

    public CustomersController(Persistence.Repositories.ICustomerRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomerResponse>>> GetAsync(CancellationToken cancellationToken)
    {
        var customers = await _repo.GetAllAsync(cancellationToken);
        var responses = customers.Select(c => new CustomerResponse { Id = c.CustomerId, Name = c.FullName, Email = c.ContactEmail }).ToList();
        return Ok(responses);
    }
}
