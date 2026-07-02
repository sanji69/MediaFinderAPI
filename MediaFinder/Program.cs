using MediaFinder.Data;
using MediaFinder.Interface;
using MediaFinder.Options;
using MediaFinder.Services.Admin;
using MediaFinder.Services.Auth;
using MediaFinder.Services.Comments;
using MediaFinder.Services.Ebay;
using MediaFinder.Services.Email;
using MediaFinder.Services.Favorites;
using MediaFinder.Services.Localization;
using MediaFinder.Services.Profile;
using MediaFinder.Services.Ratings;
using MediaFinder.Services.Tmdb;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtOptions = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtOptions>();

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtOptions!.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Secret))
        };
    });

builder.Services.Configure<TmdbOptions>(
    builder.Configuration.GetSection("Tmdb"));
builder.Services.Configure<LocalizationOptions>(
    builder.Configuration.GetSection("Localization"));
builder.Services.Configure<EbayOptions>(
    builder.Configuration.GetSection("Ebay"));
builder.Services.AddDbContext<MediaFinderDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<FrontendOptions>(
    builder.Configuration.GetSection("Frontend"));
builder.Services.Configure<SmtpOptions>(
    builder.Configuration.GetSection("Smtp"));

builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddHttpClient<ITmdbService, TmdbService>(); 
builder.Services.AddHttpClient<ISearchService, SearchService>(); 
builder.Services.AddHttpClient<IPhysicalOfferProvider, EbayOfferProvider>();
builder.Services.AddHttpClient<IEbayAuthService, EbayAuthService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IAdminService, AdminService>();

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
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseCors("VueDevClient");

app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

app.MapControllers();

app.Run();
