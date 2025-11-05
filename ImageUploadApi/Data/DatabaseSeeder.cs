using Microsoft.EntityFrameworkCore;
using ImageUploadApi.Data;
using ImageUploadApi.Models;
using ImageUploadApi.Services;

namespace ImageUploadApi.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedDevelopmentDataAsync(ImageUploadDbContext context, IAuthService authService)
        {
            // Check if data already exists
            if (await context.Tenants.AnyAsync())
                return;

            // Create sample tenants
            var tenant1 = new Tenant
            {
                Name = "Acme Corporation",
                Subdomain = "acme",
                StorageContainer = "acme-images",
                IsActive = true
            };

            var tenant2 = new Tenant
            {
                Name = "Tech Startup Inc",
                Subdomain = "techstartup",
                StorageContainer = "techstartup-images",
                IsActive = true
            };

            context.Tenants.AddRange(tenant1, tenant2);
            await context.SaveChangesAsync();

            // Create sample users
            var user1 = new User
            {
                Username = "john.doe",
                Email = "john.doe@acme.com",
                PasswordHash = authService.HashPassword("password123"),
                TenantId = tenant1.Id,
                IsActive = true
            };

            var user2 = new User
            {
                Username = "jane.smith",
                Email = "jane.smith@acme.com",
                PasswordHash = authService.HashPassword("password123"),
                TenantId = tenant1.Id,
                IsActive = true
            };

            var user3 = new User
            {
                Username = "bob.wilson",
                Email = "bob.wilson@techstartup.com",
                PasswordHash = authService.HashPassword("password123"),
                TenantId = tenant2.Id,
                IsActive = true
            };

            context.Users.AddRange(user1, user2, user3);
            await context.SaveChangesAsync();
        }
    }
}