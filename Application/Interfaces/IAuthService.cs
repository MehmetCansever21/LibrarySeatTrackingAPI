using LibrarySeatTrackingAPI.Application.DTOs; // LoginRequestDto, LoginResponseDto ve ApiResponseDto kullanmak için

namespace LibrarySeatTrackingAPI.Application.Interfaces; // Bu dosyanın Application/Interfaces katmanına ait olduğunu belirtir

public interface IAuthService
{
    Task<ApiResponseDto<LoginResponseDto>> LoginAsync(LoginRequestDto request); // Kullanıcının giriş yapmasını sağlayacak metot
}