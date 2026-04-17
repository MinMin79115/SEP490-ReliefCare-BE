# Frontend Handoff - Relief Package Assembly Flow

Tài liệu này mô tả luồng **định nghĩa gói cứu trợ**, **kiểm tra khả năng đóng gói**, **đóng gói từ kho**, và **xem lịch sử đóng gói** để frontend có thể build màn hình và gọi API đúng.

---

## 1. Mục tiêu của flow

Flow này dùng để:

- định nghĩa gói cứu trợ từ các vật tư có sẵn trong kho
- xác định gói nào sẽ được cộng vào kho sau khi đóng gói
- kiểm tra tối đa có thể đóng được bao nhiêu gói từ tồn kho hiện tại
- thực hiện đóng gói tại một trạm cứu trợ / inventory cụ thể
- ghi lịch sử đóng gói để audit

Sau khi đóng gói xong, **gói cứu trợ đầu ra được lưu như một `SupplyItem` bình thường trong kho** và có thể được dùng trong các flow inventory / transfer / relief tiếp theo.

---

## 2. Phạm vi

### In scope

- Tạo package definition
- Chọn `outputSupplyItemId` cho package definition
- Xem danh sách package definitions của campaign Relief
- Kiểm tra `max assemblable quantity`
- Đóng gói từ inventory hiện tại
- Xem lịch sử đóng gói theo campaign / station / package definition

### Out of scope

- In tem hoặc barcode cho từng gói
- Lot/batch tracking nâng cao
- Trừ stock package tự động khi complete household delivery
- Route planning hoặc dispatch logistics nâng cao
- Unit conversion động giữa các đơn vị lớn/nhỏ

> Lưu ý: backend hiện giả định `SupplyItem` đã được lưu theo **đơn vị nhỏ nhất thống nhất**.

---

## 3. Các khái niệm FE cần hiểu

### 3.1 Relief Package Definition

Đây là **công thức của gói**.

Ví dụ:

- Gói A gồm:
  - 5 kg gạo
  - 3 chai nước

### 3.2 Output Supply Item

Là `SupplyItem` sẽ được **cộng vào kho** sau khi đóng gói thành công.

Ví dụ:

- `SupplyItem = Gói cứu trợ A`

Khi assemble 10 gói A:

- trừ component items khỏi kho
- cộng thêm `SupplyItem = Gói cứu trợ A` với quantity = 10

### 3.3 Assembly Availability

Là kết quả backend tính từ tồn kho hiện tại để trả về:

- mỗi item thành phần hiện còn bao nhiêu
- mỗi item giới hạn tối đa được bao nhiêu gói
- toàn bộ package hiện tối đa đóng được bao nhiêu gói

### 3.4 Package Assembly History

Là lịch sử audit cho việc đóng gói:

- ai tạo
- ở trạm nào
- ở inventory nào
- đóng bao nhiêu gói
- dùng những vật tư nào

---

## 4. Flow tổng quan

```text
1. Manager/Moderator mở màn hình package definition
2. Tạo package definition
   - nhập tên gói
   - chọn output supply item
   - chọn các component items và quantity
3. Mở màn hình assembly
4. Chọn station + inventory
5. FE gọi API availability
6. Backend trả maxAssemblableQuantity
7. User nhập quantity muốn assemble
8. FE gọi assemble API
9. Backend:
   - validate stock
   - trừ component stock
   - cộng output package stock
   - ghi assembly history
10. FE reload stock / history nếu cần
```

---

## 5. Màn hình FE cần build

## 5.1 Package Definition List

Hiển thị:

- tên package definition
- output supply item
- trạng thái active/default
- createdAt

API chính:

- `GET /api/relief/campaigns/{campaignId}/packages`

---

## 5.2 Create Package Definition

Form field tối thiểu:

- `name`
- `description`
- `outputSupplyItemId`
- `isDefault`
- `isActive`
- danh sách component items

Mỗi component item gồm:

- `supplyItemId`
- `quantity`
- `unit`

API chính:

- `POST /api/relief/campaigns/{campaignId}/packages`

---

## 5.3 Package Assembly Screen

Màn hình này nên cho user:

- chọn station
- chọn inventory
- xem component stock hiện tại
- xem `maxAssemblableQuantity`
- nhập `quantityToAssemble`
- bấm Assemble

API chính:

- `GET /api/relief/campaigns/{campaignId}/packages/{reliefPackageDefinitionId}/assembly-availability`
- `POST /api/relief/campaigns/{campaignId}/packages/{reliefPackageDefinitionId}/assemble`

---

## 5.4 Assembly History Screen

Màn hình hiển thị:

- package definition nào đã được assemble
- output item nào được tạo
- quantity created
- created by
- created at
- detail vật tư đã tiêu thụ

API chính:

- `GET /api/relief/campaigns/{campaignId}/package-assemblies`
- `GET /api/relief/campaigns/{campaignId}/stations/{reliefStationId}/package-assemblies`
- `GET /api/relief/campaigns/{campaignId}/packages/{reliefPackageDefinitionId}/package-assemblies`

---

## 6. API Contracts

---

## API 01 — Create package definition

### Endpoint

`POST /api/relief/campaigns/{campaignId}/packages`

### Khi FE gọi

Khi user bấm nút tạo package definition mới.

### Request body

```json
{
  "name": "Gói cứu trợ A",
  "description": "5kg gạo + 3 chai nước",
  "outputSupplyItemId": "OUTPUT_SUPPLY_ITEM_ID",
  "isDefault": true,
  "isActive": true,
  "items": [
    {
      "supplyItemId": "RICE_ID",
      "quantity": 5,
      "unit": "kg"
    },
    {
      "supplyItemId": "WATER_ID",
      "quantity": 3,
      "unit": "chai"
    }
  ]
}
```

### Response mẫu

```json
{
  "reliefPackageDefinitionId": "PACKAGE_DEF_ID",
  "campaignId": "CAMPAIGN_ID",
  "outputSupplyItemId": "OUTPUT_SUPPLY_ITEM_ID",
  "outputSupplyItemName": "Gói cứu trợ A",
  "outputUnit": "goi",
  "name": "Gói cứu trợ A",
  "description": "5kg gạo + 3 chai nước",
  "isDefault": true,
  "isActive": true,
  "createdAt": "2026-04-15T12:00:00Z",
  "items": [
    {
      "reliefPackageDefinitionItemId": "ITEM_1_ID",
      "supplyItemId": "RICE_ID",
      "supplyItemName": "Gạo",
      "quantity": 5,
      "unit": "kg"
    }
  ]
}
```

### Validation FE cần nhớ

- `outputSupplyItemId` là bắt buộc
- component item không được trùng nhau
- component item không được là package item
- `quantity > 0`

---

## API 02 — Get package definitions

### Endpoint

`GET /api/relief/campaigns/{campaignId}/packages`

### Khi FE gọi

- khi mở package definition list
- sau khi tạo package definition thành công

### Response

Trả về mảng `ReliefPackageDefinitionResponse`.

---

## API 03 — Get assembly availability

### Endpoint

`GET /api/relief/campaigns/{campaignId}/packages/{reliefPackageDefinitionId}/assembly-availability?reliefStationId={reliefStationId}&inventoryId={inventoryId}`

### Khi FE gọi

- khi user chọn station + inventory
- khi user thay package definition
- sau mỗi lần assemble nếu muốn refresh lại khả năng đóng gói

### Response mẫu

```json
{
  "campaignId": "CAMPAIGN_ID",
  "reliefStationId": "STATION_ID",
  "inventoryId": "INVENTORY_ID",
  "reliefPackageDefinitionId": "PACKAGE_DEF_ID",
  "outputSupplyItemId": "OUTPUT_SUPPLY_ITEM_ID",
  "outputSupplyItemName": "Gói cứu trợ A",
  "outputUnit": "goi",
  "maxAssemblableQuantity": 20,
  "components": [
    {
      "supplyItemId": "RICE_ID",
      "supplyItemName": "Gạo",
      "unit": "kg",
      "requiredPerPackage": 5,
      "availableQuantity": 100,
      "maxAssemblableByItem": 20
    },
    {
      "supplyItemId": "WATER_ID",
      "supplyItemName": "Nước suối",
      "unit": "chai",
      "requiredPerPackage": 3,
      "availableQuantity": 60,
      "maxAssemblableByItem": 20
    }
  ]
}
```

### FE dùng response này để làm gì

- hiển thị tồn hiện tại của từng component
- hiển thị `maxAssemblableQuantity`
- disable hoặc giới hạn input nếu user nhập quá số lượng tối đa

### Lưu ý

`maxAssemblableQuantity` là giá trị **động**, được tính từ stock hiện tại. Sau khi assemble xong, FE nên **reload API này** nếu muốn hiển thị số mới.

---

## API 04 — Assemble package

### Endpoint

`POST /api/relief/campaigns/{campaignId}/packages/{reliefPackageDefinitionId}/assemble`

### Khi FE gọi

Khi user xác nhận đóng gói.

### Request body

```json
{
  "reliefStationId": "STATION_ID",
  "inventoryId": "INVENTORY_ID",
  "quantityToAssemble": 10,
  "notes": "Đóng gói cho đợt phát đầu tiên"
}
```

### Response mẫu

```json
{
  "reliefPackageAssemblyId": "ASSEMBLY_ID",
  "campaignId": "CAMPAIGN_ID",
  "reliefStationId": "STATION_ID",
  "inventoryId": "INVENTORY_ID",
  "reliefPackageDefinitionId": "PACKAGE_DEF_ID",
  "outputSupplyItemId": "OUTPUT_SUPPLY_ITEM_ID",
  "outputSupplyItemName": "Gói cứu trợ A",
  "outputUnit": "goi",
  "quantityCreated": 10,
  "createdBy": "USER_ID",
  "createdAt": "2026-04-15T12:05:00Z",
  "notes": "Đóng gói cho đợt phát đầu tiên",
  "details": [
    {
      "supplyItemId": "RICE_ID",
      "supplyItemName": "Gạo",
      "unit": "kg",
      "quantityConsumed": 50
    },
    {
      "supplyItemId": "WATER_ID",
      "supplyItemName": "Nước suối",
      "unit": "chai",
      "quantityConsumed": 30
    }
  ]
}
```

### Business effect

Khi assemble thành công, backend sẽ:

- trừ component items khỏi stock
- cộng output package item vào stock
- ghi assembly history

### FE handling recommendation

Sau khi assemble thành công, FE nên:

1. show toast success
2. refetch:
   - inventory stocks nếu màn hình đang hiển thị stock
   - assembly availability
   - assembly history

---

## API 05 — Get package assembly history by campaign

### Endpoint

`GET /api/relief/campaigns/{campaignId}/package-assemblies`

### Khi FE gọi

- khi mở màn hình assembly history tổng

### Response

Trả về mảng `ReliefPackageAssemblyResponse`.

---

## API 06 — Get package assembly history by station

### Endpoint

`GET /api/relief/campaigns/{campaignId}/stations/{reliefStationId}/package-assemblies`

### Khi FE gọi

- khi user lọc history theo station

---

## API 07 — Get package assembly history by package definition

### Endpoint

`GET /api/relief/campaigns/{campaignId}/packages/{reliefPackageDefinitionId}/package-assemblies`

### Khi FE gọi

- khi user muốn xem chỉ lịch sử của 1 package definition

---

## 7. Data model FE nên map

## 7.1 ReliefPackageDefinitionResponse

Field quan trọng:

| Field | Ý nghĩa | FE usage |
|---|---|---|
| `reliefPackageDefinitionId` | ID package definition | route / action target |
| `outputSupplyItemId` | item đầu ra sẽ được cộng stock | hiển thị + dùng logic |
| `outputSupplyItemName` | tên item đầu ra | label UI |
| `outputUnit` | đơn vị output item | hiển thị |
| `items[]` | component items | render công thức gói |

---

## 7.2 ReliefPackageAssemblyAvailabilityResponse

Field quan trọng:

| Field | Ý nghĩa | FE usage |
|---|---|---|
| `maxAssemblableQuantity` | tối đa hiện tại có thể đóng được | giới hạn input / hint |
| `components[]` | tình trạng từng item | render bảng availability |

---

## 7.3 ReliefPackageAssemblyResponse

Field quan trọng:

| Field | Ý nghĩa | FE usage |
|---|---|---|
| `quantityCreated` | số gói đã tạo | hiển thị history |
| `createdAt` | thời điểm đóng gói | hiển thị timeline/history |
| `createdBy` | người tạo | hiển thị audit |
| `details[]` | chi tiết component đã tiêu thụ | hiển thị expand details |

---

## 8. Step-by-step FE flow khuyến nghị

## Step 1 — Load package definitions

FE gọi:

- `GET /api/relief/campaigns/{campaignId}/packages`

## Step 2 — User chọn package definition và station/inventory

FE cần có:

- `campaignId`
- `reliefPackageDefinitionId`
- `reliefStationId`
- `inventoryId`

## Step 3 — Load availability

FE gọi:

- `GET assembly-availability`

FE hiển thị:

- output package item
- max assemblable quantity
- bảng component stock

## Step 4 — User nhập quantity muốn assemble

FE nên validate trước:

- `quantityToAssemble > 0`
- `quantityToAssemble <= maxAssemblableQuantity`

## Step 5 — User bấm Assemble

FE gọi:

- `POST assemble`

## Step 6 — Sau khi thành công

FE nên reload:

- assembly history
- availability
- inventory stock nếu màn hình đang hiển thị tồn

---

## 9. Validation và lỗi FE dễ gặp

## 9.1 Package component không hợp lệ

Backend sẽ reject nếu component item là package item.

### FE recommendation

Khi load item picker cho component items, nên **filter bỏ các `SupplyItem` có category = Package**.

---

## 9.2 Assemble vượt quá stock

Ví dụ backend trả:

```json
{
  "message": "Internal Server Error",
  "detail": "Insufficient component stock. Maximum assemblable quantity is 10."
}
```

### FE recommendation

- show error banner/toast
- reload lại availability
- cập nhật max mới cho user

> Hiện tại backend đang trả lỗi business theo format error chung và status runtime có thể là `500` với `detail` chứa thông tin business. FE nên đọc `detail` để hiển thị dễ hiểu.

---

## 9.3 Không reload availability sau khi assemble

Đây là lỗi FE rất dễ gặp.

Ví dụ:

- trước assemble: max = 20
- assemble 10
- nếu không reload thì UI vẫn hiển thị max = 20, nhưng thực tế chỉ còn 10

### FE recommendation

Sau mỗi lần assemble thành công, luôn gọi lại:

- `GET assembly-availability`

---

## 9.4 Không đồng bộ stock màn hình inventory

Nếu cùng màn hình có hiển thị inventory stock, FE nên refetch stock để phản ánh:

- component giảm
- package output tăng

---

## 10. Ví dụ thực tế từ test đã pass

Test đã chạy thành công với kết quả:

```json
{
  "maxAssemblable": 20,
  "assemblyCreated": 10,
  "packageStockQty": 10,
  "riceRemaining": 50,
  "waterRemaining": 30,
  "historyCount": 1,
  "historyFirstDetailCount": 2
}
```

### Diễn giải

- package definition cần:
  - 5 gạo
  - 3 nước
- stock ban đầu:
  - gạo 100
  - nước 60
- nên max = 20
- assemble 10 thành công
- stock sau assemble:
  - gạo còn 50
  - nước còn 30
  - package output tăng lên 10

---

## 11. Common pitfalls

- Hiểu nhầm `PackageDefinition` là stock thật → thực tế đây chỉ là template
- Quên chọn `outputSupplyItemId`
- Không filter package item khỏi component picker
- Không reload availability sau khi assemble
- Không refetch inventory stock sau khi assemble
- Dùng `maxAssemblableQuantity` cũ sau khi tồn kho đã đổi

---

## 12. Tóm tắt FE action map

| User action | API |
|---|---|
| Xem package definitions | `GET /packages` |
| Tạo package definition | `POST /packages` |
| Xem có thể đóng tối đa bao nhiêu | `GET /packages/{id}/assembly-availability` |
| Đóng gói | `POST /packages/{id}/assemble` |
| Xem history toàn campaign | `GET /package-assemblies` |
| Xem history theo station | `GET /stations/{stationId}/package-assemblies` |
| Xem history theo package | `GET /packages/{id}/package-assemblies` |
