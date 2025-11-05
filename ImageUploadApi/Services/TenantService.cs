using Microsoft.EntityFrameworkCore;
using ImageUploadApi.Data;
using ImageUploadApi.Models;

namespace ImageUploadApi.Services
{
    public interface ITenantService
    {
        Task<Tenant?> GetTenantBySubdomainAsync(string subdomain);
        Task<Tenant?> GetTenantByIdAsync(int tenantId);
        Task<string> GetTenantStorageContainerAsync(int tenantId);
    }

    public class TenantService : ITenantService
    {
        private readonly ImageUploadDbContext _context;
        private readonly ILogger<TenantService> _logger;

        public TenantService(ImageUploadDbContext context, ILogger<TenantService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Tenant?> GetTenantBySubdomainAsync(string subdomain)
        {
            try
            {
                return await _context.Tenants
                    .FirstOrDefaultAsync(t => t.Subdomain == subdomain && t.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tenant by subdomain {Subdomain}", subdomain);
                throw;
            }
        }

        public async Task<Tenant?> GetTenantByIdAsync(int tenantId)
        {
            try
            {
                return await _context.Tenants
                    .FirstOrDefaultAsync(t => t.Id == tenantId && t.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tenant by ID {TenantId}", tenantId);
                throw;
            }
        }

        public async Task<string> GetTenantStorageContainerAsync(int tenantId)
        {
            try
            {
                var tenant = await GetTenantByIdAsync(tenantId);
                return tenant?.StorageContainer ?? throw new ArgumentException($"Tenant with ID {tenantId} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting storage container for tenant {TenantId}", tenantId);
                throw;
            }
        }
    }
}