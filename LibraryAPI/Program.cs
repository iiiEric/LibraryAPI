using LibraryAPI.Data;
using LibraryAPI.Middlewares;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

#region Services area
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
#endregion

var app = builder.Build();

#region Middleware area
app.UseLogRequest();

app.MapControllers();
#endregion

app.Run();
