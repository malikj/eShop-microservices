using Microsoft.EntityFrameworkCore;
using Orders.Application.Abstractions.Repositories;
using Orders.Application.Ordering.Commands.CreateOrder;
using Orders.Infrastructure.Persistence;
using Orders.Infrastructure.Repositories;
using Orders.Application.Ordering.Commands.PayOrder;

using Orders.Application.Abstractions.Messaging;
using Serilog;
using Orders.Application.Abstractions.Correlation;
using Orders.Infrastructure.Correlation;
using Orders.Api.Middleware;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog.Enrichers.Span;
using Orders.Infrastructure.Messaging;
using Orders.Infrastructure.Messaging.Consumers;


var builder = WebApplication.CreateBuilder(args);

// configure Serilog
builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithSpan()   // IMPORTANT
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] " +
            "TraceId={TraceId} SpanId={SpanId} " +
            "{Message:lj}{NewLine}{Exception}");
});



// --------------------
// Database
// --------------------
//builder.Services.AddDbContext<OrdersDbContext>(options =>
//{
//    options.UseSqlServer(
//        builder.Configuration.GetConnectionString("OrdersDb"));
//});

// Inmem db
builder.Services.AddDbContext<OrdersDbContext>(options =>
{
    options.UseInMemoryDatabase("OrdersDb");
});

builder.Services.AddHttpContextAccessor();

// --------------------
// Repositories
// --------------------
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderReadRepository, OrderReadRepository>();
// Register AzureServiceBusPublisher as IEventPublisher
builder.Services.AddScoped<IEventPublisher, AzureServiceBusPublisher>();
builder.Services.AddScoped<IInboxRepository, InboxRepository>();





// Register OutboxPublisher as a hosted service (if still needed)
//builder.Services.AddHostedService<OutboxPublisher>();


// Register the Azure Service Bus consumer background service
builder.Services.AddHostedService<AzureServiceBusConsumer>();


builder.Services.AddScoped<ICorrelationIdAccessor, HttpCorrelationIdAccessor>();


// Bind ServiceBusSettings from configuration
builder.Services.Configure<Orders.Infrastructure.Messaging.ServiceBusSettings>(
    builder.Configuration.GetSection(Orders.Infrastructure.Messaging.ServiceBusSettings.SectionName));



// --------------------
// MediatR (REGISTER ONCE)
// --------------------

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(PayOrderCommand).Assembly);
});


builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddSource("Orders.Messaging")
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService("OrdersService"))
            .SetSampler(new AlwaysOnSampler())
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
            })
            .AddHttpClientInstrumentation()
            // .AddMassTransitInstrumentation() // Removed with MassTransit
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://host.docker.internal:4318/v1/traces");
                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            });
    });


// --------------------
// API
// --------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register consumers for DI
builder.Services.AddScoped<OrderRequestedConsumer>();
builder.Services.AddScoped<PaymentSucceededConsumer>();
builder.Services.AddScoped<PaymentFailedConsumer>();


var app = builder.Build();

Console.WriteLine(
    builder.Configuration.GetConnectionString("OrdersDb"));


// --------------------
// Middleware
// --------------------
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();
// 2 Register Middleware
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
