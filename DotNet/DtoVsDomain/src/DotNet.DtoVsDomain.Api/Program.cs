using DotNet.DtoVsDomain.Api.Persistence;
using DotNet.DtoVsDomain.Api.Services;
using DotNet.DtoVsDomain.Api.Services.Tenancy;
using Microsoft.EntityFrameworkCore;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<DotNet.DtoVsDomain.Api.Validators.CreateOrderRequestValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ITenantProvider, HttpTenantProvider>();
builder.Services.AddScoped<Persistence.Repositories.IOrderRepository, Persistence.Repositories.OrderRepository>();
builder.Services.AddScoped<Persistence.Repositories.ICustomerRepository, Persistence.Repositories.CustomerRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

var connectionString = builder.Configuration.GetConnectionString("Sql")
    ?? "Server=localhost,1433;Database=DtoVsDomain;User Id=sa;Password=Your_password123;TrustServerCertificate=True;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    await DataSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
