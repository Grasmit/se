using DotNet.DtoVsDomain.Api.Contracts.Common;
using DotNet.DtoVsDomain.Api.Contracts.Requests;
using DotNet.DtoVsDomain.Api.Contracts.Responses;
using DotNet.DtoVsDomain.Api.Mappings;
using DotNet.DtoVsDomain.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNet.DtoVsDomain.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderResponse>>> GetAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var (orders, total) = await _orderService.GetAsync(page, pageSize, cancellationToken);
        var payload = new PagedResult<OrderResponse>
        {
            Items = orders.Select(o => o.ToResponse()).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        };

        return Ok(payload);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        return Ok(order.ToResponse());
    }

    // NOTE: Model validation is performed automatically (FluentValidation + [ApiController]).
    // If the incoming DTO is invalid, the framework will return 400 before this action runs.
    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateAsync([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderService.CreateAsync(request, cancellationToken);
            var response = order.ToResponse();
            return CreatedAtAction(nameof(GetByIdAsync), new { id = order.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
