using ECommereceApi.Repo;
using ECommereceApi.Services.classes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using CloudinaryDotNet;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using System.Globalization;
using ECommereceApi.Services.Interfaces;
using Serilog;
using ECommereceApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

var webHostEnvironment = builder.Services.BuildServiceProvider().GetRequiredService<IWebHostEnvironment>();

builder.Services.AddDbContext<ECommerceContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    .AddInterceptors(new SoftDeleteInterceptor());
});
builder.Services.AddCors(corsOptions =>
{
    corsOptions.AddPolicy("myPolicy", corsPolicyBuilder =>
    {
        corsPolicyBuilder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("Bearer-Token");
    });
});

#region FileServer

var cloudinaryCredentials = builder.Configuration.GetSection("Cloudinary");
var account = new Account(
    cloudinaryCredentials["CloudName"],
    cloudinaryCredentials["ApiKey"],
    cloudinaryCredentials["ApiSecret"]
);

builder.Services.AddSingleton(account);
builder.Services.AddScoped<Cloudinary>();
#endregion

#region Localization Service
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.SupportedCultures = new List<CultureInfo>
    {
        new CultureInfo("en-US"),
        new CultureInfo("ar-EG")
    };
    options.SupportedCultures = options.SupportedCultures;
    options.SupportedUICultures = options.SupportedCultures;
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(culture: "ar-EG", uiCulture: "ar-EG");

});

builder.Services.AddScoped<ILanguageRepo, LanguageRepo>();
#endregion

#region JWT Service

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JWT:secretkey"))),
    };
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT token in this format: Bearer {token}",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };
    c.IncludeXmlComments(webHostEnvironment.WebRootPath + "\\mydoc.xml");
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
#endregion

#region Logging Service

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/eCommercelog-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

//builder.Services.AddLogging(config =>
//{
//    config.AddSerilog();
//});

#endregion

// Add AutoMapper Service
builder.Services.AddAutoMapper(typeof(Program));

//File Server Service
builder.Services.AddScoped<IFileCloudService, FileCloudService>();

//builder.Services.AddScoped(typeof(IGenericRepo<>), typeof(GenericRepo<>));
builder.Services.AddScoped<IProductRepo, ProductRepo>();
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<IOfferRepo, OfferRepo>();
builder.Services.AddScoped<IWebInfoRepo, WebInfoRepo>();
builder.Services.AddScoped<IWishListRepo, WishListRepo>();
builder.Services.AddScoped<IUserManagementRepo, UserManagementRepo>();
builder.Services.AddScoped<IMailRepo, MailRepo>();
builder.Services.AddScoped<ICartRepo, CartRepo>();

// Global Exception Handling Service
builder.Services.AddTransient<GlobalExceptionHandlingMiddleware>();

builder.Services.AddTransient<NotificationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Global Exception Handling Middleware
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseCors("myPolicy");
app.UseAuthentication();
app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();

app.Run();
