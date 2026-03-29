using Microsoft.EntityFrameworkCore;
using short_term_stay_project.Models;

namespace short_term_stay_project.Data;

public class ShortTermStayDbContext : DbContext
{
    public ShortTermStayDbContext(DbContextOptions<ShortTermStayDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User relations
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Username).IsUnique();
        });

        // Listing relations
        modelBuilder.Entity<Listing>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.HasOne(e => e.Host)
                .WithMany(u => u.Listings)
                .HasForeignKey(e => e.HostId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Booking relations
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Listing)
                .WithMany(l => l.Bookings)
                .HasForeignKey(e => e.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Guest)
                .WithMany(u => u.Bookings)
                .HasForeignKey(e => e.GuestId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Review relations
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Booking)
                .WithOne(b => b.Review)
                .HasForeignKey<Review>(e => e.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Guest)
                .WithMany()
                .HasForeignKey(e => e.GuestId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Listing)
                .WithMany(l => l.Reviews)
                .HasForeignKey(e => e.ListingId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
