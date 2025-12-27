using MassTransit;
using Payments.Api.Consumers;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
