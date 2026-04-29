using Backend.Models;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<ItemModel> Items { get; set; }
    public DbSet<RatingsModel> Ratings { get; set; }
}
