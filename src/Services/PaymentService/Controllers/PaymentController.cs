using Microsoft.AspNetCore.Mvc;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/payment")]
public sealed class PaymentController : ControllerBase
{
    [HttpPost("process")]
    public IActionResult Process([FromBody] object request)
    {
        return Ok(new { Status = "Processed", Request = request });
    }
}
