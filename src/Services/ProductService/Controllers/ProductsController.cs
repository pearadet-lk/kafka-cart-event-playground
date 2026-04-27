using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(ProductDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await db.Products.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return Ok(product);
    }
}
