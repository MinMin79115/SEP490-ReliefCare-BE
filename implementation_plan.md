# Implement Last-mile Distribution (Relief Delivery)

This plan outlines the required changes to implement support for tracking the final step of the relief supply chain: handing over the goods to the people in need ([ReliefRequest](file:///c:/Users/k10n10/source/repos/SEP490-ReliefCare-BE/ReliefManagementSystem.Domain/Entities/ReliefRequest.cs#11-22)).

## Approach
Currently, [SupplyAllocation](file:///c:/Users/k10n10/source/repos/SEP490-ReliefCare-BE/ReliefManagementSystem.Domain/Entities/SupplyAllocation.cs#10-25) is used to withdraw items from the Inventory for a [Campaign](file:///c:/Users/k10n10/source/repos/SEP490-ReliefCare-BE/ReliefManagementSystem.Domain/Entities/Campaign.cs#10-42). However, we lack the entity that records the actual delivery point: *Volunteer A gave 1 box of noodles to Household B (ReliefRequest)*.

We will create two new entities: `ReliefDistributionRecord` and `ReliefDistributionRecordItem`.

## 1. New Entities

### `ReliefDistributionRecord`
This entity represents a single "handover" receipt between a campaign team/volunteer and a verified relief request.

**Properties:**
- `DistributionId` (Guid, PK)
- `CampaignId` (Guid, FK to Campaign)
- `ReliefRequestId` (Guid, FK to ReliefRequest - The recipient/household receiving the goods)
- `VolunteerProfileId` (Guid?, FK to VolunteerProfile - The volunteer who handed over the goods)
- `MemberTaskId` (Guid?, FK to MemberTask - Optional link if this was a specific task assigned)
- `DistributedAt` (DateTime)
- `RecipientSignatureUrl` (string? - Proof of receipt, e.g. photo/signature uploaded to cloud)
- `DeliveryLocation` (string? - GPS or Address where it was handed over)
- `Notes` (string?)

### `ReliefDistributionRecordItem`
The specific items given in this handover.

**Properties:**
- `DistributionItemId` (Guid, PK)
- `DistributionId` (Guid, FK to ReliefDistributionRecord)
- `SupplyItemId` (Guid, FK to SupplyItem)
- `Quantity` (int)

## 2. Navigation Properties

### [ReliefManagementSystem.Domain\Entities\ReliefRequest.cs](file:///c:/Users/k10n10/source/repos/SEP490-ReliefCare-BE/ReliefManagementSystem.Domain/Entities/ReliefRequest.cs)
Add a collection to see all distributions made to this request.
```csharp
public ICollection<ReliefDistributionRecord> Distributions { get; set; } = new List<ReliefDistributionRecord>();
```

### [ReliefManagementSystem.Domain\Entities\Campaign.cs](file:///c:/Users/k10n10/source/repos/SEP490-ReliefCare-BE/ReliefManagementSystem.Domain/Entities/Campaign.cs)
```csharp
public ICollection<ReliefDistributionRecord> Distributions { get; set; } = new List<ReliefDistributionRecord>();
```

### [ReliefManagementSystem.Domain\Entities\VolunteerProfile.cs](file:///c:/Users/k10n10/source/repos/SEP490-ReliefCare-BE/ReliefManagementSystem.Domain/Entities/VolunteerProfile.cs)
```csharp
public ICollection<ReliefDistributionRecord> DistributionsPerformed { get; set; } = new List<ReliefDistributionRecord>();
```

### [ReliefManagementSystem.Domain\Entities\SupplyItem.cs](file:///c:/Users/k10n10/source/repos/SEP490-ReliefCare-BE/ReliefManagementSystem.Domain/Entities/SupplyItem.cs)
```csharp
public ICollection<ReliefDistributionRecordItem> DistributedItems { get; set; } = new List<ReliefDistributionRecordItem>();
```

## 3. DbContext Configuration ([ApplicationDbContext.cs](file:///c:/Users/k10n10/source/repos/SEP490-ReliefCare-BE/ReliefManagementSystem.Infrastructure/Data/ApplicationDbContext.cs))
- Add DbSets: `ReliefDistributionRecords` and `ReliefDistributionRecordItems`.
- Configure One-to-Many relationships:
  - `ReliefDistributionRecord` -> [ReliefRequest](file:///c:/Users/k10n10/source/repos/SEP490-ReliefCare-BE/ReliefManagementSystem.Domain/Entities/ReliefRequest.cs#11-22) (DeleteBehavior.SetNull or Cascade depending if we want to keep records after a request is deleted. Usually Cascade).
  - `ReliefDistributionRecord` -> [Campaign](file:///c:/Users/k10n10/source/repos/SEP490-ReliefCare-BE/ReliefManagementSystem.Domain/Entities/Campaign.cs#10-42) (DeleteBehavior.Restrict)
  - `ReliefDistributionRecord` -> `VolunteerProfile` (DeleteBehavior.SetNull)
  - `ReliefDistributionRecordItem` -> [SupplyItem](file:///c:/Users/k10n10/source/repos/SEP490-ReliefCare-BE/ReliefManagementSystem.Domain/Entities/SupplyItem.cs#10-32) (DeleteBehavior.Restrict)

## Verification Plan
1. Apply the changes.
2. Ensure EF Core mapping has no cyclic cascade paths.
3. Build the project.
