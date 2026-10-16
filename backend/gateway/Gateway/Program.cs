using CustomerFeedback.BuildingBlocks.Hosting;
using CustomerFeedback.BuildingBlocks.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.AddSerilogLogging();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:5173"])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseCustomerFeedbackPipeline();
app.UseCors();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapReverseProxy();

app.Run();
