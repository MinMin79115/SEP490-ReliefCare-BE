# Relief Planning Handoff - Backend

## Mục tiêu tài liệu
- Chốt lại **flow cứu trợ thực tế** cho 3 vai trò chính:
  - `Coordinator`
  - `Team Leader`
  - `Volunteer`
- Đối chiếu với backend hiện tại để biết:
  - endpoint nào **đã hỗ trợ đúng nghiệp vụ**
  - endpoint nào **đã có nhưng chưa đủ logic**
  - endpoint nào **không nên dùng trực tiếp trong flow thực tế**
- Làm nền cho bước chuẩn hóa backend tiếp theo.

---

## Flow nghiệp vụ cứu trợ hoàn chỉnh

## 1. Đầu vào từ địa phương
Địa phương gửi xuống danh sách hộ dân cần cứu trợ, thường chỉ có:
- mã hộ
- tên chủ hộ
- số điện thoại
- địa chỉ text
- `lat/lng`
- số người/hộ
- hộ cô lập hay không
- mức ngập / mức cô lập nếu có
- có cần xuồng / cần người dẫn đường hay không

Lưu ý rất quan trọng:
- `LocationId` **không phải dữ liệu bắt buộc**
- backend phải vận hành được chỉ với `address + lat/lng`

---

## 2. Phân nhánh nghiệp vụ sau khi Coordinator điều phối

## Nhánh A - Hộ nhận tại điểm phát
Điều kiện:
- hộ dân loại `PickupAtPoint`
- không bị cô lập, hoặc có thể tiếp cận được đến điểm phát

Flow thực tế:
1. Coordinator import hộ dân vào campaign
2. Coordinator tạo / điều phối `distribution point`
3. Coordinator gán hộ dân vào team và điểm phát
4. Team Leader tạo **task chính** cho campaign/team:
   - ví dụ: `Phát hàng tại điểm phát A`
5. Team Leader tạo nhiều **subtask** cho thành viên:
   - hậu cần
   - phát hàng
   - xác thực/chứng nhận phát hàng
   - điều phối dòng người
   - cập nhật minh chứng
6. Volunteer thực hiện subtask
7. Volunteer / team complete delivery theo checklist hộ dân

## Nhánh B - Hộ bị cô lập, phát tận nơi
Điều kiện:
- hộ dân `IsIsolated = true`
- ưu tiên `DoorToDoor`
- có thể cần xuồng/ghe, xe chuyên dụng, người dẫn đường

Flow thực tế:
1. Coordinator import hộ dân bị cô lập
2. Coordinator lên plan summary và xác định:
   - khu vực ưu tiên
   - số đội cần
   - số xuồng/ghe/xe cần
   - nhu cầu áo phao, TNV địa phương, người dẫn đường
3. Coordinator gán **team cứu trợ** cho nhóm hộ cô lập
4. Coordinator điều phối phương tiện cứu trợ cho team
5. Team Leader tạo **task chính**:
   - ví dụ: `Phát hàng tới các hộ bị cô lập khu vực X`
6. Team Leader tạo nhiều **subtask** cho thành viên:
   - lái xuồng / vận hành phương tiện
   - mang hàng tiếp cận hộ
   - xác minh hộ nhận hàng
   - ghi nhận chứng từ / ảnh minh chứng
   - dẫn đường / phối hợp địa phương
7. Volunteer thực hiện subtask ngoài hiện trường
8. Team complete delivery cho từng household delivery

---

## Phân tích backend hiện tại theo vai trò

## A. Coordinator

### Chức năng Coordinator cần có
- import hộ dân
- xem, lọc, cập nhật hộ dân
- gán team cho hộ dân
- gán team cho hộ cô lập mà không cần điểm phát
- tạo điểm phát
- tạo gói cứu trợ
- xem plan summary để ra quyết định điều phối
- điều phối giao hàng / checklist / shortage

### Endpoint hiện có và phù hợp

#### 1. Import hộ dân
- `POST /api/relief/campaigns/{campaignId}/households/import`

Phù hợp:
- đúng cho dữ liệu từ địa phương gửi xuống
- hỗ trợ `lat/lng`, `address`, `householdSize`, `isIsolated`

Lưu ý:
- `LocationId` hiện là optional, đúng thực tế

#### 2. Xem danh sách hộ dân
- `GET /api/relief/campaigns/{campaignId}/households`

Phù hợp:
- có pagination
- có filter nghiệp vụ:
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
  - `Search`

#### 3. Gán hộ dân bình thường
- `PATCH /api/relief/campaigns/{campaignId}/households/{campaignHouseholdId}/assign`

Phù hợp khi:
- Coordinator muốn gán hộ vào team
- và nếu là `PickupAtPoint` thì gán luôn vào điểm phát

#### 4. Gán team cho hộ cô lập
- `PATCH /api/relief/campaigns/{campaignId}/households/{campaignHouseholdId}/assign-isolated-team`

Rất phù hợp cho flow thực tế:
- không cần gán distribution point
- ép về `DoorToDoor`
- gán team xử lý hộ cô lập

Đây là endpoint Coordinator nên dùng cho nhánh hộ cô lập.

#### 5. Cập nhật hộ dân
- `PATCH /api/relief/campaigns/{campaignId}/households/{campaignHouseholdId}`

Phù hợp:
- sửa địa chỉ, lat/lng, số người
- sửa thông tin mức ngập / mức cô lập
- cập nhật cần xuồng / cần dẫn đường

#### 6. Xóa hộ dân
- `DELETE /api/relief/campaigns/{campaignId}/households/{campaignHouseholdId}`

Phù hợp:
- chỉ khi chưa có delivery record

#### 7. Plan summary
- `GET /api/relief/campaigns/{campaignId}/plan-summary`

Phù hợp:
- Coordinator dùng để lên kế hoạch điều phối
- đã tính:
  - tổng hộ
  - hộ cô lập
  - số người/hộ
  - mật độ dân số nếu có
  - độ phân tán theo `lat/lng`
  - mode ưu tiên `điểm phát` hay `đội cơ động`
  - số đội / xuồng / áo phao gợi ý

#### 8. Điểm phát
- `POST /distribution-points`
- `GET /distribution-points`
- `PATCH /distribution-points/{distributionPointId}`
- `DELETE /distribution-points/{distributionPointId}`

Phù hợp cho nhánh `PickupAtPoint`.

#### 9. Gói cứu trợ / lắp gói / shortage
- `POST/GET/PATCH/DELETE /packages`
- `GET /packages/{id}/assembly-availability`
- `POST /packages/{id}/assemble`
- `GET /package-assemblies`
- `POST /shortage-requests`
- `GET /shortage-requests`
- `PATCH /shortage-requests/{id}/approve`
- `PATCH /shortage-requests/{id}/reject`

Phù hợp cho Coordinator / station side.

---

## B. Team Leader

### Chức năng Team Leader cần có
- biết team mình được gán hộ nào
- biết hộ nào là hộ cô lập / cần xuồng / cần dẫn đường
- biết khu vực nào cần điểm phát hoặc đội cơ động
- tạo task chính cho campaign/team
- tạo subtask cho thành viên
- gán nhiều subtask cho một thành viên hoặc nhiều thành viên

### Endpoint backend hiện có hỗ trợ

#### 1. Team lấy hộ cần đi hỗ trợ / giao hàng
- `GET /api/relief/campaigns/{campaignId}/team-worklist`

Đây là endpoint quan trọng cho Team Leader/Volunteer.

Dùng để lấy:
- danh sách household delivery cần đi
- ưu tiên hộ cô lập
- biết ai là team phụ trách
- có đầy đủ:
  - địa chỉ
  - `lat/lng`
  - `RequiresBoat`
  - `RequiresLocalGuide`
  - `FloodSeverityLevel`
  - `IsolationSeverityLevel`
  - `SuggestedSupportMode`

=> Đây là endpoint đúng để Team Leader nhìn “đội mình cần đi đâu”.

#### 2. Team Leader tạo task chính
Endpoint không nằm trong `ReliefDistributionController`, mà ở `CampaignTaskController`:

- `POST /api/campaigns/{campaignId}/tasks`

Phù hợp:
- tạo task chính cho chiến dịch/team
- ví dụ:
  - `Phát hàng tại điểm phát A`
  - `Phát hàng tới các hộ bị cô lập khu vực B`

#### 3. Team Leader xem danh sách task
- `GET /api/campaigns/{campaignId}/tasks`

#### 4. Team Leader xem chi tiết task
- `GET /api/campaigns/tasks/{campaignTaskId}`

#### 5. Team Leader gán subtask cho thành viên
- `POST /api/campaigns/tasks/{campaignTaskId}/members`
- `POST /api/campaigns/tasks/{campaignTaskId}/members/bulk`

Phù hợp cho:
- gán hậu cần
- gán phát hàng
- gán xác nhận/chứng từ
- gán lái phương tiện

#### 6. Team Leader tạo subtask từ danh sách hộ cần giao
- `POST /api/campaigns/tasks/{campaignTaskId}/members/from-households`

Đây là endpoint mới, rất sát thực tế.

Ý nghĩa:
- từ task chính, Team Leader chọn các `HouseholdDeliveryIds`
- tạo luôn subtask cho member
- phù hợp cho:
  - giao từng hộ
  - chia cụm hộ cho thành viên

---

## C. Volunteer

### Chức năng Volunteer cần có
- xem task/subtask của mình
- xem hộ dân cần đi giao / hỗ trợ
- biết cần xuồng hay không, có cần người dẫn đường hay không
- cập nhật trạng thái subtask
- complete delivery và gửi proof

### Endpoint hiện có hỗ trợ

#### 1. Lấy member tasks của chính mình
- `GET /api/campaigns/{campaignId}/my-member-tasks`

#### 2. Đổi trạng thái subtask
- `PATCH /api/campaigns/tasks/member-tasks/{memberTaskId}/status`

#### 3. Xem worklist của team
- `GET /api/relief/campaigns/{campaignId}/team-worklist`

Volunteer có thể dùng endpoint này để biết:
- hộ nào đang chờ
- cần support mode gì
- đường thủy / đường bộ

#### 4. Xem checklist phát hàng
- `GET /api/relief/campaigns/{campaignId}/checklist`

#### 5. Complete 1 delivery
- `POST /api/relief/campaigns/{campaignId}/deliveries/{householdDeliveryId}/complete`

#### 6. Complete batch delivery
- `POST /api/relief/campaigns/{campaignId}/deliveries/complete-batch`

#### 7. Xem delivery detail
- `GET /api/relief/campaigns/{campaignId}/deliveries/{householdDeliveryId}`

---

## Endpoint nào đang hợp logic

### Nên giữ và dùng
- `POST /households/import`
- `GET /households`
- `PATCH /households/{id}/assign`
- `PATCH /households/{id}/assign-isolated-team`
- `PATCH /households/{id}`
- `PATCH /households/{id}/status`
- `DELETE /households/{id}`
- `GET /plan-summary`
- `GET /team-worklist`
- `GET /checklist`
- toàn bộ CRUD `distribution-points`
- toàn bộ CRUD `packages`
- toàn bộ delivery complete / history / shortage requests
- `POST /api/campaigns/{campaignId}/tasks`
- `GET /api/campaigns/{campaignId}/tasks`
- `POST /api/campaigns/tasks/{campaignTaskId}/members`
- `POST /api/campaigns/tasks/{campaignTaskId}/members/bulk`
- `POST /api/campaigns/tasks/{campaignTaskId}/members/from-households`
- `PATCH /api/campaigns/tasks/member-tasks/{memberTaskId}/status`

---

## Endpoint nào chưa thật sự hợp logic / cần hạn chế dùng

### 1. `PATCH /households/{id}/assign`
Không sai, nhưng không nên dùng cho mọi trường hợp.

#### Nên dùng khi
- hộ nhận tại điểm phát
- Coordinator muốn gán điểm phát + team cùng lúc

#### Không nên là endpoint chính cho hộ cô lập
Vì với hộ cô lập, nghiệp vụ thực tế rõ hơn là:
- gán team cứu trợ
- bỏ điểm phát
- ép `DoorToDoor`

=> Với hộ cô lập nên ưu tiên dùng:
- `PATCH /households/{id}/assign-isolated-team`

### 2. `GET /distribution-points`
Endpoint này đúng cho nhánh `PickupAtPoint`, nhưng không phải nơi chính để team biết “hôm nay cần đi cứu hộ nào”.

Đối với team hiện trường, endpoint đúng hơn là:
- `GET /team-worklist`

### 3. `GET /checklist`
Checklist phù hợp cho bước xác nhận giao hàng, nhưng chưa đủ ngữ cảnh điều phối team.

=> Với Team Leader / Volunteer ngoài hiện trường:
- `team-worklist` nên là entrypoint chính
- `checklist` là bước tác nghiệp giao hàng

---

## Phần còn thiếu để thật sự hoàn chỉnh sát thực tế

## 1. Điều phối phương tiện cho team
Đã bổ sung cụm endpoint vehicle assignment theo campaign/team:
- `POST /api/campaigns/{id}/teams/{campaignTeamId}/vehicles`
- `GET /api/campaigns/{id}/vehicles`
- `PATCH /api/campaigns/{id}/vehicles/{campaignVehicleId}`
- `DELETE /api/campaigns/{id}/vehicles/{campaignVehicleId}`

Ý nghĩa:
- Coordinator gán xuồng/ghe/xe cho relief team trong campaign
- có thể gán driver, thời gian bắt đầu/kết thúc, trạng thái điều phối, ghi chú
- vehicle assignment được theo dõi bằng `CampaignVehicle`

## 2. Liên kết cứng subtask với delivery
Hiện `POST /members/from-households` đã tạo subtask từ `HouseholdDeliveryIds`, nhưng link mới dừng ở nghiệp vụ service/title-note.

Chưa có foreign key cứng kiểu:
- `MemberTask.HouseholdDeliveryId`

Nếu muốn workflow chặt hơn:
- member complete subtask
- kéo theo delivery state

thì nên thêm schema/link cứng ở phase sau.

## 3. Batch gán team cho nhiều hộ cô lập
Đã bổ sung endpoint batch:
- `PATCH /api/relief/campaigns/{campaignId}/households/isolated-team/bulk-assign`

Ý nghĩa:
- Coordinator chọn nhiều hộ cô lập trong cùng cụm
- gán 1 team
- có thể gán lịch, package, ghi chú cùng lúc

Response trả về:
- tổng số yêu cầu
- số thành công
- số thất bại
- chi tiết từng household

## 4. Điều phối theo cluster lat/lng nâng cao
Hiện summary đã có grouping theo tọa độ/address khá tốt, nhưng vẫn là heuristic nhẹ.

Nếu muốn sát thực tế hơn nữa:
- có clustering địa lý theo bán kính / DBSCAN-like
- tách “cụm giao tận nơi” và “cụm nhận tại điểm” rõ hơn

---

## Kết luận ngắn

### Với Coordinator
Backend hiện **đã có nền khá đầy đủ** cho:
- import hộ
- gán team
- gán team cho hộ cô lập
- tạo điểm phát
- tạo gói cứu trợ
- xem plan summary
- quản lý delivery/checklist/shortage

### Với Team Leader
Backend hiện **đã hỗ trợ gần đúng flow thực tế** cho:
- lấy danh sách hộ cần đi qua `team-worklist`
- tạo task chính qua `CampaignTaskController`
- tạo subtask cho member
- tạo subtask từ các household deliveries

### Với Volunteer
Backend hiện **đã có**:
- my member tasks
- đổi trạng thái subtask
- xem team worklist
- complete delivery / complete batch / proof

### Endpoint không nên lạm dụng
- `PATCH /households/{id}/assign` cho hộ cô lập

### Endpoint nên coi là chuẩn theo flow mới
- `PATCH /households/{id}/assign-isolated-team`
- `PATCH /households/isolated-team/bulk-assign`
- `GET /team-worklist`
- `POST /api/campaigns/tasks/{campaignTaskId}/members/from-households`
- `POST /api/campaigns/{id}/teams/{campaignTeamId}/vehicles`

---

## Route/role chuẩn hóa mới

## ReliefDistributionController

### Coordinator (`Manager, Moderator`)
- `POST /households/import`
- `PATCH /households/{id}/assign`
- `PATCH /households/{id}/assign-isolated-team`
- `PATCH /households/isolated-team/bulk-assign`
- `PATCH /households/{id}`
- `PATCH /households/{id}/status`
- `DELETE /households/{id}`
- `POST /distribution-points`
- `PATCH /distribution-points/{id}`
- `DELETE /distribution-points/{id}`
- `POST /packages`
- `PATCH /packages/{id}`
- `DELETE /packages/{id}`
- `POST /packages/{id}/assemble`
- `PATCH /shortage-requests/{id}/approve`
- `PATCH /shortage-requests/{id}/reject`

### Team Leader / Volunteer (`Manager, Moderator, Volunteer`)
- `GET /households`
- `GET /plan-summary`
- `GET /checklist`
- `GET /team-worklist`
- `GET /distribution-points`
- `GET /packages`
- `GET /package-assemblies`
- `GET /deliveries`
- `GET /deliveries/{id}`
- `POST /deliveries/{id}/complete`
- `POST /deliveries/complete-batch`
- `POST /shortage-requests`
- `GET /shortage-requests`

## CampaignTaskController

### Coordinator / Team Leader (`Manager, Moderator`)
- `POST /api/campaigns/{campaignId}/tasks`
- `PUT /api/campaigns/tasks/{campaignTaskId}`
- `PATCH /api/campaigns/tasks/{campaignTaskId}/status`
- `POST /api/campaigns/tasks/{campaignTaskId}/members`
- `POST /api/campaigns/tasks/{campaignTaskId}/members/bulk`
- `POST /api/campaigns/tasks/{campaignTaskId}/members/from-households`
- `DELETE /api/campaigns/tasks/{campaignTaskId}`

### Team Leader / Volunteer đọc/xử lý (`Manager, Moderator, Volunteer`)
- `GET /api/campaigns/{campaignId}/tasks`
- `GET /api/campaigns/tasks/{campaignTaskId}`
- `GET /api/campaigns/{campaignId}/member-tasks/me`
- `PATCH /api/campaigns/member-tasks/{memberTaskId}/status`

## CampaignController

### Coordinator (`Manager, Moderator`)
- `POST /api/campaigns/{id}/teams`
- `PATCH /api/campaigns/{id}/teams/{campaignTeamId}/status`
- `DELETE /api/campaigns/{id}/teams/{campaignTeamId}`
- `POST /api/campaigns/{id}/stations`
- `DELETE /api/campaigns/{id}/stations/{reliefStationId}`
- `POST /api/campaigns/{id}/teams/{campaignTeamId}/vehicles`
- `PATCH /api/campaigns/{id}/vehicles/{campaignVehicleId}`
- `DELETE /api/campaigns/{id}/vehicles/{campaignVehicleId}`

### Team Leader / Volunteer đọc (`Manager, Moderator, Volunteer`)
- `GET /api/campaigns/{id}/teams`
- `GET /api/campaigns/{id}/vehicles`

---

## Roadmap backend tiếp theo
1. Liên kết cứng `MemberTask` với `HouseholdDelivery`
2. Tạo endpoint complete subtask kèm sync delivery
3. Siết service-level ownership check để Team Leader chỉ thao tác đúng team mình
4. Thêm batch generate tasks/subtasks theo cụm households
5. Bổ sung cluster địa lý nâng cao và mission vehicle planning sâu hơn

---

## Mô hình backend cuối cùng đề xuất

## 1. Cấu trúc nghiệp vụ chuẩn

### `CampaignTask`
- là **task chính** của campaign/team
- biểu diễn mission ở mức lớn

Ví dụ:
- `Phát hàng tại điểm phát A`
- `Phát hàng tới cụm hộ cô lập khu vực X`

### `MemberTask`
- là **subtask theo vai trò/tổ**
- không đại diện cho đúng 1 hộ
- có thể giao cho 1 volunteer hoặc một nhóm làm chung theo vai trò

Ví dụ:
- `Hậu cần bốc xếp`
- `Bàn phát hàng số 1`
- `Tổ lái xuồng số 2`
- `Tổ xác minh và chứng từ`

### `MemberTaskDelivery`
- là bảng nối giữa `MemberTask` và từng `HouseholdDelivery`
- dùng khi cần theo dõi delivery nào thuộc subtask nào
- có thể thêm `AssignedVolunteerProfileId` nếu cần audit trách nhiệm cá nhân

---

## 2. Mô hình entity/backend cuối cùng cho `PickupAtPoint`

## Mục tiêu
- Coordinator gán hộ vào `CampaignTeam` và `DistributionPoint`
- Team Leader không bị bắt buộc phải gán lại từng hộ nếu không cần
- Chỉ tạo `MemberTaskDelivery` khi muốn chia nhỏ delivery xuống bàn/line/người cụ thể

## Flow chuẩn
1. Coordinator import households
2. Coordinator tạo `DistributionPoint`
3. Coordinator gán households vào:
   - `CampaignTeamId`
   - `DistributionPointId`
   - sinh `HouseholdDelivery`
4. Team Leader tạo `CampaignTask` chính:
   - `Phát hàng tại điểm phát A`
5. Team Leader tạo nhiều `MemberTask` theo vai trò:
   - hậu cần
   - phát hàng bàn 1
   - phát hàng bàn 2
   - xác thực / chứng từ
6. Nếu cần chia delivery cụ thể cho bàn/người thì mới tạo `MemberTaskDelivery`
7. Volunteer thực hiện task và complete delivery

## Rule quan trọng
- `HouseholdDelivery.CampaignTeamId` là phân công bắt buộc ở mức team
- `HouseholdDelivery.DistributionPointId` là bắt buộc với `PickupAtPoint`
- `MemberTaskDelivery` là **optional detail layer**
- Nếu không tạo `MemberTaskDelivery`, cả team vẫn có thể xử lý delivery qua `team-worklist` + `checklist`

## Khi nào cần `MemberTaskDelivery` trong nhánh này
- muốn chia delivery cho từng bàn phát hàng
- muốn chia delivery cho từng nhóm volunteer
- muốn audit member nào hoàn thành delivery nào
- muốn auto-complete subtask khi toàn bộ deliveries con hoàn tất

## Khi nào không cần
- cả team xử lý chung tại một điểm phát
- Team Leader chỉ cần phân vai trò, không cần chia từng hộ

---

## 3. Mô hình entity/backend cuối cùng cho `DoorToDoor`

## Mục tiêu
- hộ cô lập được xử lý theo mission/cụm/team/phương tiện trước
- không ép Team Leader phải map từng hộ ngay từ đầu
- cho phép tạo `MemberTaskDelivery` sau khi cần chia delivery cụ thể

## Flow chuẩn
1. Coordinator import isolated households
2. Coordinator dùng `plan-summary` để xác định:
   - khu vực ưu tiên
   - số đội cần
   - phương tiện cần
3. Coordinator gán households/cụm households vào `CampaignTeam`
4. Coordinator điều phối phương tiện qua `CampaignVehicle`
5. Team Leader tạo `CampaignTask` chính:
   - `Phát hàng tới cụm hộ cô lập khu vực X`
6. Team Leader tạo `MemberTask` theo tổ:
   - tổ lái phương tiện
   - tổ mang hàng
   - tổ xác minh/chứng từ
   - tổ dẫn đường
7. Nếu cần chốt delivery cụ thể cho từng người/tổ thì mới tạo `MemberTaskDelivery`
8. Volunteer thực hiện theo tổ, complete delivery ngoài hiện trường

## Rule quan trọng
- `HouseholdDelivery.CampaignTeamId` là bắt buộc ở mức team
- `DistributionPointId` không bắt buộc, thường null cho `DoorToDoor`
- `CampaignVehicle` gắn với `CampaignTeam` để thể hiện phương tiện mission-level
- `MemberTaskDelivery` là optional cho bước chi tiết hóa delivery xuống cá nhân/tổ

## Khi nào cần `MemberTaskDelivery` trong nhánh này
- muốn biết tổ/người nào phụ trách hộ nào
- mission đã tách cụ thể deliveries cho từng nhánh tiếp cận
- cần audit ai complete delivery nào

## Khi nào không cần
- team đang xử lý cả cụm như một mission chung
- Team Leader mới chỉ phân tổ, chưa chia từng delivery cụ thể

---

## 4. Rule tạo `MemberTaskDelivery` khi nào, khi nào không cần

## Nên tạo `MemberTaskDelivery` khi
- 1 `MemberTask` phải theo dõi danh sách `HouseholdDelivery` cụ thể
- cần rõ delivery nào thuộc subtask nào
- cần biết volunteer nào chịu trách nhiệm delivery nào
- cần auto-complete `MemberTask` từ trạng thái delivery con
- cần thống kê hiệu suất theo subtask/người

## Chưa cần tạo `MemberTaskDelivery` khi
- chỉ mới phân công ở mức team hoặc tổ
- subtask là nghiệp vụ chung như hậu cần / xác thực / điều phối
- team đang làm mission theo cụm, chưa cần chốt từng delivery
- điểm phát xử lý chung theo line/bàn, chưa cần audit từng hộ xuống người

---

## 5. Thiết kế entity cho `MemberTaskDelivery`

## Entity đề xuất
```csharp
public class MemberTaskDelivery
{
    public Guid MemberTaskDeliveryId { get; set; }
    public Guid MemberTaskId { get; set; }
    public Guid HouseholdDeliveryId { get; set; }
    public Guid? AssignedVolunteerProfileId { get; set; }
    public MemberTaskStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? CompletedByUserId { get; set; }
    public string? Note { get; set; }

    public MemberTask MemberTask { get; set; } = default!;
    public HouseholdDelivery HouseholdDelivery { get; set; } = default!;
    public VolunteerProfile? AssignedVolunteerProfile { get; set; }
}
```

## Quan hệ
- `CampaignTask` 1-n `MemberTask`
- `MemberTask` 1-n `MemberTaskDelivery`
- `HouseholdDelivery` 1-n `MemberTaskDelivery`

## Constraint quan trọng
- unique `(MemberTaskId, HouseholdDeliveryId)`
- chặn map `HouseholdDelivery` đã terminal (`Delivered`, `Cancelled` nếu có)
- nếu muốn 1 delivery chỉ thuộc 1 subtask active thì cần business check thêm

---

## 6. Flow backend rất rõ để duyệt trước khi code tiếp

```text
Coordinator
  -> Import households
  -> Create distribution points (PickupAtPoint only)
  -> Assign team/point for pickup households
  -> Assign isolated team for door-to-door households
  -> Bulk assign isolated households if needed
  -> Assign campaign vehicles to campaign team
  -> System creates/updates HouseholdDeliveries

Team Leader
  -> View team-worklist
  -> Create CampaignTask (main mission)
  -> Create MemberTasks (roles/teams)
  -> Optional: map HouseholdDeliveries into MemberTaskDelivery

Volunteer
  -> View member-tasks/me
  -> View team-worklist
  -> Execute MemberTask
  -> Complete HouseholdDelivery or MemberTaskDelivery

System
  -> Sync MemberTaskDelivery status
  -> Auto-complete MemberTask when delivery mappings complete
  -> Auto-complete CampaignTask when all MemberTasks complete
```

---

## 7. API backend nên có ở phase tiếp theo

### Tạo mapping delivery vào subtask
- `POST /api/campaigns/tasks/{campaignTaskId}/member-deliveries`

### Lấy delivery mappings của subtask
- `GET /api/campaigns/tasks/{campaignTaskId}/member-deliveries`

### Đổi trạng thái 1 mapping delivery
- `PATCH /api/campaigns/member-deliveries/{memberTaskDeliveryId}/status`

### Complete mapping delivery và sync delivery thật
- `POST /api/campaigns/member-deliveries/{memberTaskDeliveryId}/complete-with-delivery`

### Bulk gán deliveries vào subtask
- `POST /api/campaigns/tasks/{campaignTaskId}/member-deliveries/bulk`

---

## 8. Kết luận chốt mô hình
- `CampaignTask` = task chính theo mission/team
- `MemberTask` = subtask theo vai trò/tổ
- `MemberTaskDelivery` = optional detail layer để map từng `HouseholdDelivery` vào subtask
- `PickupAtPoint` không bắt buộc phải tạo `MemberTaskDelivery` cho mọi hộ
- `DoorToDoor` cũng không bắt buộc phải tạo `MemberTaskDelivery` ngay từ đầu; chỉ tạo khi cần chia delivery cụ thể cho người/tổ
