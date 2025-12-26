using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Orders.Api.Consumers;
using Orders.Application.Abstractions.Repositories;
using Orders.Application.Orders.Commands.CreateOrder;
using Orders.Infrastructure.Persistence;
using Orders.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Database
// --------------------
builder.Services.AddDbContext<OrdersDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("OrdersDb"));
});

// --------------------
// Repositories
// --------------------
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderReadRepository, OrderReadRepository>();



// --------------------
// MediatR (REGISTER ONCE)
// --------------------
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly);
});

// --------------------
// MassTransit + RabbitMQ
// --------------------
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderRequestedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("order-requested-queue", e =>
        {
            e.ConfigureConsumer<OrderRequestedConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });
});

// --------------------
// API
// --------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
//    dbContext.Database.Migrate();
//}

Console.WriteLine(
    builder.Configuration.GetConnectionString("OrdersDb"));


// --------------------
// Middleware
// --------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
