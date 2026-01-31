using Asp.Versioning;
using CBTW_TEST.Core.Activities.Match;
using CBTW_TEST.Core.Workflows.Match;
using CBTW_TEST.Services.Http;
using Dapr.Workflow;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
options.AddDefaultPolicy(
    policy =>
{
policy.AllowAnyHeader();
policy.AllowAnyOrigin();
policy.AllowAnyMethod();
});
});

builder.AddServiceDefaults();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddTransient<OpenLibraryApiClient>();
builder.Services.AddDaprWorkflow(options =>
{
    options.RegisterWorkflow<LibraryDiscoveryWorkflow>();
    
    options.RegisterActivity<ExtractSearchEntitiesActivity>();
    options.RegisterActivity<SearchOpenLibraryActivity>();
    options.RegisterActivity<RankAndExplainActivity>();
});


builder.Services.AddLogging();


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.UseCors(policy =>
{
    policy.AllowAnyHeader();
    policy.AllowAnyOrigin();
    policy.AllowAnyMethod();
}
);
app.MapDefaultEndpoints();

app.UseSwagger();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
