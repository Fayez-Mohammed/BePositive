using Base.DAL.Contexts;
using Base.DAL.Models.BaseModels;
using Base.DAL.Models.DonorModels;
using Base.DAL.Models.RequestModels;
using Base.Services.Interfaces;
using Base.Shared.DTOs.DonorDTOs;
using Base.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Base.Services.Implementations
{
    public class DonorService : IDonorService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DonorService(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        // 🅰️ ميثود إكمال البيانات وترقية الحساب لـ متبرع
        public async Task<bool> CompleteDonorProfileAsync(string userId, CompleteProfileDTO dto)
        {
            // 1. التأكد من أن المستخدم لم يقم بإنشاء بروفايل متبرع من قبل لمنع الازدواجية
            var exists = await _context.Donors.AnyAsync(d => d.UserId == userId && !d.IsDeleted);
            if (exists)
                throw new InvalidOperationException("This user is already registered as a donor.");

            // 2. التحقق من السن القانوني للتبرع (أكبر من أو يساوي 18 سنة)
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - dto.BirthDate.Year;
            if (dto.BirthDate > today.AddYears(-age)) age--;

            if (age < 18)
                throw new InvalidOperationException("Medical Rule: You must be at least 18 years old to register as a blood donor.");

            // 3. إنشاء كيان المتبرع الجديد وربطه بالـ UserId
            var donor = new Donor
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                NationalId = dto.NationalId,
                BloodTypeId = dto.BloodTypeId,
                CityId = dto.CityId,
                Gender = dto.Gender,
                BirthDate = dto.BirthDate,
                LastDonationDate = dto.LastDonationDate,
                Latitude = dto.Latitude,     // الـ Types متوافقة decimal تلقائياً
                Longitude = dto.Longitude,   // الـ Types متوافقة decimal تلقائياً
                FcmToken = dto.FcmToken,
                IsAvailableForDonation = true,
                IsDeleted = false
            };

            await _context.Donors.AddAsync(donor);

            // 4. تحديث الـ ApplicationUser وترقيته في الـ Identity
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.Type = UserTypes.Donor; // تحويل الـ Enum لـ Donor

                // إضافة الـ Identity Role الرسمية للمستخدم لضمان تجديد صلاحيات الـ Token
                if (!await _userManager.IsInRoleAsync(user, "Donor"))
                {
                    await _userManager.AddToRoleAsync(user, "Donor");
                }
            }

            // 5. حفظ كل التغييرات في الـ Database حتة واحدة بنظام الـ Transaction الضمني
            await _context.SaveChangesAsync();
            return true;
        }

        // 🅱️ ميثود تحديث الـ FCM Token اللحظي من الـ Splash Screen
        public async Task<bool> UpdateFcmTokenAsync(string userId, UpdateFcmDTO dto)
        {
            var donor = await _context.Donors
                .Where(d => d.UserId == userId && !d.IsDeleted)
                .FirstOrDefaultAsync();

            if (donor == null)
                return false; // ليس متبرعاً بعد، فلا داعي لتحديث توكن التبرع له

            // تحديث التوكن لو كان متغير عن المتخزن حالياً لحفظ الـ Performance
            if (donor.FcmToken != dto.FcmToken)
            {
                donor.FcmToken = dto.FcmToken;
                await _context.SaveChangesAsync();
            }

            return true;
        }
        public async Task<AcceptRequestResultDTO> AcceptBloodRequestAsync(string donorUserId, AcceptRequestDTO dto)
        {
            // 1. جلب بيانات المتبرع الحالية
            var donor = await _context.Donors
                .Where(d => d.UserId == donorUserId && !d.IsDeleted)
                .FirstOrDefaultAsync();

            if (donor == null)
                throw new UnauthorizedAccessException("Donor profile not found.");

            if (!donor.IsAvailableForDonation)
                throw new InvalidOperationException("You are currently marked as unavailable for donation.");

            // 🛑 شرط الأمان الطبي الصارم: فحص الـ 3 أشهر (90 يوماً) للـ Cooldown
            var threeMonthsAgo = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3));
            if (donor.LastDonationDate.HasValue && donor.LastDonationDate.Value > threeMonthsAgo)
            {
                var nextAvailableDate = donor.LastDonationDate.Value.AddMonths(3);
                throw new InvalidOperationException($"Medical Cooldown: You cannot donate yet. Next eligible date is: {nextAvailableDate}");
            }

            // 2. جلب بيانات طلب التبرع الأصلي للتأكد من حالته المفتوحة
            var bloodRequest = await _context.DonationRequests
                .Where(r => r.Id == dto.RequestId && !r.IsDeleted)
                .FirstOrDefaultAsync();

            if (bloodRequest == null)
                throw new ArgumentException("Blood request not found.");

            if ((int)bloodRequest.Status != 1) // 1 = Open
                throw new InvalidOperationException("This blood request is no longer open or has been fulfilled.");

            // 3. حساب المسافة الجغرافية الحالية بالـ Haversine Formula مع عمل Cast للـ double
            double distanceKm = 0.0;
            if (bloodRequest.Latitude.HasValue && bloodRequest.Longitude.HasValue && donor.Latitude.HasValue && donor.Longitude.HasValue)
            {
                distanceKm = CalculateHaversineDistance(
                    (double)bloodRequest.Latitude.Value, (double)bloodRequest.Longitude.Value,
                    (double)donor.Latitude.Value, (double)donor.Longitude.Value
                );
            }

            // 4. فحص سجل الاستجابة وعمل الـ Upsert الذكي
            var responseLog = await _context.RequestResponses
                .Where(rr => rr.RequestId == dto.RequestId && rr.DonorId == donor.Id)
                .FirstOrDefaultAsync();

            if (responseLog != null)
            {
                // إذا كان مقبولاً مسبقاً، نكتفي بالإرجاع مباشرة
                if ((int)responseLog.Status == 2) // 2 = Accepted
                {
                    return await BuildResultAsync(bloodRequest.HospitalId, bloodRequest.Latitude, bloodRequest.Longitude);
                }

                responseLog.Status = (ResponseStatus)2; // 2 = Accepted
                responseLog.RespondedAt = DateTime.UtcNow;
                responseLog.DonorDistanceKm = (decimal)distanceKm; // 🌟 إصلاح الـ Cast هنا
                responseLog.DateOfUpdate = DateTime.UtcNow;
            }
            else
            {
                // إنشاء سجل استجابة وقبول جديد كلياً
                var newResponse = new RequestResponse
                {
                    Id = Guid.NewGuid().ToString(),
                    RequestId = dto.RequestId,
                    DonorId = donor.Id,
                    Status = (ResponseStatus)2, // 2 = Accepted
                    RespondedAt = DateTime.UtcNow,
                    DonorDistanceKm = (decimal)distanceKm, // 🌟 إصلاح الـ Cast هنا
                    DateOfCreattion = DateTime.UtcNow,
                    DateOfUpdate = DateTime.UtcNow
                };
                await _context.RequestResponses.AddAsync(newResponse);
            }

            // حفظ التغييرات المحققة في قاعدة البيانات
            await _context.SaveChangesAsync();

            // 5. بناء وإرجاع الـ DTO محملاً ببيانات الخريطة
            return await BuildResultAsync(bloodRequest.HospitalId, bloodRequest.Latitude, bloodRequest.Longitude);
        }

        // ميثود مساعدة لبناء الـ Result متوافقة مع الـ decimal? الخاص بالـ Database
        private async Task<AcceptRequestResultDTO> BuildResultAsync(string hospitalId, decimal? lat, decimal? lng)
        {
            var hospitalName = await _context.Hospitals
                .AsNoTracking()
                .Where(h => h.Id == hospitalId)
                .Select(h => h.Name)
                .FirstOrDefaultAsync() ?? "Hospital";

            return new AcceptRequestResultDTO
            {
                HospitalName = hospitalName,
                HospitalLatitude = (double?)lat,   // 🌟 تحويل آمن للـ DTO المتوقع double?
                HospitalLongitude = (double?)lng   // 🌟 تحويل آمن للـ DTO المتوقع double?
            };
        }
        public async Task<DonorNearbyRequestListResult> GetNearbyRequestsAsync(string donorUserId, NearbyRequestQueryDTO query)
        {
            // 1. جلب بيانات موقع وفصيلة المتبرع الحالي
            var donor = await _context.Donors
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == donorUserId && !d.IsDeleted);

            if (donor == null)
                throw new UnauthorizedAccessException("Donor record not found.");

            // لو المتبرع مش حاطط إحداثيات موقعه، مش هنعرف نحسب المسافة القريبة
            if (!donor.Latitude.HasValue || !donor.Longitude.HasValue)
            {
                return new DonorNearbyRequestListResult
                {
                    Success = false,
                    Message = "Donor location coordinates are missing. Please enable location on your profile."
                };
            }

            // 2. جلب الفصائل المتوافقة (هذا المتبرع يمنح لمن؟)
            var compatibleRecipientBloodTypeIds = await _context.BloodTypeCompatibilities
                .AsNoTracking()
                .Where(c => c.DonorBloodTypeId == donor.BloodTypeId)
                .Select(c => c.RecipientBloodTypeId)
                .ToListAsync();

            if (!compatibleRecipientBloodTypeIds.Contains(donor.BloodTypeId))
                compatibleRecipientBloodTypeIds.Add(donor.BloodTypeId);

            // 3. جلب كل طلبات الاستغاثة المفتوحة (Status == 1) والمتوافقة طبياً
            var openRequests = await _context.DonationRequests
                .AsNoTracking()
.Where(r => r.IsDeleted == false && r.Status == RequestStatus.Open && compatibleRecipientBloodTypeIds.Contains(r.BloodTypeId)).Select(r => new
                {
                    r.Id,
                    r.UrgencyLevel,
                    r.Note,
                    r.Latitude,
                    r.Longitude,
                    r.DateOfCreattion,
                    HospitalName = _context.Hospitals.Where(h => h.Id == r.HospitalId).Select(h => h.Name).FirstOrDefault(),
                    BloodTypeName = _context.BloodTypes.Where(b => b.Id == r.BloodTypeId).Select(b => b.TypeName).FirstOrDefault() ?? r.BloodTypeId
                })
                .ToListAsync();

            // 4. الفلترة الجغرافية في الـ Memory باستخدام الـ Haversine Formula والترتيب للأقرب
            // 4. الفلترة الجغرافية في الـ Memory باستخدام الـ Haversine Formula والترتيب للأقرب
            var donorLat = (double)donor.Latitude.Value;
            var donorLon = (double)donor.Longitude.Value;

            var nearbyItems = openRequests
                .Select(r => new DonorNearbyRequestDTO
                {
                    RequestId = r.Id,
                    HospitalName = r.HospitalName ?? "Unknown Hospital",
                    BloodTypeName = r.BloodTypeName,
                    UrgencyLevel = r.UrgencyLevel.ToString(),
                    Note = r.Note,
                    Latitude = (double?)r.Latitude,   // ✅ تحويل من decimal? لـ double?
                    Longitude = (double?)r.Longitude, // ✅ تحويل من decimal? لـ double?
                    DateOfCreation = r.DateOfCreattion,
                    // ✅ عملنا Cast صريح لـ (double) لكل باراميتر رايح للدالة عشان الـ Argument Error يختفي
                    DistanceKm = Math.Round(CalculateDistanceKm2(
                        donorLat,
                        donorLon,
                        (double)(r.Latitude ?? 0),
                        (double)(r.Longitude ?? 0)), 2)
                })
                .Where(r => r.DistanceKm <= query.MaxDistanceKm) // الفلترة بالمسافة المطلوبة
                .OrderBy(r => r.DistanceKm) // الترتيب من الأقرب فالأبعد
                .ToList();

            // 5. تطبيق الـ Pagination النظيف على القائمة المصفاة
            var total = nearbyItems.Count;
            var pagedItems = nearbyItems
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

            return new DonorNearbyRequestListResult
            {
                Success = true,
                Message = "Nearby blood donation requests retrieved successfully.",
                Items = pagedItems,
                Total = total,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling((double)total / query.PageSize)
            };
        }

        // ── معادلة الـ Haversine لحساب المسافة بين نقطتين جغرافيتين ──
        private static double CalculateDistanceKm2(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371.0; // نصف قطر الأرض بالكيلومترات
            var dLat = (lat2 - lat1) * Math.PI / 180.0;
            var dLon = (lon2 - lon1) * Math.PI / 180.0;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
        // Haversine Formula لـ مسافات الـ GPS الدقيقة
        private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371.0;
            var dLat = (lat2 - lat1) * Math.PI / 180.0;
            var dLon = (lon2 - lon1) * Math.PI / 180.0;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return Math.Round(R * c, 2);
        }
    }
}