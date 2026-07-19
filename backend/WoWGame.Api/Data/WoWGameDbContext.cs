using Microsoft.EntityFrameworkCore;
using WoWGame.Api.Data.Entities;

namespace WoWGame.Api.Data;

public class WoWGameDbContext : DbContext
{
    public WoWGameDbContext(DbContextOptions<WoWGameDbContext> options) : base(options)
    {
    }

    public DbSet<Level> Levels => Set<Level>();
    public DbSet<WordMeaning> WordMeanings => Set<WordMeaning>();
    public DbSet<Player> Players => Set<Player>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Level>()
            .HasIndex(l => l.LevelNumber)
            .IsUnique();

        modelBuilder.Entity<Player>()
            .Property(p => p.Username)
            .HasDefaultValue("Player 1");
    }
}
