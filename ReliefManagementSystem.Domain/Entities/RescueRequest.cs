using ReliefManagementSystem.Domain.Common;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class RescueRequest : Request
    {
        public Guid? CampaignId { get; set; }
        public Campaign? Campaign { get; set; }

        public DisasterType DisasterType { get; set; }
        public RescueRequestType RescueRequestType { get; set; }
        public int? PriorityPoint { get; set; }
        public RescuePriorityLevel RescuePriorityLevel { get; set; }
        public string? Note { get; set; }

        // Weather snapshot at request creation (mainly for Emergency verification)
        public string? WeatherCondition { get; set; }
        public double? WeatherTempC { get; set; }
        public double? WeatherWindKph { get; set; }
        public double? WeatherPrecipMm { get; set; }
        public double? WeatherVisibilityKm { get; set; }
        public int? WeatherRiskScore { get; set; }
        public string? WeatherRiskLevel { get; set; }
        public DateTime? WeatherObservedAt { get; set; }

        public double? StationToRequestDistanceKm { get; set; }
        public int? StationToRequestDurationMinutes { get; set; }
        public int? StationToRequestDistanceMeters { get; set; }
        public int? StationToRequestDurationSeconds { get; set; }

        public RescueRequestStatus RescueRequestStatus { get; set; }
        public DispatchMode DispatchMode { get; set; }
        public ICollection<RescueRequestPriority> RescueRequestPriorities { get; set; } = new List<RescueRequestPriority>();
        public ICollection<RescueOperation> RescueOperations { get; set; } = new List<RescueOperation>();
        public ICollection<RescueBatchItem> RescueBatchItems { get; set; } = new List<RescueBatchItem>();
    }
}
