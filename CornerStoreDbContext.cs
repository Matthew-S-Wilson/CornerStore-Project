using Microsoft.EntityFrameworkCore;
using CornerStore.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Net.Http.Headers;

public class CornerStoreDbContext : DbContext
{
        public DbSet<Cashier> Cashiers { get; set; }
        public DbSet<Category> Categories {get; set;}
        public DbSet<Order> Orders {get; set;}
        public DbSet<OrderProduct> OrderProducts {get; set;}
        public DbSet<Product> Products {get; set;}
    public CornerStoreDbContext(DbContextOptions<CornerStoreDbContext> context) : base(context)
    {

    }

    //allows us to configure the schema when migrating as well as seed data
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cashier>().HasData(new Cashier[]
        {
        new Cashier {Id = 1, FirstName = "Jim", LastName = "Bob" },
        new Cashier {Id = 2, FirstName = "Steve", LastName = "Texas" }
        });
        modelBuilder.Entity<Category>().HasData(new Category[]
        {
        new Category {Id = 1, CategoryName = "Dairy" },
        new Category {Id = 2, CategoryName = "Bread" },
        new Category {Id = 3, CategoryName = "Produce" },
        });
        modelBuilder.Entity<Order>().HasData(new Order[]
        {
        new Order {Id = 1, CashierId = 2, PaidOnDate = null },
        new Order {Id = 2, CashierId = 1, PaidOnDate = null },
        new Order {Id = 3, CashierId = 2, PaidOnDate = new DateTime(2023,09,29) },
        });
        modelBuilder.Entity<OrderProduct>().HasData(new OrderProduct[]
        {
        new OrderProduct {Id = 1,ProductId = 1, OrderId = 3, Quantity = 111 },
        new OrderProduct {Id = 2,ProductId = 2, OrderId = 2, Quantity = 55 },
        new OrderProduct {Id = 3,ProductId = 3, OrderId = 1, Quantity = 22 },
        });
                modelBuilder.Entity<Product>().HasData(new Product[]
        {
        new Product {Id = 1, ProductName = "Sourdough Bread", Brand = "Wheaties", CategoryId = 2 },
        new Product {Id = 2, ProductName = "Almond Milk", Brand = "Silk", CategoryId = 1},
        new Product {Id = 3, ProductName = "Pork Tenderloin", Brand = "Butchers", CategoryId = 3},
        });
    }
}