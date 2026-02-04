using CBTW_TEST.Core.Activities.Match;
using CBTW_TEST.Core.LibraryDiscover;
using CBTW_TEST.Domain.Interfaces;
using CBTW_TEST.Services.Http;
using Dapr.Workflow;
using Google.GenAI;
using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
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
var geminiAIKey = builder.Configuration.GetSection("GEMINI_API_KEY");
var geminiAI =  new Client(apiKey:geminiAIKey.Value);
builder.Services.AddSingleton(geminiAI);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<OpenLibraryApiClient>(client =>
{
    client.BaseAddress = new Uri("https://openlibrary.org/");
});
builder.Services.AddScoped<ExtractSearchEntitiesActivity>();
builder.Services.AddScoped<SearchOpenLibraryActivity>();
builder.Services.AddScoped<RankAndExplainActivity>();
builder.Services.AddScoped<ILibraryDiscoveryService, LibraryDiscoveryService>();


builder.Services.AddLogging();
builder.Services.AddSwaggerGen();
var app = builder.Build();

app.UseCors(policy =>
{
    policy.AllowAnyHeader();
    policy.AllowAnyOrigin();
    policy.AllowAnyMethod();
}
);
app.MapDefaultEndpoints();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();


