using MediaFinder.Interface;
using MediaFinder.Options;
using MediaFinder.Services.Localization;
using MediaFinder.Services.Tmdb;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<TmdbOptions>(
    builder.Configuration.GetSection("Tmdb"));
builder.Services.Configure<LocalizationOptions>(
    builder.Configuration.GetSection("Localization"));

builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddHttpClient<ITmdbService, TmdbService>(); 
builder.Services.AddHttpClient<ISearchService, SearchService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("VueDevClient", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseCors("VueDevClient");

app.UseAuthorization();

app.MapControllers();

app.Run();
