using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetCareConnect.Models;

namespace PetCareConnect.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        });
    }

    public DbSet<Product> Products 
    { 
        get; 
        set; 
    }

    public DbSet<Pet> Pets 
    { 
        get; 
        set;
    }

    public DbSet<Notification> Notifications 
    { 
        get; 
        set;
    }

    public DbSet<Order> Orders 
    { 
        get; 
        set;
    }

    public DbSet<Appointment> Appointments 
    { 
        get; 
        set;
    }

    public DbSet<Prescription> Prescriptions 
    { 
        get; 
        set;
    }

    public DbSet<OrderItem> OrderItems 
    { 
        get; 
        set;
    }
}