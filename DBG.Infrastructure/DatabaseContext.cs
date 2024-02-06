#region

using DBG.Infrastructure.Models.Db;
using Microsoft.EntityFrameworkCore;

#endregion

namespace DBG.Infrastructure;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<SystemEntry> SystemEntries { get; set; } = null!;
    public DbSet<OsSystemEntry> OsSystemEntries { get; set; } = null!;
    public DbSet<DbSystemEntry> DbSystemEntries { get; set; } = null!;
    public DbSet<DbStaticState> DbStaticStates { get; set; } = null!;
    public DbSet<DbDynamicState> DbDynamicStates { get; set; } = null!;
    public DbSet<OsStaticState> OsStaticStates { get; set; } = null!;
    public DbSet<OsDynamicState> OsDynamicStates { get; set; } = null!;
}