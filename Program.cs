using CornerStore.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core and provides dummy value for testing
builder.Services.AddNpgsql<CornerStoreDbContext>(builder.Configuration["CornerStoreDbConnectionString"] ?? "testing");

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//endpoints go here

app.MapPost("/cashiers", (CornerStoreDbContext db, Cashier cashier) =>
{
    db.Cashiers.Add(cashier);
    db.SaveChanges();
    return Results.Created($"/cashiers/{cashier.Id}", cashier);
});

app.MapGet("/cashiers/{id}", (int id, CornerStoreDbContext db) =>
{
    var cashierWithOrders = db.Cashiers
        .Include(c => c.Orders)
        .ThenInclude(o => o.OrderProducts)
        .ThenInclude(op => op.Product)
        .FirstOrDefault(c => c.Id == id);
    return cashierWithOrders;
});

app.MapGet("/products", (CornerStoreDbContext db, string search) =>
{
    IQueryable<Product> query = db.Products.Include(p => p.Category);

    if (!string.IsNullOrEmpty(search))
    {
        string searchValue = search.ToLower();
        query = query.Where(p =>
            p.ProductName.ToLower().Contains(searchValue) ||
            p.Category.CategoryName.ToLower().Contains(searchValue)
        );
    }
    var products = query.ToList();

    return products;
});

//add a product
app.MapPost("/products", async (CornerStoreDbContext db, Product product) =>
{
    // Add the product to the database
    db.Products.Add(product);
    await db.SaveChangesAsync();

    // Return the newly created product with its generated Id
    return Results.Created($"/products/{product.Id}", product);
});

//update product
app.MapPut("/products/{id}", async (CornerStoreDbContext db, int id, Product updatedProduct) =>
{
    // Find the existing product by its ID
    var existingProduct = await db.Products.FindAsync(id);

    if (existingProduct == null)
    {
        return Results.NotFound(); // Product with the specified ID was not found.
    }

    // Update the properties of the existing product with the new values
    existingProduct.ProductName = updatedProduct.ProductName;
    existingProduct.Price = updatedProduct.Price;
    existingProduct.Brand = updatedProduct.Brand;
    existingProduct.CategoryId = updatedProduct.CategoryId;

    await db.SaveChangesAsync();

    return Results.NoContent(); 
});

app.MapGet("/orders/{id}", (int id, CornerStoreDbContext db) =>
{
    
    var orderDetails = db.Orders
        .Include(o => o.Cashier)
        .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
                .ThenInclude(p => p.Category)
        .FirstOrDefault(o => o.Id == id);

    if (orderDetails == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(orderDetails);
});

app.MapGet("/orders", (CornerStoreDbContext db, HttpContext context) =>
{
    // Get the value of the 'orderDate' query string parameter
    var orderDateQueryParam = context.Request.Query["orderDate"].ToString();
    // Initialize a DateTime? variable to hold the parsed orderDate value
    DateTime? parsedOrderDate = null;
    // Check if the 'orderDate' query parameter is provided and parse it
    if (!string.IsNullOrEmpty(orderDateQueryParam) && DateTime.TryParse(orderDateQueryParam, out var parsedDate))
    {
        parsedOrderDate = parsedDate;
    }
    var ordersQuery = db.Orders.AsQueryable();
    if (parsedOrderDate.HasValue)
    {
        // Filter orders by the parsed order date if it has a value
        ordersQuery = ordersQuery.Where(o => o.PaidOnDate != null && o.PaidOnDate.Value.Date == parsedOrderDate.Value.Date);
    }
    var orders = ordersQuery.ToList();
    return orders;
});

app.MapDelete("/orders/{id}", (int id, CornerStoreDbContext db) =>
{
   Order order = db.Orders.SingleOrDefault(o => o.Id == id);

    if (order == null)
    {
        return Results.NotFound(); 
    }
    db.Orders.Remove(order);
    db.SaveChanges();

    return Results.NoContent();
});

app.MapPost("/orders", (CornerStoreDbContext db, Order order) =>
{

    var cashier = db.Cashiers.FirstOrDefault(c => c.Id == order.CashierId);
    if (cashier == null)
    {
        return Results.BadRequest("Invalid CashierId");
    }

    if (order.OrderProducts == null || !order.OrderProducts.Any())
    {
        return Results.BadRequest("Order must have at least one product");
    }

    decimal total = 0;
    foreach (OrderProduct orderProduct in order.OrderProducts)
    {
        Product product = db.Products.FirstOrDefault(p => p.Id == orderProduct.ProductId);
        if (product == null)
        {
            return Results.BadRequest($"Invalid ProductId: {orderProduct.ProductId}");
        }

        total += product.Price * orderProduct.Quantity;
    }

    order.PaidOnDate = null;

    db.Orders.Add(order);
    db.SaveChanges();


    return Results.Created($"/orders/{order.Id}", order); 
});



app.Run();

//don't move or change this!
public partial class Program { }