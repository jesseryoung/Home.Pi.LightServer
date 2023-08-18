
using System.Text.Json;
using System.Text.Json.Serialization;
using Home.Pi.LightServer.Animations;
using Home.Pi.LightServer.Console;
using Home.Pi.LightServer.Led;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<ILightController, ConsoleLightController>();
    builder.Services.AddLogging(c => c.ClearProviders());
}
else
{
    builder.Services.AddSingleton<ILightController, Ws2812bLightController>();
}

builder
    .Services
    .AddCors()
    .AddSingleton<AnimationController>()
    .AddSingleton(sp =>
    {
        var pathToFile = Environment.GetEnvironmentVariable(LightServerConfiguration.ENVIRONMENT_VARIABLE) ?? "/config/lightserver.json";
        var options = new JsonSerializerOptions()
        {
            Converters = {
                new JsonStringEnumConverter()
            }
        };
        var config = JsonSerializer.Deserialize<LightServerConfiguration>(File.ReadAllText(pathToFile), options) ?? throw new Exception($"Could not load {pathToFile}");
        return config;
    })
    .AddScoped<ClockAnimation>()
    .AddScoped<LightBarAnimation>()
    .AddHostedService<AnimationService>();


var app = builder.Build();



app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());


var animationTypeMap = new Dictionary<string, Type>()
{
    { "clock", typeof(ClockAnimation) }
};

app.MapGet("startAnimation/{animationTypeName}", (AnimationController controller, string animationTypeName) =>
{
    if (animationTypeMap.TryGetValue(animationTypeName, out var animationType))
    {
        controller.StartAnimation(animationType);
        return Results.Json(new
        {
            success = true
        });
    }

    return Results.BadRequest(new
    {
        message = $"Animation '{animationTypeName}' doesn't exist."
    });
});

app.MapGet("stop", (AnimationController controller) =>
{
    controller.StopAnimation();
    return Results.Json(new
    {
        success = true
    });
});

app.Run();