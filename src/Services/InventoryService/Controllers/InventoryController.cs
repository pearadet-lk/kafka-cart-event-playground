using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers;

[ApiController]
[Route("api/inventory")]
public sealed class InventoryController : ControllerBase
{
    [HttpPost("reserve")]
    public IActionResult Reserve([FromBody] object request)
    {
        return Ok(new { Status = "Reserved", Request = request });
    }
}
