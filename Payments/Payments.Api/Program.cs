using MassTransit;
using Payments.Api.Consumers;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter;
using Serilog;
using Serilog.Enrichers.Span;


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



// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderPaymentRequestedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitSection = builder.Configuration.GetSection("RabbitMQ");

        cfg.Host(
            rabbitSection["Host"],
            "/",
            h =>
            {
                h.Username(rabbitSection["Username"]!);
                h.Password(rabbitSection["Password"]!);
            });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService("PaymentsService"))
            .SetSampler(new AlwaysOnSampler())
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("MassTransit")
             .AddSource("MassTransit.Transport")  
            .AddMassTransitInstrumentation()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://localhost:4318/v1/traces");
                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            });
    });


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
