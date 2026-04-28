# Relief API Usage Guide

## Mục tiêu tài liệu
- Hướng dẫn sử dụng các endpoint backend của luồng cứu trợ.
- Tách theo 3 role nghiệp vụ:
  - `Coordinator` - thao tác chủ yếu trên web
  - `Team Leader` - thao tác trên mobile
  - `Volunteer` - thao tác trên mobile
- Mô tả đúng flow thực tế đã phân tích và code.
- Giải thích khi nào gọi endpoint nào, theo thứ tự nào.

---

## 1. Tổng quan vai trò và thiết bị sử dụng

## Coordinator - dùng web
Coordinator chịu trách nhiệm:
- import hộ dân từ địa phương
- chuẩn hóa thông tin hộ dân
- xem plan summary
- gán team cho hộ dân
- gán team cho hộ cô lập
- batch assign hộ cô lập cho team
- tạo điểm phát
- tạo gói cứu trợ
- điều phối phương tiện cho team
- duyệt shortage request

## Team Leader - dùng mobile
Team Leader là thành viên của team, nhưng có thêm quyền điều phối nội bộ:
- xem plan summary
- xem team worklist
- tạo task chính cho campaign/team
- tạo subtask
- chia deliveries thành line / tổ / cụm bằng subtask
- theo dõi member task deliveries
- hỗ trợ complete delivery nếu cần

## Volunteer - dùng mobile
Volunteer thực thi ngoài hiện trường:
- xem task/subtask của mình
- xem deliveries được giao cho mình
- xem team worklist
- cập nhật trạng thái subtask
- complete delivery
- gửi proof

---

## 2. Hai nhánh nghiệp vụ chính

## Nhánh A - PickupAtPoint
Áp dụng khi:
- hộ dân nhận tại điểm phát
- Coordinator gán team + điểm phát
- Team Leader chia line / bàn / nhóm phát hàng

## Nhánh B - DoorToDoor
Áp dụng khi:
- hộ dân bị cô lập
- cần giao tận nơi
- có thể cần xuồng/ghe/xe/driver/người dẫn đường
- Coordinator gán team và phương tiện
- Team Leader chia theo tổ công tác

---

## 3. Quy ước gọi API

Base route chính cho relief:

```text
/api/relief/campaigns/{campaignId}
```

Base route cho campaign tasks:

```text
/api/campaigns
```

Lưu ý:
- Tất cả endpoint dưới đây giả định đã có JWT hợp lệ.
- `campaignId`, `campaignTaskId`, `campaignHouseholdId`, `householdDeliveryId`, `campaignTeamId`, `memberTaskId`, `memberTaskDeliveryId`, `campaignVehicleId` đều là `Guid`.

---

## 4. Flow Coordinator - web

## Bước 1 - Import households từ địa phương

### Endpoint
`POST /api/relief/campaigns/{campaignId}/households/import`

### Khi dùng
- bắt đầu chiến dịch cứu trợ
- nhận file/danh sách hộ từ địa phương

### Body mẫu
```json
{
  "households": [
    {
      "householdCode": "HH-001",
      "headOfHouseholdName": "Nguyen Van A",
      "contactPhone": "0909123456",
      "address": "Ap 1, Xa Binh Loi",
      "latitude": 10.8123,
      "longitude": 106.7123,
      "householdSize": 5,
      "isIsolated": true,
      "floodSeverityLevel": 7,
      "isolationSeverityLevel": 8,
      "requiresBoat": true,
      "requiresLocalGuide": true,
      "deliveryMode": 0
    },
    {
      "householdCode": "HH-002",
      "headOfHouseholdName": "Tran Thi B",
      "contactPhone": "0911222333",
      "address": "Truong tieu hoc Xa Binh Loi",
      "latitude": 10.8111,
      "longitude": 106.7188,
      "householdSize": 4,
      "isIsolated": false,
      "floodSeverityLevel": 2,
      "isolationSeverityLevel": 0,
      "requiresBoat": false,
      "requiresLocalGuide": false,
      "deliveryMode": 1
    }
  ]
}
```

### Ý nghĩa field
- `deliveryMode = 0` -> `DoorToDoor`
- `deliveryMode = 1` -> `PickupAtPoint`
- `floodSeverityLevel`: mức ngập 0-10
- `isolationSeverityLevel`: mức cô lập 0-10
- `requiresBoat`: có cần xuồng/ghe không
- `requiresLocalGuide`: có cần người địa phương dẫn đường không

---

## Bước 2 - Xem và lọc households

### Endpoint
`GET /api/relief/campaigns/{campaignId}/households`

### Query hỗ trợ
- `PageIndex`
- `PageSize`
- `Search`
- `Status`
- `DeliveryMode`
- `DistributionPointId`
- `CampaignTeamId`
- `IsIsolated`
- `IsAssigned`
- `RequiresBoat`
- `RequiresLocalGuide`
- `MinFloodSeverityLevel`
- `MinIsolationSeverityLevel`
- `HasCoordinates`

### Ví dụ gọi
```text
GET /api/relief/campaigns/{campaignId}/households?PageIndex=1&PageSize=20&IsIsolated=true&RequiresBoat=true&MinFloodSeverityLevel=5
```

### Khi dùng
- lọc hộ cô lập
- lọc hộ cần xuồng
- lọc hộ chưa gán team
- rà lại dữ liệu đầu vào

---

## Bước 3 - Cập nhật household

### Endpoint
`PATCH /api/relief/campaigns/{campaignId}/households/{campaignHouseholdId}`

### Khi dùng
- sửa lại địa chỉ, số người, lat/lng
- cập nhật mức ngập, mức cô lập
- cập nhật cần xuồng / dẫn đường

### Body mẫu
```json
{
  "address": "Ap 1, Xa Binh Loi, Gan UBND xa",
  "latitude": 10.8129,
  "longitude": 106.7129,
  "householdSize": 6,
  "isIsolated": true,
  "floodSeverityLevel": 8,
  "isolationSeverityLevel": 9,
  "requiresBoat": true,
  "requiresLocalGuide": true,
  "notes": "Duong bo bi chia cat"
}
```

---

## Bước 4 - Xem plan summary trước khi điều phối

### Endpoint
`GET /api/relief/campaigns/{campaignId}/plan-summary`

### Khi dùng
- trước khi gán team
- trước khi điều phối phương tiện
- trước khi quyết định mở điểm phát hoặc chia tổ cơ động

### Dữ liệu chính trả về
- tổng số hộ
- số hộ cô lập
- tổng nhân khẩu
- số đội gợi ý
- số xuồng, áo phao, nhân lực gợi ý
- area summaries theo cụm địa lý
- mode gợi ý:
  - ưu tiên điểm phát
  - ưu tiên đội cơ động

### Quyết định Coordinator cần rút ra
- khu vực nào nên mở điểm phát
- khu vực nào nên đi door-to-door
- team nào cần phương tiện nào

---

## Bước 5A - Nhánh PickupAtPoint: tạo điểm phát

### Endpoint
`POST /api/relief/campaigns/{campaignId}/distribution-points`

### Body mẫu
```json
{
  "name": "Diem phat Truong TH Binh Loi",
  "reliefStationId": "00000000-0000-0000-0000-000000000001",
  "campaignTeamId": "00000000-0000-0000-0000-000000000002",
  "address": "Truong TH Binh Loi",
  "latitude": 10.8111,
  "longitude": 106.7188,
  "deliveryMode": 1,
  "startsAt": "2026-04-28T08:00:00Z",
  "endsAt": "2026-04-28T17:00:00Z",
  "isActive": true
}
```

### Sau đó có thể
- `GET /distribution-points`
- `PATCH /distribution-points/{distributionPointId}`
- `DELETE /distribution-points/{distributionPointId}`

---

## Bước 5B - Nhánh PickupAtPoint: gán household vào team và điểm phát

### Endpoint
`PATCH /api/relief/campaigns/{campaignId}/households/{campaignHouseholdId}/assign`

### Body mẫu
```json
{
  "deliveryMode": 1,
  "distributionPointId": "00000000-0000-0000-0000-000000000010",
  "campaignTeamId": "00000000-0000-0000-0000-000000000002",
  "reliefPackageDefinitionId": "00000000-0000-0000-0000-000000000020",
  "scheduledAt": "2026-04-28T09:00:00Z",
  "notes": "Nhan tai diem phat truong hoc"
}
```

### Kết quả
- household được gán team
- household được gán điểm phát
- system tạo/cập nhật `HouseholdDelivery`

---

## Bước 6B - Nhánh DoorToDoor: gán team cho 1 hộ cô lập

### Endpoint
`PATCH /api/relief/campaigns/{campaignId}/households/{campaignHouseholdId}/assign-isolated-team`

### Body mẫu
```json
{
  "campaignTeamId": "00000000-0000-0000-0000-000000000101",
  "reliefPackageDefinitionId": "00000000-0000-0000-0000-000000000020",
  "scheduledAt": "2026-04-28T06:30:00Z",
  "keepDoorToDoor": true,
  "notes": "Tiep can bang xuong"
}
```

### Khi dùng
- gán riêng lẻ 1 hộ cô lập
- hộ đặc biệt cần xử lý riêng

---

## Bước 6C - Nhánh DoorToDoor: batch assign nhiều hộ cô lập

### Endpoint
`PATCH /api/relief/campaigns/{campaignId}/households/isolated-team/bulk-assign`

### Body mẫu
```json
{
  "campaignHouseholdIds": [
    "00000000-0000-0000-0000-000000000201",
    "00000000-0000-0000-0000-000000000202",
    "00000000-0000-0000-0000-000000000203"
  ],
  "campaignTeamId": "00000000-0000-0000-0000-000000000101",
  "reliefPackageDefinitionId": "00000000-0000-0000-0000-000000000020",
  "scheduledAt": "2026-04-28T06:30:00Z",
  "keepDoorToDoor": true,
  "notes": "Cum ho co lap kenh so 2"
}
```

### Khi dùng
- gán 1 team cho cả cụm hộ cô lập
- đúng flow thực tế của Coordinator

---

## Bước 7 - Điều phối phương tiện cho team relief

### Endpoint
`POST /api/campaigns/{campaignId}/teams/{campaignTeamId}/vehicles`

### Body mẫu
```json
{
  "vehicleId": "00000000-0000-0000-0000-000000000301",
  "campaignTeamId": "00000000-0000-0000-0000-000000000101",
  "assignedDriverId": "00000000-0000-0000-0000-000000000401",
  "startDate": "2026-04-28T06:00:00Z",
  "endDate": "2026-04-28T18:00:00Z",
  "status": 1,
  "note": "Xuồng máy cho tổ co lap kenh so 2"
}
```

### Tra cứu / cập nhật / xóa
- `GET /api/campaigns/{campaignId}/vehicles`
- `PATCH /api/campaigns/{campaignId}/vehicles/{campaignVehicleId}`
- `DELETE /api/campaigns/{campaignId}/vehicles/{campaignVehicleId}`

### Khi dùng
- sau khi plan summary xác định cần phương tiện
- trước khi Team Leader bắt đầu chia tổ công tác door-to-door

---

## Bước 8 - Gói cứu trợ / lắp gói / shortage

### CRUD package
- `POST /api/relief/campaigns/{campaignId}/packages`
- `GET /api/relief/campaigns/{campaignId}/packages`
- `PATCH /api/relief/campaigns/{campaignId}/packages/{reliefPackageDefinitionId}`
- `DELETE /api/relief/campaigns/{campaignId}/packages/{reliefPackageDefinitionId}`

### Availability / assemble
- `GET /api/relief/campaigns/{campaignId}/packages/{reliefPackageDefinitionId}/assembly-availability?reliefStationId=...&inventoryId=...`
- `POST /api/relief/campaigns/{campaignId}/packages/{reliefPackageDefinitionId}/assemble`

### Shortage request approval
- `GET /api/relief/campaigns/{campaignId}/shortage-requests`
- `PATCH /api/relief/campaigns/{campaignId}/shortage-requests/{shortageRequestId}/approve`
- `PATCH /api/relief/campaigns/{campaignId}/shortage-requests/{shortageRequestId}/reject`

---

## 5. Flow Team Leader - mobile

## Bước 1 - Xem team worklist

### Endpoint
`GET /api/relief/campaigns/{campaignId}/team-worklist`

### Query thường dùng
- `CampaignTeamId`
- `IncludePendingOnly=true`
- `PrioritizeIsolated=true`
- `RequiresBoat=true`
- `MinFloodSeverityLevel=5`

### Ví dụ
```text
GET /api/relief/campaigns/{campaignId}/team-worklist?CampaignTeamId={campaignTeamId}&IncludePendingOnly=true&PrioritizeIsolated=true
```

### Dùng để
- biết team mình hôm nay phải đi đâu
- biết hộ nào cần xuồng / cần dẫn đường
- biết line nào hoặc tổ nào cần ưu tiên

---

## Bước 2 - Tạo task chính

### Endpoint
`POST /api/campaigns/{campaignId}/tasks`

### Ví dụ PickupAtPoint
```json
{
  "campaignTeamId": "00000000-0000-0000-0000-000000000002",
  "title": "Phat hang tai diem phat Truong TH Binh Loi",
  "description": "Task chinh cho team phu trach diem phat",
  "startDate": "2026-04-28T07:00:00Z",
  "dueDate": "2026-04-28T17:30:00Z",
  "priority": 2
}
```

### Ví dụ DoorToDoor
```json
{
  "campaignTeamId": "00000000-0000-0000-0000-000000000101",
  "title": "Phat hang toi cum ho co lap kenh so 2",
  "description": "Team di tiep can bang xuong va phat hang tan noi",
  "startDate": "2026-04-28T06:00:00Z",
  "dueDate": "2026-04-28T18:00:00Z",
  "priority": 3
}
```

---

## Bước 3 - Tạo subtask thủ công theo vai trò

### Endpoint
`POST /api/campaigns/tasks/{campaignTaskId}/members`

### Body mẫu
```json
{
  "volunteerProfileId": "00000000-0000-0000-0000-000000000501",
  "subTaskTitle": "Hau can line 1",
  "taskNote": "Chuan bi goi hang va ho tro ban phat"
}
```

### Bulk gán nhiều member
`POST /api/campaigns/tasks/{campaignTaskId}/members/bulk`

---

## Bước 4 - Tạo subtask từ deliveries của team

### Endpoint
`POST /api/campaigns/tasks/{campaignTaskId}/members/from-households`

### Khi dùng
- muốn tạo 1 subtask chứa một nhóm deliveries cho 1 member

### Body mẫu
```json
{
  "volunteerProfileId": "00000000-0000-0000-0000-000000000501",
  "householdDeliveryIds": [
    "00000000-0000-0000-0000-000000000601",
    "00000000-0000-0000-0000-000000000602",
    "00000000-0000-0000-0000-000000000603"
  ],
  "subTaskTitle": "Line 1 phat hang",
  "taskNote": "Phu trach 3 ho dau tien"
}
```

---

## Bước 5 - Batch chia deliveries thành nhiều line / nhiều tổ

### Endpoint
`POST /api/campaigns/tasks/{campaignTaskId}/members/batch-from-deliveries`

### Khi dùng
- chia 60 hộ tại điểm phát thành 3 line
- chia cụm hộ cô lập cho nhiều tổ

### Body mẫu - chia 60 hộ thành 3 line
```json
{
  "assignments": [
    {
      "volunteerProfileId": "00000000-0000-0000-0000-000000000701",
      "householdDeliveryIds": [
        "00000000-0000-0000-0000-000000000801",
        "00000000-0000-0000-0000-000000000802"
      ],
      "subTaskTitle": "Phat hang",
      "taskNote": "Line 1",
      "lineName": "Line 1"
    },
    {
      "volunteerProfileId": "00000000-0000-0000-0000-000000000702",
      "householdDeliveryIds": [
        "00000000-0000-0000-0000-000000000803",
        "00000000-0000-0000-0000-000000000804"
      ],
      "subTaskTitle": "Phat hang",
      "taskNote": "Line 2",
      "lineName": "Line 2"
    },
    {
      "volunteerProfileId": "00000000-0000-0000-0000-000000000703",
      "householdDeliveryIds": [
        "00000000-0000-0000-0000-000000000805",
        "00000000-0000-0000-0000-000000000806"
      ],
      "subTaskTitle": "Phat hang",
      "taskNote": "Line 3",
      "lineName": "Line 3"
    }
  ]
}
```

### Body mẫu - chia cụm hộ cô lập cho từng tổ
```json
{
  "assignments": [
    {
      "volunteerProfileId": "00000000-0000-0000-0000-000000000711",
      "householdDeliveryIds": [
        "00000000-0000-0000-0000-000000000811",
        "00000000-0000-0000-0000-000000000812"
      ],
      "subTaskTitle": "To xuong so 1",
      "taskNote": "Cum ho co lap phia Bac",
      "lineName": "To xuong 1"
    },
    {
      "volunteerProfileId": "00000000-0000-0000-0000-000000000712",
      "householdDeliveryIds": [
        "00000000-0000-0000-0000-000000000813",
        "00000000-0000-0000-0000-000000000814"
      ],
      "subTaskTitle": "To tiep can duong bo",
      "taskNote": "Cum ho tiep can duoc bang duong bo",
      "lineName": "To bo 1"
    }
  ]
}
```

---

## Bước 6 - Xem deliveries trong một subtask

### Endpoint
`GET /api/campaigns/member-tasks/{memberTaskId}/deliveries`

### Dùng để
- Team Leader kiểm tra line/tổ đó đang gánh những household deliveries nào
- Volunteer vào chi tiết subtask để xem danh sách hộ cụ thể

---

## 6. Flow Volunteer - mobile

## Bước 1 - Xem subtask của tôi

### Endpoint
`GET /api/campaigns/{campaignId}/member-tasks/me`

### Dùng để
- xem task/subtask được giao
- xem các deliveries nested trong subtask

---

## Bước 2 - Xem delivery mappings của tôi trực tiếp

### Endpoint
`GET /api/campaigns/{campaignId}/member-task-deliveries/me`

### Dùng để
- lấy danh sách delivery assignment cá nhân
- phù hợp mobile dashboard của volunteer

### Dữ liệu trả về
- household code
- tên chủ hộ
- địa chỉ
- status của delivery mapping
- status delivery thật
- scheduled time

---

## Bước 3 - Đổi trạng thái subtask

### Endpoint
`PATCH /api/campaigns/member-tasks/{memberTaskId}/status`

### Body mẫu
```json
{
  "status": 1
}
```

Lưu ý enum `MemberTaskStatus`:
- `Assigned`
- `InProgress`
- `Completed`
- `Failed`
- `Cancelled`

---

## Bước 4 - Đổi trạng thái delivery mapping trong subtask

### Endpoint
`PATCH /api/campaigns/member-task-deliveries/{memberTaskDeliveryId}/status`

### Body mẫu
```json
{
  "status": 1,
  "note": "Dang tiep can"
}
```

### Khi dùng
- muốn đánh dấu 1 delivery mapping đang xử lý
- chưa complete delivery thật ngay

---

## Bước 5 - Complete delivery mapping và sync delivery thật

### Endpoint
`POST /api/campaigns/member-task-deliveries/{memberTaskDeliveryId}/complete-with-delivery`

### Body mẫu
```json
{
  "proofFileUrl": "https://cdn.example.com/proofs/household-001.jpg",
  "proofContentType": "image/jpeg",
  "proofNote": "Ho dan da nhan du hang",
  "deliveryNote": "Phat du goi cuu tro va xac minh xong"
}
```

### Backend sẽ làm gì
1. cập nhật `HouseholdDelivery` sang `Delivered`
2. thêm proof vào `HouseholdDeliveryProof`
3. cập nhật `CampaignHousehold.FulfillmentStatus`
4. cập nhật `MemberTaskDelivery` sang `Completed`
5. nếu toàn bộ delivery mappings của subtask đã complete -> auto complete `MemberTask`
6. nếu toàn bộ member tasks của task chính đã complete -> auto complete `CampaignTask`

---

## 7. Checklist cho từng nhánh nghiệp vụ

## A. Checklist web cho Coordinator - PickupAtPoint
1. import households
2. xem households + lọc nhóm nhận tại điểm phát
3. xem plan summary
4. tạo distribution point
5. gán households vào team + point
6. chuẩn bị package / assemble package nếu cần
7. theo dõi shortage / approve shortage

## B. Checklist web cho Coordinator - DoorToDoor
1. import households
2. lọc hộ cô lập / cần xuồng / cần dẫn đường
3. xem plan summary
4. batch assign hộ cô lập cho team
5. assign vehicle cho team
6. theo dõi worklist / deliveries / shortage

## C. Checklist mobile cho Team Leader - PickupAtPoint
1. mở `team-worklist`
2. tạo task chính cho điểm phát
3. chia line bằng `batch-from-deliveries`
4. theo dõi subtask / member task deliveries
5. hỗ trợ complete delivery nếu cần

## D. Checklist mobile cho Team Leader - DoorToDoor
1. mở `team-worklist`
2. tạo task chính cho cụm hộ cô lập
3. chia deliveries cho từng tổ bằng `batch-from-deliveries`
4. theo dõi tiến độ từng tổ
5. xử lý proof / hỗ trợ complete delivery

## E. Checklist mobile cho Volunteer
1. mở `member-tasks/me`
2. mở `member-task-deliveries/me`
3. cập nhật trạng thái subtask hoặc delivery mapping
4. complete delivery thật với proof

---

## 8. Endpoint quan trọng nhất theo vai trò

## Coordinator - web
- `POST /households/import`
- `GET /households`
- `GET /plan-summary`
- `PATCH /households/{id}/assign`
- `PATCH /households/{id}/assign-isolated-team`
- `PATCH /households/isolated-team/bulk-assign`
- `POST /distribution-points`
- `POST /api/campaigns/{id}/teams/{campaignTeamId}/vehicles`

## Team Leader - mobile
- `GET /team-worklist`
- `POST /api/campaigns/{campaignId}/tasks`
- `POST /api/campaigns/tasks/{campaignTaskId}/members/batch-from-deliveries`
- `GET /api/campaigns/member-tasks/{memberTaskId}/deliveries`

## Volunteer - mobile
- `GET /api/campaigns/{campaignId}/member-tasks/me`
- `GET /api/campaigns/{campaignId}/member-task-deliveries/me`
- `PATCH /api/campaigns/member-task-deliveries/{memberTaskDeliveryId}/status`
- `POST /api/campaigns/member-task-deliveries/{memberTaskDeliveryId}/complete-with-delivery`

---

## 9. Lưu ý triển khai frontend
- Web nên tập trung vào Coordinator flow.
- Mobile nên tập trung vào Team Leader + Volunteer flow.
- Với mobile, entrypoint tốt nhất là:
  - `team-worklist` cho Team Leader
  - `member-task-deliveries/me` cho Volunteer
- `checklist` là endpoint tác nghiệp giao hàng, không nên là entrypoint điều phối chính.
