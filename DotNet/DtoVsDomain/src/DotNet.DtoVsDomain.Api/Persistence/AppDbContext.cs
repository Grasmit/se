using DotNet.DtoVsDomain.Api.Domain.Entities;
using DotNet.DtoVsDomain.Api.Services.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace DotNet.DtoVsDomain.Api.Persistence;

public class AppDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider) : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().HasQueryFilter(x => string.IsNullOrEmpty(_tenantProvider.CurrentTenant) || x.TenantId == _tenantProvider.CurrentTenant);
        modelBuilder.Entity<Product>().HasQueryFilter(x => string.IsNullOrEmpty(_tenantProvider.CurrentTenant) || x.TenantId == _tenantProvider.CurrentTenant);
        modelBuilder.Entity<Order>().HasQueryFilter(x => string.IsNullOrEmpty(_tenantProvider.CurrentTenant) || x.TenantId == _tenantProvider.CurrentTenant);
        modelBuilder.Entity<OrderItem>().HasQueryFilter(x => string.IsNullOrEmpty(_tenantProvider.CurrentTenant) || x.TenantId == _tenantProvider.CurrentTenant);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderItem>()
            .HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderItem>()
            .Property(p => p.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Product>()
            .Property(p => p.UnitPrice)
            .HasPrecision(18, 2);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var ambientTenant = _tenantProvider.CurrentTenant;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                var tenantToApply = string.IsNullOrWhiteSpace(ambientTenant)
                    ? entry.Entity.TenantId
                    : ambientTenant;

                if (string.IsNullOrWhiteSpace(tenantToApply))
                {
                    throw new InvalidOperationException("Tenant context is required when creating entities.");
                }

                entry.Entity.TenantId = tenantToApply;
                entry.Entity.CreatedAtUtc = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
