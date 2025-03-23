using _200SXContact.Models.Areas.Admin;
using _200SXContact.Models.Areas.Chat;
using _200SXContact.Models.Areas.MaintenApp;
using _200SXContact.Models.Areas.Newsletter;
using _200SXContact.Models.Areas.Orders;
using _200SXContact.Models.Areas.Products;
using _200SXContact.Models.Areas.UserContent;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace _200SXContact.Interfaces.Areas.Data
{
    public interface IApplicationDbContext
    {
        DbSet<EmailLog> EmailLogs { get; }
        DbSet<Logging> Logging { get; }
        DbSet<User> Users { get; }
        DbSet<ReminderItem> Items { get; }
        DbSet<UserBuild> UserBuilds { get; }
        DbSet<NewsletterSubscription> NewsletterSubscriptions { get; }
        DbSet<BuildComment> BuildComments { get; }
        DbSet<Product> Products { get; }
        DbSet<CartItem> CartItems { get; }
        DbSet<OrderPlacement> Orders { get; }
        DbSet<OrderTracking> OrderTrackings { get; }
        DbSet<ChatMessage> ChatMessages { get; }
        DbSet<ChatSession> ChatSessions { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        DatabaseFacade Database { get; }
        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    }
}
