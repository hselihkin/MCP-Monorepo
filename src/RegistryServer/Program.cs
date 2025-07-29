using RegistryServer.Tools;
using RegistryServer.Data;
using RegistryServer.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient();

// Add services
builder.Services.AddScoped<IServerService, ServerService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<ServerTools>()
    .WithTools<AuthTools>();

var app = builder.Build();

// Ensure database is created (for development)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

app.MapMcp();
app.Run();