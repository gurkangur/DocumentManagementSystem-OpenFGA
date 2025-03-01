using DocumentManagement.Data;
using DocumentManagement.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<DocumentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<OpenFgaService>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var fgaService = scope.ServiceProvider.GetRequiredService<OpenFgaService>();
    fgaService.InitializeAuthorizationModel().GetAwaiter().GetResult();
}

// Configure the HTTP request pipeline.

app.UseAuthorization();
ApplyMigration();
app.MapControllers();

app.Run();


void ApplyMigration()
{
    using (var scope = app.Services.CreateScope())
    {
        var _Db = scope.ServiceProvider.GetRequiredService<DocumentDbContext>();
        if (_Db != null)
        {
            if (_Db.Database.GetPendingMigrations().Any())
            {
                _Db.Database.Migrate();
            }
        }
    }
}