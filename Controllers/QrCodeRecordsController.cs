using LibrarySeatTrackingAPI.Application.DTOs; // DTO sınıflarını kullanmak için
using LibrarySeatTrackingAPI.Application.Interfaces; // IQrCodeRecordService interface'ini kullanmak için
using Microsoft.AspNetCore.Authorization; // [Authorize] attribute'u için
using Microsoft.AspNetCore.Mvc; // ControllerBase ve API attribute'ları için

namespace LibrarySeatTrackingAPI.Controllers; // Bu dosyanın Controllers katmanına ait olduğunu belirtir

[ApiController] // Bu sınıfın API controller olduğunu belirtir
[Route("api/qr-code-records")] // Endpoint adreslerini api/qr-code-records olarak başlatır
public class QrCodeRecordsController : ControllerBase // API controller temel sınıfından miras alır
{
    private readonly IQrCodeRecordService _qrCodeRecordService; // QR kod işlemleri için service

    public QrCodeRecordsController(IQrCodeRecordService qrCodeRecordService) // Service bağımlılığını dışarıdan alır
    {
        _qrCodeRecordService = qrCodeRecordService; // Gelen service nesnesini sınıf içinde kullanmak için saklar
    }

    [Authorize(Roles = "Admin,Staff")] // Bu endpoint'e Admin ve Staff rolündeki kullanıcılar erişebilir
    [HttpPost] // POST api/qr-code-records endpointini oluşturur
    public async Task<IActionResult> Create(CreateQrCodeRecordDto request) // Yeni QR kod kaydı ekler
    {
        var result = await _qrCodeRecordService.CreateAsync(request); // QR kod ekleme işlemini service'e gönderir

        if (!result.Success) // İşlem başarısızsa
        {
            return BadRequest(result); // 400 Bad Request cevabı döner
        }

        return Ok(result); // İşlem başarılıysa 200 OK cevabı döner
    }

    [Authorize(Roles = "Admin,Staff")] // Bu endpoint'e Admin ve Staff rolündeki kullanıcılar erişebilir
    [HttpGet] // GET api/qr-code-records endpointini oluşturur
    public async Task<IActionResult> GetAll() // Tüm QR kod kayıtlarını listeler
    {
        var result = await _qrCodeRecordService.GetAllAsync(); // QR kod listeleme işlemini service'e gönderir

        return Ok(result); // Liste sonucunu 200 OK olarak döner
    }
    [Authorize(Roles = "Admin,Staff")] // Bu endpoint'e Admin ve Staff erişebilir
        [HttpPut("{qrCodeRecordId}/status")] // PUT api/qr-code-records/1/status endpointini oluşturur
        public async Task<IActionResult> UpdateStatus(int qrCodeRecordId, UpdateQrCodeStatusDto request) // QR kodu aktif/pasif yapar
        {
            var result = await _qrCodeRecordService.UpdateStatusAsync(qrCodeRecordId, request); // Durum güncelleme işlemini service'e gönderir

            if (!result.Success) // İşlem başarısızsa
            {
                return BadRequest(result); // 400 Bad Request döner
            }

            return Ok(result); // İşlem başarılıysa 200 OK döner
        }
}