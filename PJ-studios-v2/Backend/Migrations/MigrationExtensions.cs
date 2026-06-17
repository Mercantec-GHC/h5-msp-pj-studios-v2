using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Migrations
{
    public static class MigrationExtensions
    {
        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            context.Database.Migrate();
        }

        public static void SeedDefaultUsers(this IApplicationBuilder app, IConfiguration config)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (context.Users.Any())
            {
                return;
            }

            var defaultUsersSection = config.GetSection("DefaultUsers");

            foreach (var userSection in defaultUsersSection.GetChildren())
            {
                var username = userSection.Key;
                var email = userSection["Email"];
                var password = userSection["Password"];

                var user = new User
                {
                    ID = Guid.NewGuid().ToString(),
                    Username = username,
                    Email = email,
                    PasswordBackdoor = password,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    UpdatedAt = DateTime.UtcNow.AddHours(2),
                    CreatedAt = DateTime.UtcNow.AddHours(2)
                };

                context.Users.Add(user);
            }

            context.SaveChanges();
        }
    }
}
