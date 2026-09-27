using Microsoft.EntityFrameworkCore;
using SenaViva.Shared.Entities;

namespace SenaViva.API.Data;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<ConversationSession> ConversationSessions => Set<ConversationSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }
}
