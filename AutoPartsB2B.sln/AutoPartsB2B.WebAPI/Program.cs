using AutoPartsB2B.Infrastructure.Data;
using AutoPartsB2B.WebAPI.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Veritabanı olarak SQLite ve appsettings.json'daki bağlantıyı kullanıyoruz
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

//Frontend için CORS ayarı
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => policy
        .SetIsOriginAllowed(origin => true) // Tüm Vue portlarına (5173, 8080 vb.) izin ver
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()); // DİKKAT: SignalR canlı bağlantısı için bu satır ZORUNLUDUR!
});

//JWT Kimlik Doğrulama (Authentication) Ayarları
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("KayseriOtoB2BSuperGizliAnahtar123456!")),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

//Cors politikası Middleware'i
app.UseCors("AllowAll");

//DİKKAT: Kimlik Doğrulama (Authentication) her zaman Yetkilendirmeden (Authorization) ÖNCE gelmelidir!
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<StockHub>("/stockHub");

app.Run();