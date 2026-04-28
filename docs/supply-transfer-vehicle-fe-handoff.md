# Supply Transfer Vehicle FE Handoff

Tài liệu này mô tả cách FE dùng phần Vehicle trong luồng điều chuyển hàng cứu trợ (`SupplyTransfer`). Đây là luồng logistics/cứu trợ, khác với luồng cứu hộ (`RescueOperation`).

## 1. Tổng quan flow

Một `SupplyTransfer` hiện có thể gán nhiều xe:

```text
Destination station head tạo phiếu
        -> Pending
Source station head duyệt phiếu
        -> Approved
Source station head gán nhiều xe
        -> Vehicles[].status = Assigned, Vehicle.status = Busy
Source station head ship
        -> SupplyTransfer.status = Shipping
        -> Vehicles[].status = InTransit
Destination station head receive
        -> SupplyTransfer.status = Received
        -> Vehicles[].status = Completed
        -> Vehicle.status = Free
```

Nguồn dữ liệu xe chuẩn của transfer là:

```ts
SupplyTransferResponse.vehicles: SupplyTransferVehicleResponse[]
```

Không nên dùng `vehicleId` / `driverUserId` ở cấp `SupplyTransfer` cho UI mới. Hai field đó chỉ giữ để tương thích legacy.

## 2. Base API

Local Docker:

```text
http://localhost:8080
```

Tất cả endpoint bên dưới cần JWT:

```http
Authorization: Bearer <accessToken>
```

Login seed account thường dùng:

```http
POST /api/Auth/login
```

```json
{
  "email": "moderator@system.com",
  "password": "Moderator@123"
}
```

## 3. Enums FE cần map

### SupplyTransferStatus

| Value | Name | Ý nghĩa |
|---:|---|---|
| 1 | `Pending` | Chờ trạm nguồn duyệt |
| 2 | `Approved` | Đã được duyệt, có thể gán xe |
| 3 | `Shipping` | Đang vận chuyển |
| 4 | `Received` | Đã nhận hàng xong |
| 5 | `Cancelled` | Đã hủy |

### SupplyTransferVehicleStatus

| Value | Name | Ý nghĩa | FE action gợi ý |
|---:|---|---|---|
| 1 | `Assigned` | Xe đã được gán, chưa xuất phát | Remove, Depart |
| 2 | `InTransit` | Xe đang vận chuyển | Arrive, Incident |
| 3 | `Arrived` | Xe đã tới nơi | Complete |
| 4 | `Completed` | Xe đã hoàn tất nhiệm vụ | View only |
| 5 | `Cancelled` | Assignment xe đã bị hủy | View only |
| 6 | `Incident` | Xe gặp sự cố | Complete/handle manually |

### VehicleStatus

| Value | Name | Ý nghĩa |
|---:|---|---|
| 1 | `Free` | Xe rảnh, có thể gán |
| 2 | `Busy` | Xe đang được reserve/sử dụng |

## 4. Lấy xe khả dụng cho transfer

Dùng endpoint này ở màn chọn xe:

```http
GET /api/vehicle/available-for-transfer
```

Role:

```text
Moderator
```

Backend tự lấy station theo moderator hiện tại và chỉ trả xe:

```text
Vehicle.status = Free
Vehicle.reliefStationId = station của moderator
Vehicle.isDeleted = false
```

Response là danh sách `VehicleResponse`. Các field quan trọng cho FE:

```ts
type VehicleResponse = {
  vehicleId: string;
  vehicleTypeId: string;
  vehicleTypeName?: string;
  licensePlate: string;
  status: number;
  statusName?: string;
  reliefStationId?: string;
  reliefStationName?: string;
  teamId?: string;
  teamName?: string;
};
```

FE nên hiển thị:

- biển số `licensePlate`
- loại xe `vehicleTypeName`
- trạm `reliefStationName`
- team `teamName` nếu có

## 5. Response SupplyTransfer mới

Các endpoint detail/action trả về `SupplyTransferResponse`.

Field quan trọng:

```ts
type SupplyTransferResponse = {
  supplyTransferId: string;
  transferCode: string;
  sourceStationId: string;
  sourceStationName: string;
  destinationStationId: string;
  destinationStationName: string;
  status: number;
  requestedAt: string;
  approvedAt?: string | null;
  shippedAt?: string | null;
  receivedAt?: string | null;
  requestedBy: string;
  requestedByName: string;
  approvedBy?: string | null;
  approvedByName?: string | null;

  // Legacy, không dùng cho UI mới
  vehicleId?: string | null;
  driverUserId?: string | null;

  // Source of truth mới
  vehicles: SupplyTransferVehicleResponse[];

  notes?: string | null;
  evidenceUrls: string[];
  items: SupplyTransferItemResponse[];
  inventoryTransactionIds: string[];
};

type SupplyTransferVehicleResponse = {
  supplyTransferVehicleId: string;
  vehicleId: string;
  licensePlate: string;
  vehicleTypeId: string;
  vehicleTypeName: string;
  driverUserId?: string | null;
  driverName?: string | null;
  status: number;
  assignedAt: string;
  departedAt?: string | null;
  arrivedAt?: string | null;
  completedAt?: string | null;
  note?: string | null;
};
```

## 6. Gán nhiều xe vào transfer

Chỉ gán xe khi:

```text
SupplyTransfer.status = Approved
```

Endpoint:

```http
PATCH /api/SupplyTransfer/{transferId}/vehicles
```

Role:

```text
Source station head
```

Request:

```json
{
  "vehicles": [
    {
      "vehicleId": "d9748869-cc3a-4d49-a76d-a2ddf3927610",
      "driverUserId": null,
      "note": "Chở gạo và nhu yếu phẩm"
    },
    {
      "vehicleId": "bbb7f51b-283b-49cd-90d4-e92b252e91e3",
      "driverUserId": null,
      "note": "Chở nước uống"
    }
  ]
}
```

Response:

```json
{
  "supplyTransferId": "...",
  "status": 2,
  "vehicles": [
    {
      "supplyTransferVehicleId": "...",
      "vehicleId": "d9748869-cc3a-4d49-a76d-a2ddf3927610",
      "licensePlate": "RC-00001",
      "status": 1,
      "assignedAt": "2026-04-28T...Z"
    },
    {
      "supplyTransferVehicleId": "...",
      "vehicleId": "bbb7f51b-283b-49cd-90d4-e92b252e91e3",
      "licensePlate": "RC-00002",
      "status": 1,
      "assignedAt": "2026-04-28T...Z"
    }
  ]
}
```

Sau khi assign thành công:

```text
Vehicle.status = Busy
SupplyTransferVehicle.status = Assigned
```

Validation backend có thể trả lỗi nếu:

- transfer chưa `Approved`
- xe không thuộc trạm nguồn
- xe không `Free`
- xe trùng trong request
- xe đã được gán vào transfer này
- xe đang active trong rescue operation

## 7. Bỏ xe khỏi transfer trước khi ship

Chỉ dùng khi xe còn `Assigned` và transfer chưa ship.

Endpoint:

```http
DELETE /api/SupplyTransfer/{transferId}/vehicles/{supplyTransferVehicleId}
```

Role:

```text
Source station head
```

Kết quả:

```text
SupplyTransferVehicle.status = Cancelled
Vehicle.status = Free
```

FE lưu ý: path param là `supplyTransferVehicleId`, không phải `vehicleId`.

## 8. Ship transfer

Endpoint:

```http
PATCH /api/SupplyTransfer/{transferId}/ship
```

Role:

```text
Source station head
```

Request:

```json
{
  "notes": "Đoàn xe xuất phát lúc 08:30",
  "evidenceUrls": []
}
```

Không cần gửi `vehicleId`/`vehicleIds` trong flow mới. Xe phải được assign trước qua endpoint `/vehicles`.

Kết quả:

```text
SupplyTransfer.status = Shipping
SupplyTransfer.shippedAt != null
Vehicles[].status = InTransit
Vehicles[].departedAt != null
```

Nếu chưa có xe active, backend reject:

```text
Không thể xuất hàng khi chưa có xe được phân công.
```

## 9. Update trạng thái từng xe

Endpoint dùng chung:

```http
PATCH /api/SupplyTransfer/{transferId}/vehicles/{supplyTransferVehicleId}/status
```

Request:

```json
{
  "status": 3,
  "note": "Xe đã tới trạm đích"
}
```

Các status FE nên gửi:

| Action | status gửi | Role |
|---|---:|---|
| Depart | 2 (`InTransit`) | Source station head |
| Arrive | 3 (`Arrived`) | Destination station head |
| Complete | 4 (`Completed`) | Destination station head |
| Report incident | 6 (`Incident`) | Source station head |

Khi `Completed`:

```text
SupplyTransferVehicle.status = Completed
Vehicle.status = Free
```

## 10. Receive transfer

Endpoint:

```http
PATCH /api/SupplyTransfer/{transferId}/receive
```

Role:

```text
Destination station head
```

Request:

```json
{
  "items": [
    {
      "supplyItemId": "47bfc41a-05a5-4e44-b734-33c2be510f9a",
      "actualQuantity": 5,
      "notes": "Nhận đủ"
    }
  ],
  "notes": "Đã kiểm hàng tại trạm đích",
  "evidenceUrls": []
}
```

Kết quả:

```text
SupplyTransfer.status = Received
SupplyTransfer.receivedAt != null
Active Vehicles[].status = Completed
Vehicle.status = Free
InventoryTransaction import được tạo
```

## 11. Cancel transfer

Endpoint:

```http
PATCH /api/SupplyTransfer/{transferId}/cancel
```

Request:

```json
{
  "notes": "Hủy do không còn nhu cầu",
  "evidenceUrls": []
}
```

Chỉ hủy được khi transfer chưa `Shipping` hoặc `Received`.

Nếu transfer có xe active:

```text
Vehicles[].status = Cancelled
Vehicle.status = Free
```

## 12. UI state gợi ý

### Khi transfer `Pending`

- Hiển thị thông tin phiếu.
- Source station head có nút `Approve`.
- Không cho chọn xe.

### Khi transfer `Approved`

- Source station head thấy nút:
  - `Add vehicles`
  - `Remove vehicle` với xe `Assigned`
  - `Ship`
- Gọi `GET /api/vehicle/available-for-transfer` để mở modal chọn nhiều xe.

### Khi transfer `Shipping`

- Hiển thị tracking từng xe trong `vehicles[]`.
- Với xe `InTransit`, destination station head có thể `Arrive`.
- Với xe `Arrived`, destination station head có thể `Complete`.
- Destination station head có thể `Receive` tổng thể sau khi kiểm hàng.

### Khi transfer `Received` hoặc `Cancelled`

- Read-only.
- Không hiển thị action thay đổi xe.

## 13. Test case FE nên kiểm

### Happy path

1. Destination station head tạo transfer.
2. Source station head approve.
3. Source station head lấy available vehicles.
4. Assign 2 xe.
5. Ship transfer.
6. Destination station head receive.
7. Verify UI hiển thị 2 xe `Completed`.

### Validation cases

- Assign xe khi transfer còn `Pending` -> lỗi.
- Assign cùng một xe 2 lần -> lỗi.
- Assign xe không thuộc source station -> lỗi.
- Ship khi chưa assign xe -> lỗi.
- Remove xe sau khi transfer đã `Shipping` -> lỗi.
- Receive actual quantity lớn hơn requested quantity -> lỗi.

## 14. Kết quả BE đã test bằng Docker

Flow end-to-end đã pass với 2 xe:

```json
{
  "createStatus": 1,
  "approveStatus": 2,
  "assignedCount": 2,
  "assignedStatuses": "1,1",
  "shipStatus": 3,
  "shipVehicleStatuses": "2,2",
  "receiveStatus": 4,
  "receiveVehicleStatuses": "4,4",
  "plates": "RC-00001,RC-00003"
}
```

DB verify sau receive:

```text
SupplyTransfer.status = Received
SupplyTransferVehicle.status = Completed
Vehicle.status = Free
```
