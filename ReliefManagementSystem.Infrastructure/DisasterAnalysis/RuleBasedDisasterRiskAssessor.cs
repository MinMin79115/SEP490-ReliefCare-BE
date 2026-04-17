using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Infrastructure.DisasterAnalysis
{
    public class RuleBasedDisasterRiskAssessor : IDisasterRiskAssessor
    {
        public DisasterRiskAssessment Assess(CurrentWeatherResult weather, DisasterType disasterType, string locationName, string? additionalContext = null)
        {
            var score = weather.WeatherRiskScore;
            var triggerFactors = new List<string>();
            var potentialScenarios = new List<string>();
            var topThreats = new List<string>();
            var confidence = "Medium";
            string? dataLimitationNote = null;

            AddBaseWeatherFactors(weather, triggerFactors, potentialScenarios);

            switch (disasterType)
            {
                case DisasterType.Flood:
                    if (weather.PrecipMm >= 20)
                    {
                        score += 25;
                        triggerFactors.Add($"Lượng mưa rất cao ({weather.PrecipMm:0.##} mm)");
                    }
                    else if (weather.PrecipMm >= 10)
                    {
                        score += 15;
                        triggerFactors.Add($"Mưa đáng kể ({weather.PrecipMm:0.##} mm)");
                    }

                    if (weather.VisibilityKm <= 3)
                    {
                        score += 8;
                        triggerFactors.Add($"Tầm nhìn giảm mạnh ({weather.VisibilityKm:0.##} km)");
                    }

                    topThreats.AddRange(new[] { "Ngập cục bộ", "Chia cắt tuyến đường", "Khó tiếp cận khu dân cư" });
                    potentialScenarios.AddRange(new[]
                    {
                        "Mưa lớn kéo dài có thể gây ngập cục bộ tại vùng trũng thấp.",
                        "Các tuyến đường tiếp cận hiện trường có thể bị ảnh hưởng bởi nước dâng và tầm nhìn kém."
                    });
                    break;

                case DisasterType.Landslide:
                    if (weather.PrecipMm >= 15)
                    {
                        score += 22;
                        triggerFactors.Add($"Mưa lớn làm tăng nguy cơ sạt lở ({weather.PrecipMm:0.##} mm)");
                    }

                    if (weather.Humidity >= 90)
                    {
                        score += 8;
                        triggerFactors.Add($"Độ ẩm rất cao ({weather.Humidity}%)");
                    }

                    topThreats.AddRange(new[] { "Sạt lở taluy", "Chặn đường tiếp cận", "Đất đá trôi xuống khu dân cư" });
                    potentialScenarios.AddRange(new[]
                    {
                        "Địa hình dốc kết hợp mưa lớn có thể gây sạt lở cục bộ.",
                        "Đất đá và bùn trượt có thể cản trở đường cứu hộ hoặc cô lập điểm dân cư."
                    });
                    break;

                case DisasterType.Storm:
                    if (weather.WindKph >= 40)
                    {
                        score += 25;
                        triggerFactors.Add($"Gió mạnh ({weather.WindKph:0.##} km/h)");
                    }
                    else if (weather.WindKph >= 25)
                    {
                        score += 15;
                        triggerFactors.Add($"Gió tăng đáng kể ({weather.WindKph:0.##} km/h)");
                    }

                    if (ContainsAny(weather.Condition, "storm", "thunder", "squall"))
                    {
                        score += 15;
                        triggerFactors.Add($"Mô tả thời tiết bất lợi: {weather.Condition}");
                    }

                    topThreats.AddRange(new[] { "Gió giật", "Đổ cây/cột", "Mất điện cục bộ" });
                    potentialScenarios.AddRange(new[]
                    {
                        "Gió mạnh và mưa có thể gây hư hại mái che, cây xanh và hạ tầng nhẹ.",
                        "Điều kiện thời tiết xấu có thể làm chậm việc điều phối và tiếp cận hiện trường."
                    });
                    break;

                case DisasterType.Fire:
                    if (weather.TemperatureC >= 35)
                    {
                        score += 20;
                        triggerFactors.Add($"Nhiệt độ cao ({weather.TemperatureC:0.##}°C)");
                    }

                    if (weather.Humidity <= 45)
                    {
                        score += 12;
                        triggerFactors.Add($"Độ ẩm thấp ({weather.Humidity}%)");
                    }

                    if (weather.WindKph >= 25)
                    {
                        score += 10;
                        triggerFactors.Add($"Gió có thể làm lửa lan nhanh ({weather.WindKph:0.##} km/h)");
                    }

                    topThreats.AddRange(new[] { "Lan rộng đám cháy", "Khói dày", "Khó sơ tán" });
                    potentialScenarios.AddRange(new[]
                    {
                        "Nhiệt độ cao và gió mạnh có thể làm đám cháy lan nhanh hơn.",
                        "Khói và tầm nhìn kém có thể gây khó khăn cho sơ tán và cứu hộ."
                    });
                    break;

                case DisasterType.Earthquake:
                    score = Math.Min(score, 20);
                    confidence = "Low";
                    dataLimitationNote = "Dữ liệu thời tiết không phải chỉ báo trực tiếp cho động đất; kết quả chỉ phản ánh điều kiện thời tiết có thể ảnh hưởng đến công tác ứng phó nếu sự cố xảy ra.";
                    triggerFactors.Add("Thời tiết chỉ được dùng để đánh giá điều kiện ứng phó, không dự đoán động đất trực tiếp.");
                    topThreats.AddRange(new[] { "Khó tiếp cận hiện trường", "Cản trở cứu hộ do thời tiết", "Giảm tầm nhìn khi ứng cứu" });
                    potentialScenarios.AddRange(new[]
                    {
                        "Nếu xảy ra động đất, điều kiện thời tiết hiện tại có thể làm chậm công tác tiếp cận và sơ tán.",
                        "Kết quả này không nên được hiểu là dự báo xác suất động đất theo nghĩa địa chấn."
                    });
                    break;

                default:
                    topThreats.AddRange(new[] { "Biến động thời tiết bất lợi", "Giảm khả năng tiếp cận", "Nguy cơ vận hành hiện trường" });
                    potentialScenarios.Add("Điều kiện thời tiết hiện tại có thể làm tăng rủi ro vận hành và cứu hộ tại khu vực phân tích.");
                    break;
            }

            score = Math.Clamp(score, 0, 100);

            if (disasterType != DisasterType.Earthquake)
            {
                confidence = score >= 75 ? "High" : score >= 40 ? "Medium" : "Low";
            }

            return new DisasterRiskAssessment
            {
                DisasterType = disasterType,
                LocationName = locationName,
                AdditionalContext = additionalContext,
                WeatherSnapshot = weather,
                OverallRiskScore = score,
                RiskLevel = ToRiskLevel(score),
                AssessmentConfidence = confidence,
                DataLimitationNote = dataLimitationNote,
                TriggerFactors = triggerFactors.Distinct().ToList(),
                PotentialScenarios = potentialScenarios.Distinct().ToList(),
                TopThreats = topThreats.Distinct().ToList()
            };
        }

        private static void AddBaseWeatherFactors(CurrentWeatherResult weather, ICollection<string> triggerFactors, ICollection<string> potentialScenarios)
        {
            if (weather.WindKph >= 25)
            {
                triggerFactors.Add($"Gió tăng ({weather.WindKph:0.##} km/h)");
            }

            if (weather.PrecipMm >= 10)
            {
                triggerFactors.Add($"Mưa hiện tại cao ({weather.PrecipMm:0.##} mm)");
            }

            if (weather.VisibilityKm <= 3)
            {
                triggerFactors.Add($"Tầm nhìn thấp ({weather.VisibilityKm:0.##} km)");
            }

            if (ContainsAny(weather.Condition, "rain", "fog", "storm", "thunder"))
            {
                potentialScenarios.Add($"Điều kiện thời tiết hiện tại ({weather.Condition}) có thể ảnh hưởng trực tiếp tới khả năng di chuyển và đánh giá hiện trường.");
            }
        }

        private static bool ContainsAny(string source, params string[] values)
        {
            var normalized = source?.ToLowerInvariant() ?? string.Empty;
            return values.Any(normalized.Contains);
        }

        private static string ToRiskLevel(int score)
        {
            if (score >= 80) return "Critical";
            if (score >= 60) return "High";
            if (score >= 40) return "Medium";
            return "Low";
        }
    }
}
