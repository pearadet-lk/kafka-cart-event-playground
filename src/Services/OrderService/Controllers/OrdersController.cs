using Microsoft.AspNetCore.Mvc;

namespace OrderService.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    [HttpPost("create")]
    public IActionResult Create([FromBody] object request)
    {
        return Ok(new { Status = "Created", Request = request });
    }
}
