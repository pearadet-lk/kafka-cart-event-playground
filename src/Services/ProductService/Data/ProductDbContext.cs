using BuildingBlocks.Outbox;
using Microsoft.EntityFrameworkCore;
using ProductService.Models;

namespace ProductService.Data;

public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
}
