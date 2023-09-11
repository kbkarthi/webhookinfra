using Microsoft.EntityFrameworkCore;
using SampleWebApiAspNetCore.Entities;

namespace SampleWebApiAspNetCore.Repositories
{
    public class WebhookDbContext : DbContext
    {
        public WebhookDbContext(DbContextOptions<WebhookDbContext> options)
            : base(options)
        {
        }

        public DbSet<WebhookEntity> WebhookItems { get; set; } = null!;
    }
}
