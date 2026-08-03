// Base.Services/Implementations/MlPredictionService.cs
using Base.DAL.Contexts;
using Base.Services.Interfaces;
using Base.Shared.DTOs.MlDTOs;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace Base.Services.Implementations
{
    public class MlPredictionService : IMlPredictionService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;

        public MlPredictionService(HttpClient httpClient, AppDbContext context)
        {
            _httpClient = httpClient;
            _context = context;

            // ✅ تحديث الرابط الحقيقي لسيرفر الـ ML بتاعكم على Render
            _httpClient.BaseAddress = new Uri("https://ai-7zzl.onrender.com/");
        }

        public async Task<List<DonorPredictionResultDTO>> GetDonorsPredictionsAsync(string? donorId)
        {
            var results = new List<DonorPredictionResultDTO>();
            var now = DateTime.UtcNow;

            // 📊 1. جلب كل المتبرعين من الـ DB بدون أي شروط استبعاد (المتاح وغير المتاح)
            var query = _context.Donors.AsNoTracking();

            // لو الـ Request جاي فيه donorId محدد، هنفلتر بيه هو بس
            if (!string.IsNullOrWhiteSpace(donorId))
            {
                query = query.Where(d => d.Id == donorId);
            }

            var donors = await query.Select(d => new
            {
                d.Id,
                FullName = d.User.FullName,
                BloodTypeName = d.BloodType.TypeName,
                d.LastDonationDate,
                d.DateOfCreattion
            }).ToListAsync();

            if (!donors.Any()) return results;

            // 🚀 2. اللف على المتبرعين وحساب الـ Features وتوقع النتيجة من الـ ML API
            foreach (var donor in donors)
            {
                // حساب إجمالي التبرعات السابقة من جدول الـ History
                int totalDonations = await _context.DonationHistories
                    .CountAsync(dh => dh.DonorId == donor.Id);

                // تحويل الـ DateOnly لـ DateTime لتوحيد نوع الحسبة
                DateTime lastDonation = donor.LastDonationDate.HasValue
                    ? donor.LastDonationDate.Value.ToDateTime(TimeOnly.MinValue)
                    : donor.DateOfCreattion;

                double recencyMonths = Math.Max(0, (now - lastDonation).TotalDays / 30.4);
                double timeMonths = Math.Max(0, (now - donor.DateOfCreattion).TotalDays / 30.4);
                double totalMonetary = totalDonations * 450.0;

                // تجهيز الـ Request Body المطلوبة لسيرفر الـ ML بالملي
                var mlRequest = new MlPredictRequestDTO
                {
                    RecencyMonths = Math.Round(recencyMonths, 1),
                    FrequencyTimes = totalDonations,
                    Monetary = totalMonetary,
                    TimeMonths = Math.Round(timeMonths, 1)
                };

                try
                {
                    // إرسال البيانات لسيرفر الـ ML (POST /predict)
                    var response = await _httpClient.PostAsJsonAsync("predict", mlRequest);

                    if (response.IsSuccessStatusCode)
                    {
                        var mlResult = await response.Content.ReadFromJsonAsync<MlPredictResponseDTO>();
                        if (mlResult != null)
                        {
                            results.Add(new DonorPredictionResultDTO
                            {
                                DonorId = donor.Id,
                                FullName = donor.FullName,
                                BloodTypeName = donor.BloodTypeName,
                                Probability = mlResult.Probability,
                                WillDonate = mlResult.WillDonate,
                                Threshold = mlResult.Threshold,
                                Recommendation = mlResult.WillDonate == 1
                                    ? "High probability donor. Highly recommended for emergency calls."
                                    : "Low probability donor. Keep as backup option."
                            });
                        }
                    }
                }
                catch
                {
                    // لو حصل هنج لأي يوزر السيرفر بيكمل عادي وميقفش
                    continue;
                }
            }

            // ترتيب النتايج تنازلياً حسب الأعلى احتمالية
            return results.OrderByDescending(r => r.Probability).ToList();
        }
        // ضيف الميثود دي جوه كلاس MlPredictionService
        public async Task<MlHealthCheckResponseDTO?> CheckMlHealthAsync()
        {
            try
            {
                // 🚀 ضرب إندبوينت الـ GET health على سيرفر Render الحقيقي
                var response = await _httpClient.GetAsync("health");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<MlHealthCheckResponseDTO>();
                }
                return null;
            }
            catch
            {
                // لو السيرفر واقع تماماً أو بياخد وقت يقوم
                return null;
            }
        }
    }
}