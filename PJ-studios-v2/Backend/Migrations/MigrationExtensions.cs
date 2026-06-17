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

            context.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""Users"" (
                    ""ID"" text PRIMARY KEY,
                    ""Username"" text NOT NULL,
                    ""Email"" text NOT NULL,
                    ""PasswordBackdoor"" text NOT NULL,
                    ""PasswordHash"" text NOT NULL,
                    ""LoginAttempts"" integer NOT NULL DEFAULT 0,
                    ""LastFailedLogin"" timestamp with time zone NULL,
                    ""IsLocked"" boolean NOT NULL DEFAULT false,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedAt"" timestamp with time zone NOT NULL
                );

                CREATE TABLE IF NOT EXISTS ""Items"" (
                    ""Id"" text PRIMARY KEY,
                    ""UserId"" text NOT NULL DEFAULT '',
                    ""Name"" text NOT NULL,
                    ""Description"" text NOT NULL,
                    ""ImageUrl"" text NOT NULL DEFAULT ''
                );

                CREATE TABLE IF NOT EXISTS ""Ratings"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""ItemId"" text NOT NULL,
                    ""UserId"" text NOT NULL,
                    ""Score"" numeric(4,1) NOT NULL CHECK (""Score"" >= 1 AND ""Score"" <= 10)
                );

                CREATE TABLE IF NOT EXISTS ""RefreshTokens"" (
                    ""Id"" text PRIMARY KEY,
                    ""Token"" text NOT NULL,
                    ""UserId"" text NOT NULL,
                    ""ExpiryDate"" timestamp with time zone NOT NULL,
                    ""IsRevoked"" boolean NOT NULL DEFAULT false
                );

                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM information_schema.table_constraints
                        WHERE table_name = 'RefreshTokens'
                        AND constraint_name = 'FK_RefreshTokens_Users_UserId'
                    ) THEN
                        ALTER TABLE ""RefreshTokens""
                        ADD CONSTRAINT ""FK_RefreshTokens_Users_UserId""
                        FOREIGN KEY (""UserId"") REFERENCES ""Users""(""ID"") ON DELETE CASCADE;
                    END IF;
                END $$;
            ");
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
