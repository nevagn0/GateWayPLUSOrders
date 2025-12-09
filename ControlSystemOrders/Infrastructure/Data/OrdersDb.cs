using Microsoft.EntityFrameworkCore;
using ControlSystemOrders.Domain.Model;
using System;

namespace ControlSystemOrders.Infrastructure.Data;

public class OrdersDb : DbContext
{
    public OrdersDb(DbContextOptions<OrdersDb> options) : base(options)
    {
        
    }
    
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("Id")
                .HasColumnType("uuid")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("uuid_generate_v4()");

            entity.Property(e => e.UserId)
                .HasColumnName("UserId")
                .HasColumnType("uuid")
                .IsRequired();
            
            entity.Property(e => e.Status)
                .HasColumnName("Status")
                .HasMaxLength(140);
            
            entity.Property(e => e.Price)
                .HasColumnName("Price")
                .HasColumnType("decimal(18,2)");
            
            entity.Property(e => e.DateCreate)
                .HasColumnName("DateCreate")
                .HasDefaultValueSql("now()");
            
            entity.Property(e => e.DateUpdate)
                .HasColumnName("DateUpdate")
                .HasDefaultValueSql("now()");

            entity.OwnsOne(e => e.CountAndProduct, product =>
            {
                product.Property(p => p.Count)
                    .HasColumnName("ProductCount")
                    .HasDefaultValue(0);
                    
                product.Property(p => p.Name)
                    .HasColumnName("ProductName")
                    .HasMaxLength(255)
                    .HasDefaultValue(string.Empty);
            });
        });
    }
}