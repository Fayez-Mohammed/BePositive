// Base.Services/Implementations/HospitalService.cs
using Base.DAL.Contexts;
using Base.Services.Interfaces.HospitalInterfaces;
using Base.Shared.DTOs.HospitalDTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // <-- تأكد من وجود هذا الـ namespace فوق

namespace Base.Services.Implementations
{
    public class HospitalService : IHospitalService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HospitalService> _logger; // 1. تعريفه كـ Field هنا
        private const int CooldownDays = 90;

        // 2. حقنه داخل الـ Constructor هنا
        public HospitalService(AppDbContext context, ILogger<HospitalService> logger)
        {
            _context = context;
            _logger = logger; // 3. تعيين القيمة
        }

        public async Task<EligibleDonorsCountResponseDTO> GetNearbyEligibleDonorsCountAsync(
            string hospitalAdminUserId,
            GetEligibleDonorsCountDTO dto)
        {
            // 1. جلب الـ HospitalId الخاص بالأدمن الحالي (Scalars only)
            var hospitalAdmin = await _context.HospitalAdmins
                .AsNoTracking()
                .Where(ha => ha.UserId == hospitalAdminUserId && !ha.IsDeleted)
                .Select(ha => new { ha.HospitalId })
                .FirstOrDefaultAsync();

            if (hospitalAdmin == null)
                throw new UnauthorizedAccessException("No active hospital admin record found.");

            // 2. جلب إحداثيات ومجسم المستشفى والتحقق من حالتها (Cast Enum to Int لـ Rule 6)
            var hospital = await _context.Hospitals
                .AsNoTracking()
                .Where(h => h.Id == hospitalAdmin.HospitalId && !h.IsDeleted)
                .Select(h => new
                {
                    h.Id,
                    h.Name,
                    h.Latitude,
                    h.Longitude,
                    StatusInt = (int)h.Status
                })
                .FirstOrDefaultAsync();

            if (hospital == null || hospital.StatusInt != 2) // فرضاً أن 2 تعني Active
                throw new InvalidOperationException("Hospital is not found or not active.");

            if (!hospital.Latitude.HasValue || !hospital.Longitude.HasValue)
                throw new InvalidOperationException("Hospital coordinates are not configured correctly.");

            // 3. جلب الفصيلة المطلوبة للاستجابة
            var bloodType = await _context.BloodTypes
                .AsNoTracking()
                .Where(b => b.Id == dto.BloodTypeId)
                .Select(b => new { b.Id, b.TypeName })
                .FirstOrDefaultAsync();

            if (bloodType == null)
                throw new ArgumentException("Invalid blood type selected.");

            // 4. جلب الفصائل المتوافقة
            var compatibleBloodTypeIds = await _context.BloodTypeCompatibilities
                .AsNoTracking()
                .Where(c => c.RecipientBloodTypeId == dto.BloodTypeId)
                .Select(c => c.DonorBloodTypeId)
                .ToListAsync();

            if (!compatibleBloodTypeIds.Contains(dto.BloodTypeId))
                compatibleBloodTypeIds.Add(dto.BloodTypeId);

            // 5. جلب المتبرعين المؤهلين طبياً من قاعدة البيانات (Filtering early)
            // 5. جلب المتبرعين المؤهلين طبياً مع تحويل الإحداثيات مباشرة لـ double أثناء السحب
            var cutoff = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-CooldownDays);

            var dbDonors = await _context.Donors
                .AsNoTracking()
                .Where(d =>
                    !d.IsDeleted &&
                    d.IsAvailableForDonation &&
                    d.FcmToken != null &&
                    d.Latitude.HasValue &&
                    d.Longitude.HasValue &&
                    compatibleBloodTypeIds.Contains(d.BloodTypeId) &&
                    (d.LastDonationDate == null || d.LastDonationDate < cutoff))
                .Select(d => new
                {
                    // التحويل هنا بيحل مشكلة الـ decimal تماماً قبل ما الداتا تدخل الذاكرة
                    Latitude = (double)d.Latitude.Value,
                    Longitude = (double)d.Longitude.Value
                })
                .ToListAsync();

            _logger.LogInformation(
                "Total medically eligible donors in database: {Count}", dbDonors.Count);

            if (!dbDonors.Any())
                return new EligibleDonorsCountResponseDTO { TotalEligibleDonorsCount = 0 };

            // 6. الحساب الجغرافي في الذاكرة (الآن المتغيرات كلها double متوافقة 100%)
            var count = dbDonors
                .Count(d => CalculateDistanceKm(
                    (double)hospital.Latitude.Value, // عمل كاست لموقع المستشفى أيضاً
                    (double)hospital.Longitude.Value,
                    d.Latitude,
                    d.Longitude) <= dto.MaxDistanceKm);

            // 7. بناء الـ Response الموحد
            return new EligibleDonorsCountResponseDTO
            {
                BloodTypeId = bloodType.Id,
                BloodTypeName = bloodType.TypeName,
                SearchedDistanceKm = dto.MaxDistanceKm,
                TotalEligibleDonorsCount = count
            };
        }

        // معادلة Haversine لحساب المسافة
        private static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadiusKm = 6371.0;

            var dLat = (lat2 - lat1) * Math.PI / 180.0;
            var dLon = (lon2 - lon1) * Math.PI / 180.0;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusKm * c;
        }
    }
}