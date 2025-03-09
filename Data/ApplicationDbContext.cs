﻿using _200SXContact.Models.Areas.Admin;
using _200SXContact.Models.Areas.MaintenApp;
using _200SXContact.Models.Areas.Newsletter;
using _200SXContact.Models.Areas.Orders;
using _200SXContact.Models.Areas.Products;
using _200SXContact.Models.Areas.UserContent;
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
		public DbSet<Logging> Logging { get; set; }
		public DbSet<ReminderItem> Items { get; set; }
		public DbSet<UserBuild> UserBuilds { get; set; }
		public DbSet<NewsletterSubscription> NewsletterSubscriptions { get; set; }
		public DbSet<BuildComment> BuildComments { get; set; }
		public DbSet<Product> Products { get; set; }
		public DbSet<CartItem> CartItems { get; set; }
		public DbSet<OrderPlacement> Orders { get; set; }
		public DbSet<OrderTracking> OrderTrackings { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<ReminderItem>()
				.HasOne(i => i.User)
				.WithMany(u => u.Items)
				.HasForeignKey(i => i.UserId)
				.OnDelete(DeleteBehavior.Cascade);
			
            modelBuilder.Entity<OrderPlacement>()
				.HasOne(op => op.OrderTracking)
				.WithOne(ot => ot.Order)
				.HasForeignKey<OrderTracking>(ot => ot.OrderId);

            modelBuilder.Entity<BuildComment>()
			    .HasOne(bc => bc.UserBuild)
			    .WithMany(ub => ub.Comments)
			    .HasForeignKey(bc => bc.UserBuildId)
			    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserBuild>()
			    .HasMany(ub => ub.Comments)
			    .WithOne(bc => bc.UserBuild)
			    .HasForeignKey(bc => bc.UserBuildId)
			    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
				.HasMany(u => u.UserBuilds)
				.WithOne(ub => ub.User)
				.HasForeignKey(ub => ub.UserId)
				.OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
				.Property(p => p.Price)
				.HasColumnType("decimal(18,2)");
        }
	}
}
