using LibrarySeatTrackingAPI.Application.DTOs; // LoginRequestDto ve ApiResponseDto sınıflarını kullanmak için
using LibrarySeatTrackingAPI.Application.Interfaces; // IAuthService interface'ini kullanmak için
using Microsoft.AspNetCore.Mvc; // ControllerBase, ApiController, Route, HttpPost gibi yapılar için

namespace LibrarySeatTrackingAPI.Controllers; // Bu dosyanın Controllers katmanına ait olduğunu belirtir

[ApiController] // Bu sınıfın API controller olduğunu belirtir
[Route("api/[controller]")] // Route adresini api/auth şeklinde oluşturur
public class AuthController : ControllerBase // API controller temel sınıfından miras alır
{
    private readonly IAuthService _authService; // Login işlemleri için AuthService'i interface üzerinden kullanır

    public AuthController(IAuthService authService) // IAuthService bağımlılığını dışarıdan alır
    {
        _authService = authService; // Gelen authService nesnesini sınıf içinde kullanmak için saklar
    }

    [HttpPost("login")] // POST api/auth/login endpointini oluşturur
    public async Task<IActionResult> Login(LoginRequestDto request) // Kullanıcıdan gelen login bilgisini alır
    {
        var result = await _authService.LoginAsync(request); // Login işlemini AuthService'e gönderir

        if (!result.Success) // Login başarısızsa
        {
            return BadRequest(result); // 400 Bad Request cevabı döner
        }

        return Ok(result); // Login başarılıysa 200 OK cevabı döner
    }
}