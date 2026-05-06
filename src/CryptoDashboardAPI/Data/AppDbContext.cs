using CryptoDashboardAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoDashboardAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Cryptocurrency> Cryptocurrencies { get; set; }
    public DbSet<WatchlistItem> WatchlistItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<Cryptocurrency>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.ExternalProviderId).IsRequired().HasMaxLength(100);
            entity.HasIndex(c => c.ExternalProviderId).IsUnique();
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Symbol).IsRequired().HasMaxLength(20);
            entity.Property(c => c.CurrentPrice).HasPrecision(28, 8);
            entity.Property(c => c.MarketCap).HasPrecision(28, 2);
            entity.Property(c => c.PriceChangePercentage).HasPrecision(10, 4);
        });

        modelBuilder.Entity<WatchlistItem>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.HasIndex(w => new { w.UserId, w.CryptocurrencyId }).IsUnique();

            entity.HasOne(w => w.User)
                .WithMany(u => u.WatchlistItems)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(w => w.Cryptocurrency)
                .WithMany(c => c.WatchlistItems)
                .HasForeignKey(w => w.CryptocurrencyId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            foreach (var property in entry.Properties)
            {
                if (property.CurrentValue is DateTime dt && dt.Kind == DateTimeKind.Unspecified)
                    property.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
