using Microsoft.EntityFrameworkCore;

namespace Messaging.Worker.Data;

public class MessagingDbContext : DbContext
{
    public MessagingDbContext(
        DbContextOptions<MessagingDbContext> options)
        : base(options)
    {
    }

    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProcessedMessage> (entity =>
        {
            entity.HasKey(x => x.MessageId);
            entity.Property(x => x.MessageId).IsRequired();
            entity.Property(x => x.ProcessedAt).IsRequired();
        });
    }
}