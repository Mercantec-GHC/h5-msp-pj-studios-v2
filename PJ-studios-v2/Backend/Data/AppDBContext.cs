using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<ItemModel> Items { get; set; }
    public DbSet<RatingsModel> Ratings { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ItemModel>()
    .HasOne(i => i.User)
    .WithMany()
    .HasForeignKey(i => i.UserId)
    .HasPrincipalKey(u => u.ID);
    }
}