# Relief Planning Handoff - Backend

## Mục tiêu nghiệp vụ
- Bổ sung lớp **kế hoạch cứu trợ dự kiến** cho campaign cứu trợ.
- Hỗ trợ **hộ bị cô lập** tốt hơn thay vì chỉ có `IsIsolated`.
- Có khả năng **dự đoán nhu cầu theo khu vực**.
- Tính/gợi ý:
  - số đội cần
  - nhân lực cần
  - vật lực cần
  - thiết bị cần như xuồng, áo phao
- Cho phép **gán người / gán việc ngay từ kế hoạch** bằng cách tái sử dụng task hiện có.

---

## Hiện trạng code đã có

### Controller chính
- `ReliefManagementSystem.API/Controllers/ReliefDistributionController.cs`

### Service chính
- `ReliefManagementSystem.Application/Interface/IReliefDistributionService.cs`
- `ReliefManagementSystem.Application/Services/ReliefDistributionService.cs`

### DTO hiện có
- `ReliefManagementSystem.Application/Features/Relief/DTOs/Request/ReliefDistributionRequests.cs`
- `ReliefManagementSystem.Application/Features/Relief/DTOs/Response/ReliefDistributionResponses.cs`

### Entity hiện có
- `ReliefManagementSystem.Domain/Entities/CampaignHousehold.cs`
- `ReliefManagementSystem.Domain/Entities/DistributionPoint.cs`
- `ReliefManagementSystem.Domain/Entities/HouseholdDelivery.cs`
- `ReliefManagementSystem.Domain/Entities/Campaign.cs`
- `ReliefManagementSystem.Domain/Entities/CampaignTeam.cs`
- `ReliefManagementSystem.Domain/Entities/CampaignVehicle.cs`
- `ReliefManagementSystem.Domain/Entities/MemberTask.cs`

### Nền sẵn có
- Có danh sách hộ theo campaign
- Có `IsIsolated`
- Có distribution point
- Có delivery/checklist
- Có package + shortage request
- Có campaign task + member task

### Chưa có
- `ReliefPlan`
- forecast theo khu vực
- resource estimation
- plan assignment
- API summary kế hoạch cứu trợ

---

## Đề xuất hướng làm an toàn cho lần sau

## Phase 1 - Không tạo domain quá nặng, tính động từ dữ liệu hiện có
Mục tiêu: làm nhanh, ít migration, đủ cho mobile hiển thị kế hoạch.

### 1. Mở rộng response / summary API
Thêm endpoint mới trong `ReliefDistributionController`:

```csharp
GET api/relief/campaigns/{campaignId}/plan-summary
```

### 2. Thêm method trong interface
Trong `IReliefDistributionService.cs` thêm:

```csharp
Task<ReliefCampaignPlanSummaryResponse> GetCampaignPlanSummaryAsync(
    Guid campaignId,
    CancellationToken cancellationToken = default);
```

### 3. Thêm response DTO mới trong `ReliefDistributionResponses.cs`
Đề xuất:

```csharp
public class ReliefCampaignPlanSummaryResponse
{
    public Guid CampaignId { get; set; }
    public int TotalHouseholds { get; set; }
    public int IsolatedHouseholds { get; set; }
    public int TotalPopulation { get; set; }
    public int DistributionPointCount { get; set; }
    public int PendingHouseholds { get; set; }
    public int SuggestedTeamCount { get; set; }
    public int EstimatedReliefPersonnel { get; set; }
    public int EstimatedLocalVolunteers { get; set; }
    public int EstimatedBoatCount { get; set; }
    public int EstimatedLifeJacketCount { get; set; }
    public List<ReliefPlanAreaSummaryResponse> Areas { get; set; } = [];
    public List<IsolatedHouseholdPlanItemResponse> IsolatedHouseholds { get; set; } = [];
    public List<DistributionPointPlanSummaryResponse> DistributionPoints { get; set; } = [];
    public List<ReliefResourceRequirementResponse> ResourceRequirements { get; set; } = [];
}
```

```csharp
public class ReliefPlanAreaSummaryResponse
{
    public string AreaName { get; set; } = string.Empty;
    public Guid? LocationId { get; set; }
    public int HouseholdCount { get; set; }
    public int IsolatedHouseholdCount { get; set; }
    public int Population { get; set; }
    public int PendingHouseholds { get; set; }
    public int SuggestedTeamCount { get; set; }
    public int EstimatedPackages { get; set; }
    public int EstimatedBoatCount { get; set; }
    public int EstimatedLifeJacketCount { get; set; }
}
```

```csharp
public class IsolatedHouseholdPlanItemResponse
{
    public Guid CampaignHouseholdId { get; set; }
    public string HouseholdCode { get; set; } = string.Empty;
    public string HeadOfHouseholdName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public Guid? LocationId { get; set; }
    public int HouseholdSize { get; set; }
    public string PriorityLabel { get; set; } = string.Empty;
    public string SuggestedSupportMode { get; set; } = string.Empty;
    public int EstimatedReliefPersonnel { get; set; }
    public int EstimatedBoatCount { get; set; }
    public int EstimatedLifeJacketCount { get; set; }
    public string? CampaignTeamName { get; set; }
}
```

```csharp
public class DistributionPointPlanSummaryResponse
{
    public Guid DistributionPointId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int AssignedHouseholdCount { get; set; }
    public int PendingDeliveryCount { get; set; }
    public int SuggestedPersonnelCount { get; set; }
    public int SuggestedLocalVolunteerCount { get; set; }
}
```

```csharp
public class ReliefResourceRequirementResponse
{
    public string ResourceType { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public int EstimatedQuantity { get; set; }
    public string? Notes { get; set; }
}
```

---

## Rule tính toán gợi ý cho Phase 1
Không cần ML, chỉ cần heuristic để điều phối viên dùng tạm.

### Toàn campaign
- `TotalHouseholds` = tổng số hộ
- `IsolatedHouseholds` = số hộ `IsIsolated == true`
- `TotalPopulation` = tổng `HouseholdSize`
- `DistributionPointCount` = số điểm phát
- `PendingHouseholds` = số hộ chưa hoàn tất

### Gợi ý đội/người
- `SuggestedTeamCount = ceil(TotalHouseholds / 50.0)`
- `EstimatedReliefPersonnel = max(SuggestedTeamCount * 4, ceil(TotalPopulation / 25.0))`
- `EstimatedLocalVolunteers = max(1, ceil(IsolatedHouseholds / 10.0))`

### Gợi ý thiết bị
- `EstimatedBoatCount = ceil(IsolatedHouseholds / 8.0)`
- `EstimatedLifeJacketCount = EstimatedReliefPersonnel + EstimatedLocalVolunteers + (EstimatedBoatCount * 2)`

### Theo khu vực
Nhóm theo:
1. `LocationId` nếu có
2. fallback theo `Address`
3. cuối cùng fallback `"Chưa phân khu vực"`

### Hộ cô lập
Priority label gợi ý:
- `Khẩn cấp`: hộ cô lập và `HouseholdSize >= 5`
- `Ưu tiên cao`: hộ cô lập và `HouseholdSize 3-4`
- `Ưu tiên`: còn lại

Suggested support mode:
- nếu `IsIsolated == true` => `Giao tận nơi`
- nếu không => `Điểm phát`

---

## Phase 1b - Mở rộng household nhẹ, nếu chấp nhận migration
Nếu muốn hỗ trợ hộ cô lập tốt hơn, thêm các field vào `CampaignHousehold`:

```csharp
public string? IsolationSeverity { get; set; }
public string? AccessConstraints { get; set; }
public bool RequiresLocalGuide { get; set; }
public int? PriorityScore { get; set; }
public DateTime? LastVerifiedAt { get; set; }
```

Và expose trong request/response household.

Nếu chưa muốn migration ngay, có thể để Phase 2.

---

## Phase 2 - Kế hoạch thật sự trong DB
Chỉ làm khi cần workflow draft/approve/save nhiều phiên.

### Entity mới đề xuất
- `ReliefPlan`
- `ReliefPlanArea`
- `ReliefPlanResourceRequirement`
- `ReliefPlanAssignment`
- `ReliefPlanDistributionPoint`

### Quan hệ
- 1 campaign có nhiều plan
- 1 plan có nhiều area
- 1 area có nhiều resource requirement
- 1 area có nhiều assignment
- 1 area có nhiều distribution point plan item

### Khi đó mới nên thêm API
- create/update/approve plan
- assign team/person into plan
- materialize plan -> task / point / delivery

---

## Tái sử dụng task hiện có
Không nên làm hệ phân công mới riêng ở Phase 1.

Nên tái dùng:
- `CampaignTask`
- `MemberTask`

Hướng mở rộng về sau:
- task title gợi ý từ plan area
- task description chứa khu vực / điểm phát / hộ cô lập
- deep link từ mobile plan -> allocate task

Về sau có thể thêm reference:
- `ReliefPlanId`
- `ReliefPlanAreaId`

---

## File nên sửa ở lần implement tiếp theo

### Chắc chắn sửa
- `ReliefManagementSystem.API/Controllers/ReliefDistributionController.cs`
- `ReliefManagementSystem.Application/Interface/IReliefDistributionService.cs`
- `ReliefManagementSystem.Application/Services/ReliefDistributionService.cs`
- `ReliefManagementSystem.Application/Features/Relief/DTOs/Response/ReliefDistributionResponses.cs`

### Có thể sửa thêm
- `ReliefManagementSystem.Application/Features/Relief/DTOs/Request/ReliefDistributionRequests.cs`
- `ReliefManagementSystem.Domain/Entities/CampaignHousehold.cs`
- `ReliefManagementSystem.Domain/Entities/DistributionPoint.cs`

---

## Lưu ý kỹ thuật
- Ưu tiên **không tạo migration lớn** nếu mục tiêu chỉ là hiển thị kế hoạch cho mobile.
- Tận dụng query households + distribution points + deliveries để tính summary động.
- Nếu `ReliefDistributionService.cs` đã quá lớn, có thể tạo helper/private method trước khi tách service riêng.
- Đảm bảo plan summary chỉ áp dụng cho campaign type `Relief`.

---

## Đầu ra backend tối thiểu cần có cho mobile
Mobile chỉ cần 1 endpoint summary là đã đi tiếp được:

```json
{
  "campaignId": "...",
  "totalHouseholds": 120,
  "isolatedHouseholds": 18,
  "totalPopulation": 540,
  "distributionPointCount": 4,
  "pendingHouseholds": 67,
  "suggestedTeamCount": 3,
  "estimatedReliefPersonnel": 14,
  "estimatedLocalVolunteers": 2,
  "estimatedBoatCount": 3,
  "estimatedLifeJacketCount": 22,
  "areas": [],
  "isolatedHouseholds": [],
  "distributionPoints": [],
  "resourceRequirements": []
}
```

---

## Việc nên làm đầu tiên khi quay lại
1. Thêm response DTO plan summary
2. Thêm method service + controller endpoint
3. Implement tính toán summary động từ households/distribution points
4. Test response bằng swagger/postman
5. Sau đó mới nối mobile
