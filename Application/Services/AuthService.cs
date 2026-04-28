using System.IdentityModel.Tokens.Jwt; // JWT token oluşturmak için
using System.Security.Claims; // Token içine kullanıcı bilgileri eklemek için
using System.Text; // Secret key'i byte dizisine çevirmek için
using LibrarySeatTrackingAPI.Application.DTOs; // DTO sınıflarını kullanmak için
using LibrarySeatTrackingAPI.Application.Interfaces; // IAuthService interface'ini kullanmak için
using LibrarySeatTrackingAPI.Domain; // RefreshToken entity'sini kullanmak için
using LibrarySeatTrackingAPI.Infrastructure.Data; // ApplicationDbContext kullanmak için
using Microsoft.EntityFrameworkCore; // FirstOrDefaultAsync gibi EF metotları için
using Microsoft.IdentityModel.Tokens; // Token güvenlik ayarları için

namespace LibrarySeatTrackingAPI.Application.Services; // Bu dosyanın Application/Services katmanına ait olduğunu belirtir

public class AuthService : IAuthService // AuthService, IAuthService sözleşmesini uygular
{
    private readonly ApplicationDbContext _db; // Veritabanı işlemleri için kullanılır

    private readonly IConfiguration _configuration; // appsettings.json içindeki ayarları okumak için kullanılır

    public AuthService(ApplicationDbContext db, IConfiguration configuration) // Gerekli bağımlılıkları dışarıdan alır
    {
        _db = db; // Gelen DbContext'i sınıf içinde kullanmak için saklar

        _configuration = configuration; // Gelen configuration nesnesini sınıf içinde kullanmak için saklar
    }

    public async Task<ApiResponseDto<LoginResponseDto>> LoginAsync(LoginRequestDto request) // Kullanıcının giriş yapmasını sağlar
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == request.Email); // E-postaya göre kullanıcıyı veritabanında arar

        if (user is null) // Kullanıcı bulunamazsa
        {
            return new ApiResponseDto<LoginResponseDto> // Başarısız cevap döner
            {
                Success = false, // İşlem başarısız
                Message = "Invalid e-mail or password.", // Güvenlik için genel hata mesajı
                Data = null // Veri dönülmez
            };
        }

        var passwordIsValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash); // Girilen şifre ile hashlenmiş şifreyi karşılaştırır

        if (!passwordIsValid) // Şifre yanlışsa
        {
            return new ApiResponseDto<LoginResponseDto> // Başarısız cevap döner
            {
                Success = false, // İşlem başarısız
                Message = "Invalid e-mail or password123456.", // Güvenlik için genel hata mesajı
                Data = null // Veri dönülmez
            };
        }

        if (user.IsBlocked) // Kullanıcı blokeliyse
        {
            return new ApiResponseDto<LoginResponseDto> // Başarısız cevap döner
            {
                Success = false, // İşlem başarısız
                Message = "User is blocked.", // Kullanıcının blokeli olduğunu bildirir
                Data = null // Veri dönülmez
            };
        }

        var accessToken = GenerateAccessToken(user); // Kullanıcı için JWT access token oluşturur

        var refreshToken = GenerateRefreshToken(); // Kullanıcı için refresh token oluşturur

        var refreshTokenEntity = new RefreshToken // Refresh token kaydı oluşturur
        {
            Token = refreshToken, // Üretilen refresh token değeri

            ExpiresAt = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays")), // Refresh token bitiş tarihi

            IsRevoked = false, // Token başlangıçta iptal edilmiş değildir

            UserId = user.Id // Tokenı giriş yapan kullanıcıya bağlar
        };

        _db.RefreshTokens.Add(refreshTokenEntity); // Refresh tokenı veritabanına eklenmek üzere hazırlar

        await _db.SaveChangesAsync(); // Refresh tokenı veritabanına kaydeder

        return new ApiResponseDto<LoginResponseDto> // Başarılı login cevabı döner
        {
            Success = true, // İşlem başarılı

            Message = "Login successful.", // Başarı mesajı

            Data = new LoginResponseDto // Login sonrası dönecek veri
            {
                UserId = user.Id, // Kullanıcının Id bilgisi

                FullName = user.FullName, // Kullanıcının adı soyadı

                Email = user.Email, // Kullanıcının e-posta adresi

                Role = user.Role.ToString(), // Kullanıcının rolünü string olarak döner

                AccessToken = accessToken, // Oluşturulan JWT access token

                RefreshToken = refreshToken // Oluşturulan refresh token
            }
        };
    }

    private string GenerateAccessToken(User user) // Kullanıcı bilgilerine göre JWT access token üretir
    {
        var claims = new List<Claim> // Token içine yazılacak kullanıcı bilgileri
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Kullanıcı Id bilgisini tokena ekler

            new Claim(ClaimTypes.Name, user.FullName), // Kullanıcı ad soyad bilgisini tokena ekler

            new Claim(ClaimTypes.Email, user.Email), // Kullanıcı e-posta bilgisini tokena ekler

            new Claim(ClaimTypes.Role, user.Role.ToString()) // Kullanıcı rol bilgisini tokena ekler
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)); // appsettings içindeki gizli key ile güvenlik anahtarı oluşturur

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // Tokenın hangi algoritma ile imzalanacağını belirler

        var token = new JwtSecurityToken // JWT token nesnesi oluşturur
        (
            issuer: _configuration["Jwt:Issuer"], // Tokenı üreten sistem bilgisi

            audience: _configuration["Jwt:Audience"], // Tokenın hangi kullanıcı kitlesi için üretildiği

            claims: claims, // Token içine eklenecek kullanıcı bilgileri

            expires: DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:AccessTokenExpirationMinutes")), // Token bitiş tarihi

            signingCredentials: credentials // Token imza bilgisi
        );

        return new JwtSecurityTokenHandler().WriteToken(token); // Token nesnesini string değere çevirir
    }

    private static string GenerateRefreshToken() // Rastgele refresh token üretir
    {
        return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"); // Uzun ve rastgele token değeri üretir
    }
}