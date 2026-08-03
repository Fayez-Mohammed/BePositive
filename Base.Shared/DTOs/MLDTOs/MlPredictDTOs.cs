// Base.Shared/DTOs/MlDTOs/BloodDonationMlDTOs.cs
using System.Text.Json.Serialization;

namespace Base.Shared.DTOs.MlDTOs
{
    public class MlPredictRequestDTO
    {
        [JsonPropertyName("recency_months")]
        public double RecencyMonths { get; set; } // الشهور من آخر تبرع

        [JsonPropertyName("frequency_times")]
        public int FrequencyTimes { get; set; } // إجمالي عدد التبرعات

        [JsonPropertyName("monetary")]
        public double Monetary { get; set; } // إجمالي الدم المتبرع به بالـ c.c.

        [JsonPropertyName("time_months")]
        public double TimeMonths { get; set; } // الشهور من أول تبرع عمله
    }

    public class MlPredictResponseDTO
    {
        [JsonPropertyName("probability")]
        public double Probability { get; set; } // نسبة احتمال موافقته (0.0 إلى 1.0)

        [JsonPropertyName("will_donate")]
        public int WillDonate { get; set; } // 1 يعني هيتبرع، 0 مش هيتبرع

        [JsonPropertyName("threshold")]
        public double Threshold { get; set; } // النسبة الحدّية المعتمدة
    }


    // ── 2️⃣ الـ DTOs الجديدة الخاصة بالـ Controller والـ Flutter (تم دمجها هنا) ──
    public class MlPredictionQueryDTO
    {
        public string? DonorId { get; set; } // لو مبعوث يحسب لواحد بس، لو null يحسب للكل
    }

    public class DonorPredictionResultDTO
    {
        public string DonorId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string BloodTypeName { get; set; } = null!;
        public double Probability { get; set; } // نسبة التوقع
        public int WillDonate { get; set; } // 1 أو 0
        public double Threshold { get; set; } // النسبة الحدية
        public string Recommendation { get; set; } = null!; // نص التوصية الطبية
    }


    // ضيفه جوه ملف Base.Shared/DTOs/MlDTOs/BloodDonationMlDTOs.cs

    public class MlHealthCheckResponseDTO
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("model_loaded")]
        public bool ModelLoaded { get; set; }

        [JsonPropertyName("best_threshold")]
        public double BestThreshold { get; set; }

        [JsonPropertyName("feature_count")]
        public int FeatureCount { get; set; }

        [JsonPropertyName("feature_columns")]
        public List<string> FeatureColumns { get; set; } = new();
    }


}