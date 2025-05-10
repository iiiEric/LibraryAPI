using LibraryAPI.Data;
using LibraryAPI.Entities;
using LibraryAPI.Middlewares;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region Services area

#region Encryption
builder.Services.AddDataProtection();
#endregion

#region CORS
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()!;
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigins",
        builder =>
        {
            builder.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();
        });
});
#endregion

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllers().AddNewtonsoftJson();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityCore<User>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<UserManager<User>>();
builder.Services.AddScoped<SignInManager<User>>();
builder.Services.AddTransient<IUsersServicies, UsersServicies>();
#region Hash
builder.Services.AddTransient<IHashServicies, HashServicies>();
#endregion
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
#endregion

var app = builder.Build();

#region Middleware area

#region CORS
app.UseCors("AllowOrigins");
#endregion

app.UseLogRequest();

app.MapControllers();
#endregion

app.Run();
