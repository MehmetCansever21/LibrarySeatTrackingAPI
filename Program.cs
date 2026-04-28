using LibrarySeatTrackingAPI.Infrastructure.Data; // ApplicationDbContext sınıfını kullanmak için
using Microsoft.EntityFrameworkCore; // UseSqlServer metodu için
using LibrarySeatTrackingAPI.Infrastructure.Seed; // DataSeeder sınıfını kullanmak için
using LibrarySeatTrackingAPI.Application.Interfaces; // IAuthService interface'ini kullanmak için
using LibrarySeatTrackingAPI.Application.Services; // AuthService sınıfını kullanmak için

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers(); // Controller sınıflarını API endpoint olarak kullanmamızı sağlar
builder.Services.AddDbContext<ApplicationDbContext>(options => // ApplicationDbContext'i sisteme servis olarak ekler
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")); // appsettings.json içindeki DefaultConnection ile SQL Server'a bağlanır
});
builder.Services.AddScoped<IAuthService, AuthService>(); // IAuthService istenince AuthService nesnesi üretir

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Library Seat Tracking API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope()) // Uygulama servislerinden geçici bir kullanım alanı oluşturur
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(); // ApplicationDbContext nesnesini alır

    await db.Database.MigrateAsync(); // Bekleyen migration varsa veritabanına uygular

    await DataSeeder.SeedAsync(db); // Test kullanıcılarını, örnek alanı, masayı ve QR kodu ekler
}
app.MapControllers(); // Controller içindeki endpointleri uygulamaya bağlar
app.Run();