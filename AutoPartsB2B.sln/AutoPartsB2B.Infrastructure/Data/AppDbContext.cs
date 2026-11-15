using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPartsB2B.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsB2B.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // SQLite uyumluluğu için ondalıklı sayıları (decimal) double'a çeviriyoruz
            modelBuilder.Entity<Product>().Property(p => p.Price).HasConversion<double>();
            modelBuilder.Entity<Order>().Property(o => o.TotalAmount).HasConversion<double>();
            modelBuilder.Entity<OrderItem>().Property(oi => oi.UnitPrice).HasConversion<double>();

            // Test kullanıcısına zorunlu olan kullanıcı adı ve şifre bilgileri
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                FullName = "Kayseri Oto Yedek Parça - Merkez",
                Role = "Dealer",
                Username = "kayseri",
                Password = "1234"
            });

            // Test ürünleri 
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Toyota Corolla 1.6 Yağ Filtresi", Category = "Filtreler", StockQuantity = 500, Price = 180.50m },
                new Product { Id = 2, Name = "Hyundai Bayon 1.4 MPI Hava Filtresi", Category = "Filtreler", StockQuantity = 350, Price = 245.00m },
                new Product { Id = 3, Name = "Castrol Edge 5W-30 Motor Yağı 4L", Category = "Motor Yağları", StockQuantity = 200, Price = 1850.00m },
                new Product { Id = 4, Name = "Varta 72Ah Start-Stop Akü (AGM)", Category = "Elektrik", StockQuantity = 30, Price = 4200.00m }
            );
        }
    }
}