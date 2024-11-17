using _200SXContact.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace _200SXContact.Data
{
	public class ApplicationDbContext : IdentityDbContext<User>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}
		public DbSet<EmailLog> EmailLogs { get; set; }
		//public new DbSet<User> Users { get; set; }
		public DbSet<LoggingModel> Logging { get; set; }
		public DbSet<Item> Items { get; set; }
		public DbSet<UserBuild> UserBuilds { get; set; }
		public DbSet<NewsletterSubscription> NewsletterSubscriptions { get; set; }
		public DbSet<BuildsCommentsModel> BuildComments { get; set; }
		public DbSet<Product> Products { get; set; }
		public DbSet<CartItem> CartItems { get; set; }
		public DbSet<Order> Orders { get; set; }
		public DbSet<OrderTracking> OrderTrackings { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Item>()
				.HasOne(i => i.User)
				.WithMany(u => u.Items)
				.HasForeignKey(i => i.UserId)
				.OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<Order>()
			    .HasMany(o => o.CartItems)
			    .WithOne(ci => ci.Order)
			    .HasForeignKey(ci => ci.OrderId)
			    .OnDelete(DeleteBehavior.Cascade);
		}
	}
}
