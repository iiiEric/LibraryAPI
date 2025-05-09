using LibraryAPI.Data;
using LibraryAPI.Entities;
using LibraryAPI.Middlewares;
using LibraryAPI.Services;
using LibraryAPI.Swagger;
using LibraryAPI.Utils;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region Services area

builder.Services.AddOutputCache(options =>
{
    options.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(15);
});

//distributed cache
//builder.Services.AddStackExchangeRedisOutputCache(options =>
//{
//    options.Configuration = builder.Configuration["RedisConnection"];
//    options.InstanceName = "LibraryAPI";
//});

//Encryption
//builder.Services.AddDataProtection();

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()!;
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigins",
        builder =>
        {
            builder.WithOrigins(allowedOrigins).AllowAnyMethod().WithExposedHeaders("total-number-of-records");
        });
});

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ExecutionTimeLogger>();
    options.Conventions.Add(new ApiVersioningConvention());
}).AddNewtonsoftJson();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityCore<User>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<UserManager<User>>();
builder.Services.AddScoped<SignInManager<User>>();
builder.Services.AddTransient<IUsersService, UsersService>();
//Hash
//builder.Services.AddTransient<IHashServicies, HashServicies>();

builder.Services.AddTransient<IFileStorageService, AzureFileStorageService>();
builder.Services.AddScoped<ValidateBookFilter>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWTKey"]!)),
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdmin", policy => policy.RequireClaim("SuperAdmin"));
    options.AddPolicy("Admin", policy => policy.RequireClaim("Admin"));
});

var apiVersions = new[] { "v1" };
builder.Services.AddSwaggerGen(options =>
{
    foreach (var version in apiVersions)
    {
        options.SwaggerDoc(version, new OpenApiInfo
        {
            Version = version,
            Title = "Library API",
            Description = "Library API for managing books and authors",
            Contact = new OpenApiContact
            {
                Name = "Eric Campos Dom�nguez",
                Url = new Uri("https://eric-campos.netlify.app/")
            },
            License = new OpenApiLicense
            {
                Name = "MIT",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        });
    }

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    options.OperationFilter<AuthorizationFilter>();
});
#endregion

var app = builder.Build();

#region Middleware area
app.UseExceptionHandler(exceptionHandlerApp => exceptionHandlerApp.Run(async context =>
{
    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
    var excepcion = exceptionHandlerFeature?.Error!;

    var error = new Error()
    {
        ErrorMessage = excepcion.Message,
        StrackTrace = excepcion.StackTrace,
        OccurredAt = DateTime.UtcNow
    };

    var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();
    dbContext.Add(error);
    await dbContext.SaveChangesAsync();
    await Results.InternalServerError(new
    {
        type = "Error",
        message = "An unexpected error occurred",
        status = 500
    }).ExecuteAsync(context);
}));

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    foreach (var version in apiVersions)
        options.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"Library API {version.ToUpper()}");
});

app.UseCors("AllowOrigins");

app.UseOutputCache();

app.UseLogRequest();

app.MapControllers();
#endregion

app.Run();
