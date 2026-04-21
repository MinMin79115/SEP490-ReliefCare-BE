# Frontend Handoff - Campaign Budget Extraction & Relief Cash Support APIs

Tài liệu này mô tả các API mới để frontend có thể:

- chuyển ngân sách từ campaign fundraising sang campaign relief
- cấu hình `cashSupportAmount` trong relief package
- complete delivery có phát tiền mặt
- complete nhiều delivery trong một lần gọi

---

## 1. Mục tiêu nghiệp vụ

### 1.1 Budget extraction

Cho phép lấy một phần ngân sách từ **campaign Fundraising** để cấp sang **campaign Relief**.

Use case FE:

- màn hình chi tiết campaign fundraising có nút **Chuyển ngân sách sang campaign relief**
- user chọn campaign relief đích
- nhập số tiền cần chuyển
- backend kiểm tra ngân sách còn lại rồi tạo transaction

### 1.2 Cash support in relief distribution

Cho phép mỗi package cứu trợ có thêm một phần **hỗ trợ tiền mặt**.

Use case FE:

- khi tạo / sửa relief package, user có thể nhập `cashSupportAmount`
- khi assign / review delivery, FE hiển thị số tiền mặt đi kèm package
- khi complete delivery, FE có thể giữ nguyên tiền theo package hoặc override lại số tiền thực tế phát

---

## 2. Tổng quan flow

```text
1. Fundraising campaign chuyển ngân sách sang relief campaign
2. Relief campaign có BudgetTotal tăng lên
3. FE tạo/sửa relief package với cashSupportAmount
4. FE assign household vào package
5. Khi complete delivery:
   - FE upload proof/file trước (nếu có flow upload riêng)
   - FE gọi complete delivery API
   - backend kiểm tra campaign còn đủ tiền không
   - nếu đủ: cộng BudgetSpent và đánh dấu delivered
   - nếu không đủ: trả lỗi, không complete delivery
```

---

## 3. Các API FE cần dùng

## API 01 — Extract budget từ fundraising campaign sang relief campaign

### Endpoint

`POST /api/campaigns/{id}/extract-budget`

- `{id}` = `sourceCampaignId` = campaign fundraising nguồn

### Khi FE gọi

Khi user đang ở campaign fundraising và bấm chuyển tiền sang 1 campaign relief.

### Request body

```json
{
  "targetReliefCampaignId": "7d0d1b0b-8f2c-4f14-9f65-1f44b4fd83a1",
  "amount": 5000000,
  "note": "Chuyển ngân sách đợt 1 sang campaign cứu trợ miền Trung"
}
```

### Field rules

- `targetReliefCampaignId`: bắt buộc
- `amount`: bắt buộc, phải `> 0`
- `note`: optional, tối đa 1000 ký tự

### Response body

```json
{
  "campaignBudgetTransferId": "2cf1b8bd-1c56-4b18-988c-5d7d2a7d9a55",
  "sourceCampaignId": "6b1cd6bc-a2f9-4660-86b7-73fcb6c7428d",
  "targetCampaignId": "7d0d1b0b-8f2c-4f14-9f65-1f44b4fd83a1",
  "amount": 5000000,
  "transferredByUserId": "ceabf59f-5c53-4ac2-8e4f-1f7f51bd8f84",
  "transferredAt": "2026-04-20T10:15:32.000Z",
  "note": "Chuyển ngân sách đợt 1 sang campaign cứu trợ miền Trung",
  "sourceRemainingBudget": 15000000,
  "targetRemainingBudget": 5000000
}
```

### FE nên hiển thị

- số tiền vừa chuyển thành công
- ngân sách còn lại của campaign nguồn
- tổng ngân sách khả dụng của campaign relief đích sau khi nhận tiền

### Business rules FE cần biết

- source campaign phải là **Fundraising**
- target campaign phải là **Relief**
- không được chuyển quá số tiền còn lại của campaign nguồn
- nếu lỗi nghiệp vụ, backend sẽ reject request

### Gợi ý UX

- disable nút submit nếu `amount <= 0`
- hiển thị confirm modal trước khi submit
- sau khi success, reload campaign detail / campaign budget summary

---

## API 02 — Create relief package có hỗ trợ tiền mặt

### Endpoint

`POST /api/relief/campaigns/{campaignId}/packages`

### Khi FE gọi

Khi user tạo package cứu trợ mới cho campaign relief.

### Request body

```json
{
  "name": "Gói hỗ trợ khẩn cấp A",
  "description": "Gạo, nước, mì và tiền mặt",
  "outputSupplyItemId": "df54c5a6-aaf4-4cf8-b71d-2a5b2e0d6d4c",
  "cashSupportAmount": 300000,
  "isDefault": true,
  "isActive": true,
  "items": [
    {
      "supplyItemId": "7c2ed6d6-9707-46cc-a242-f6f7d0456a21",
      "quantity": 10,
      "unit": "kg"
    },
    {
      "supplyItemId": "d9bd06bb-9fb2-435c-a0ff-5eeb3df2323f",
      "quantity": 1,
      "unit": "thùng"
    }
  ]
}
```

### Field rules

- `name`: bắt buộc, tối đa 255 ký tự
- `description`: optional, tối đa 1000 ký tự
- `outputSupplyItemId`: bắt buộc
- `cashSupportAmount`: optional, nếu gửi thì phải `>= 0`
- `items`: bắt buộc, ít nhất 1 item
- mỗi item:
  - `supplyItemId`: bắt buộc
  - `quantity`: phải `>= 1`
  - `unit`: bắt buộc, tối đa 50 ký tự

### Response body

```json
{
  "reliefPackageDefinitionId": "f11e4f39-3b8e-4b1d-b4b4-49102089d1b3",
  "campaignId": "7d0d1b0b-8f2c-4f14-9f65-1f44b4fd83a1",
  "outputSupplyItemId": "df54c5a6-aaf4-4cf8-b71d-2a5b2e0d6d4c",
  "outputSupplyItemName": "Gói hỗ trợ khẩn cấp A",
  "outputUnit": "gói",
  "cashSupportAmount": 300000,
  "name": "Gói hỗ trợ khẩn cấp A",
  "description": "Gạo, nước, mì và tiền mặt",
  "isDefault": true,
  "isActive": true,
  "createdAt": "2026-04-20T10:20:00.000Z",
  "items": [
    {
      "reliefPackageDefinitionItemId": "c9e68b56-7c61-49cc-8d5e-520ea1d3b2e1",
      "supplyItemId": "7c2ed6d6-9707-46cc-a242-f6f7d0456a21",
      "supplyItemName": "Gạo",
      "quantity": 10,
      "unit": "kg"
    }
  ]
}
```

### FE nên hiển thị

- `cashSupportAmount` như một phần của package
- badge `default`
- trạng thái `active`

### Business rules FE cần biết

- package chỉ dùng cho **relief campaign**
- `outputSupplyItemId` là **sản phẩm đầu ra** của package, FE bắt buộc phải cho user chọn khi tạo mới
- không được có item component bị trùng nhau
- item component không được trùng với `outputSupplyItemId`
- nếu `isDefault = true`, backend sẽ tự bỏ default ở package cũ
- nếu không gửi `cashSupportAmount`, backend sẽ hiểu là `0`

### Ý nghĩa của `outputSupplyItemId`

FE nên hiểu field này như sau:

- đây là `SupplyItem` đại diện cho **gói thành phẩm sau khi assemble**
- ví dụ package là `Gói hỗ trợ khẩn cấp A`
- thì `outputSupplyItemId` chính là supply item `Gói hỗ trợ khẩn cấp A`
- còn `items[]` là các vật tư thành phần để tạo ra gói đó

Nói ngắn gọn:

- `outputSupplyItemId` = thành phẩm
- `items[]` = nguyên liệu / thành phần

---

## API 03 — Get list relief packages

### Endpoint

`GET /api/relief/campaigns/{campaignId}/packages`

### Mục đích

Lấy danh sách package để FE:

- render package list
- chọn package khi assign household
- hiển thị `cashSupportAmount`

### Field FE cần dùng

Trong mỗi item response, FE quan tâm:

- `reliefPackageDefinitionId`
- `name`
- `description`
- `cashSupportAmount`
- `isDefault`
- `isActive`
- `items`

### Gợi ý UI

Format hiển thị:

- `Tiền mặt hỗ trợ: 300.000 đ`
- nếu `cashSupportAmount = 0` thì có thể hiển thị `Không hỗ trợ tiền mặt`

---

## API 04 — Update relief package có hỗ trợ tiền mặt

### Endpoint

`PATCH /api/relief/campaigns/{campaignId}/packages/{reliefPackageDefinitionId}`

### Request body mẫu

```json
{
  "name": "Gói hỗ trợ khẩn cấp A - cập nhật",
  "description": "Tăng thêm mức hỗ trợ tiền mặt",
  "cashSupportAmount": 500000,
  "isDefault": true,
  "isActive": true,
  "items": [
    {
      "supplyItemId": "7c2ed6d6-9707-46cc-a242-f6f7d0456a21",
      "quantity": 10,
      "unit": "kg"
    }
  ]
}
```

### FE cần hiểu

- API này hỗ trợ update `cashSupportAmount`
- API này cũng hỗ trợ update `outputSupplyItemId`
- nếu user sửa package và đổi mức tiền mặt, những delivery mới / cập nhật package sau đó sẽ lấy mức mới
- FE nên reload package detail/list sau khi update thành công

### Sau khi sửa `outputSupplyItemId`, FE cần lưu ý

Backend hiện xử lý như sau:

- nếu FE **gửi** `outputSupplyItemId`:
  - backend validate ID đó có tồn tại không
  - không cho phép dùng `Guid.Empty`
- nếu FE **không gửi** `outputSupplyItemId` khi update:
  - backend sẽ dùng `outputSupplyItemId` hiện tại của package để validate

Điều này dẫn đến 2 lưu ý quan trọng cho FE:

1. Nếu user **không đổi output item**, FE vẫn nên hiển thị output item hiện tại rõ ràng trong form edit.
2. Nếu user sửa `items[]`, FE phải tránh cho user chọn một component trùng với output item hiện tại, kể cả khi user không sửa field `outputSupplyItemId`.

### Khuyến nghị implement FE khi edit package

- load đầy đủ package detail/list item cũ
- prefill `outputSupplyItemId`
- khi render dropdown chọn component items, disable option đang là `outputSupplyItemId`
- nếu user đổi `outputSupplyItemId`, revalidate lại toàn bộ `items[]`
- nếu user thêm một component trùng output item, chặn ngay ở FE trước khi submit

### Lỗi FE có thể gặp liên quan `outputSupplyItemId`

- `OutputSupplyItemId is required.`
- `OutputSupplyItemId cannot be an empty GUID.`
- `Output supply item '{id}' was not found.`
- `Output supply item cannot be used as a package component.`

---

## API 05 — Assign household vào package

### Endpoint

`PATCH /api/relief/campaigns/{campaignId}/households/{campaignHouseholdId}/assign`

### Mục đích

API này không nhận `cashSupportAmount` trực tiếp, nhưng household delivery được tạo/đồng bộ sẽ mang snapshot tiền theo package đã chọn.

### Request body FE thường dùng

Tùy DTO hiện có của màn assign, FE chủ yếu chọn:

- package
- delivery mode
- distribution point hoặc team
- notes

### FE cần hiểu

- khi assign household vào package có `cashSupportAmount`, delivery tương ứng sẽ có giá trị tiền mặt đi kèm
- FE có thể đọc giá trị này từ API deliveries sau đó

---

## API 06 — Get deliveries

### Endpoint

- `GET /api/relief/campaigns/{campaignId}/deliveries`
- `GET /api/relief/campaigns/{campaignId}/deliveries/{householdDeliveryId}`

### Mục đích

FE dùng để render danh sách/lịch sử giao hàng.

### Field mới FE nên dùng

Trong `HouseholdDeliveryResponse` có:

```json
{
  "householdDeliveryId": "...",
  "reliefPackageDefinitionId": "...",
  "reliefPackageDefinitionName": "Gói hỗ trợ khẩn cấp A",
  "cashSupportAmount": 300000,
  "status": 1,
  "scheduledAt": "2026-04-20T10:30:00.000Z",
  "deliveredAt": null,
  "notes": "...",
  "proofs": []
}
```

### FE nên hiển thị

- tên package
- số tiền mặt của delivery
- trạng thái delivered / pending
- proof nếu đã complete

---

## API 07 — Complete một delivery có cash support

### Endpoint

`POST /api/relief/campaigns/{campaignId}/deliveries/{householdDeliveryId}/complete`

### Khi FE gọi

Khi user xác nhận đã giao hàng cho 1 household.

### Request body

```json
{
  "reliefPackageDefinitionId": "f11e4f39-3b8e-4b1d-b4b4-49102089d1b3",
  "campaignTeamId": "54b2ab2e-bfaf-4d1a-a7fd-729fd8db08d8",
  "cashSupportAmount": 300000,
  "notes": "Đã giao đủ hàng và tiền mặt",
  "proofNote": "Ảnh xác nhận tại hiện trường",
  "proofFileUrl": "https://cdn.example.com/proofs/delivery-001.jpg",
  "proofContentType": "image/jpeg"
}
```

### Field rules

- `proofFileUrl`: bắt buộc, tối đa 1000 ký tự
- `proofContentType`: optional, tối đa 200 ký tự
- `cashSupportAmount`: optional, nếu gửi thì phải `>= 0`
- `reliefPackageDefinitionId`: optional
- `campaignTeamId`: optional

### Cách backend hiểu `cashSupportAmount`

- nếu FE **không gửi** `cashSupportAmount`: backend dùng giá trị đang có trên delivery / package snapshot
- nếu FE **có gửi** `cashSupportAmount`: backend dùng giá trị FE gửi làm số tiền phát thực tế

### Response body

Response là `HouseholdDeliveryResponse`, trong đó FE cần quan tâm:

```json
{
  "householdDeliveryId": "b96ec09e-fcbf-4ca5-8f18-9e8fd7be9821",
  "campaignId": "7d0d1b0b-8f2c-4f14-9f65-1f44b4fd83a1",
  "campaignHouseholdId": "12d7b4d3-4d92-4ce4-a08e-3151da319f18",
  "campaignTeamId": "54b2ab2e-bfaf-4d1a-a7fd-729fd8db08d8",
  "reliefPackageDefinitionId": "f11e4f39-3b8e-4b1d-b4b4-49102089d1b3",
  "reliefPackageDefinitionName": "Gói hỗ trợ khẩn cấp A",
  "deliveryMode": 1,
  "status": 2,
  "cashSupportAmount": 300000,
  "scheduledAt": "2026-04-20T10:30:00.000Z",
  "deliveredAt": "2026-04-20T11:05:00.000Z",
  "notes": "Đã giao đủ hàng và tiền mặt",
  "proofs": [
    {
      "householdDeliveryProofId": "47f8f608-56f9-46dd-b0dd-d4f2b3a42b3a",
      "fileUrl": "https://cdn.example.com/proofs/delivery-001.jpg",
      "fileType": "image/jpeg",
      "note": "Ảnh xác nhận tại hiện trường",
      "capturedAt": "2026-04-20T11:05:00.000Z",
      "capturedByUserId": "ceabf59f-5c53-4ac2-8e4f-1f7f51bd8f84"
    }
  ]
}
```

### Business rules FE cần biết

- delivery phải thuộc đúng campaign
- delivery đã `Delivered` rồi thì không complete lại được
- campaign phải còn đủ ngân sách để chi `cashSupportAmount`
- nếu không đủ ngân sách, request fail và delivery không được complete

### Gợi ý UX

- cho phép prefill `cashSupportAmount` theo package
- user có thể sửa lại nếu phát thực tế khác package chuẩn
- disable submit nếu chưa có proof URL
- sau khi success, reload delivery list và campaign budget summary nếu có hiển thị

---

## API 08 — Complete batch deliveries

### Endpoint

`POST /api/relief/campaigns/{campaignId}/deliveries/complete-batch`

### Khi FE gọi

Khi user muốn complete nhiều household trong cùng một thao tác.

### Request body

```json
{
  "items": [
    {
      "householdDeliveryId": "b96ec09e-fcbf-4ca5-8f18-9e8fd7be9821",
      "reliefPackageDefinitionId": "f11e4f39-3b8e-4b1d-b4b4-49102089d1b3",
      "campaignTeamId": "54b2ab2e-bfaf-4d1a-a7fd-729fd8db08d8",
      "cashSupportAmount": 300000,
      "notes": "Đã giao đợt 1",
      "proofs": [
        {
          "fileUrl": "https://cdn.example.com/proofs/batch-001.jpg",
          "fileType": "image/jpeg",
          "note": "Ảnh bàn giao"
        }
      ]
    },
    {
      "householdDeliveryId": "5cf0c154-3f0a-4326-9b42-ecb854c56c02",
      "cashSupportAmount": 500000,
      "notes": "Đã giao đợt 1",
      "proofs": [
        {
          "fileUrl": "https://cdn.example.com/proofs/batch-002.jpg",
          "fileType": "image/jpeg",
          "note": "Ảnh xác nhận"
        }
      ]
    }
  ]
}
```

### Field rules

- `items`: bắt buộc, ít nhất 1 item
- mỗi item phải có:
  - `householdDeliveryId`
  - `proofs` với ít nhất 1 proof
- `cashSupportAmount`: optional, nếu gửi thì phải `>= 0`

### Response body

```json
{
  "totalRequested": 2,
  "successCount": 1,
  "failureCount": 1,
  "items": [
    {
      "householdDeliveryId": "b96ec09e-fcbf-4ca5-8f18-9e8fd7be9821",
      "isSuccess": true,
      "error": null,
      "delivery": {
        "householdDeliveryId": "b96ec09e-fcbf-4ca5-8f18-9e8fd7be9821",
        "cashSupportAmount": 300000,
        "status": 2,
        "proofs": []
      }
    },
    {
      "householdDeliveryId": "5cf0c154-3f0a-4326-9b42-ecb854c56c02",
      "isSuccess": false,
      "error": "Campaign does not have enough remaining budget for this cash support.",
      "delivery": null
    }
  ]
}
```

### FE cần hiểu

- batch này là **partial success**
- từng item có thể success/fail độc lập
- FE nên hiển thị kết quả theo từng household, không nên assume tất cả cùng thành công

### Gợi ý UX

- sau khi batch complete, render bảng result gồm:
  - householdDeliveryId
  - trạng thái
  - lỗi nếu có
- có thể cho user retry riêng các item fail

---

## 4. Những field mới FE cần thêm vào model

## 4.1 ReliefPackageDefinitionResponse

Thêm field:

```ts
cashSupportAmount: number;
```

## 4.2 HouseholdDeliveryResponse

Thêm field:

```ts
cashSupportAmount: number;
```

## 4.3 Complete delivery payload

Thêm field:

```ts
cashSupportAmount?: number;
```

## 4.4 Complete batch delivery payload

Trong mỗi item thêm field:

```ts
cashSupportAmount?: number;
```

## 4.5 Extract budget payload/response

Payload:

```ts
type ExtractCampaignBudgetRequest = {
  targetReliefCampaignId: string;
  amount: number;
  note?: string | null;
};
```

Response:

```ts
type CampaignBudgetTransferResponse = {
  campaignBudgetTransferId: string;
  sourceCampaignId: string;
  targetCampaignId: string;
  amount: number;
  transferredByUserId?: string | null;
  transferredAt: string;
  note?: string | null;
  sourceRemainingBudget: number;
  targetRemainingBudget: number;
};
```

---

## 5. Gợi ý validation phía FE

## Budget extraction form

- `targetReliefCampaignId` bắt buộc
- `amount > 0`
- `note.length <= 1000`

## Create/update package form

- `name` bắt buộc
- `outputSupplyItemId` bắt buộc
- `cashSupportAmount >= 0`
- ít nhất 1 component item
- không cho chọn duplicate `supplyItemId`

## Complete delivery form

- phải có proof
- `cashSupportAmount >= 0`
- nếu FE cho phép edit tiền thực tế thì nên format số tiền rõ ràng trước khi submit

## Complete batch form

- mỗi item phải có ít nhất 1 proof
- các item không hợp lệ nên bị chặn submit từ FE trước

---

## 6. Các lỗi nghiệp vụ FE có thể gặp

FE nên chuẩn bị hiển thị message cho các case sau:

- `Amount must be greater than 0`
- `Target campaign must be a relief campaign`
- `Source campaign must be a fundraising campaign`
- `Amount exceeds remaining fundraising budget`
- `CashSupportAmount cannot be negative`
- `OutputSupplyItemId is required`
- `OutputSupplyItemId cannot be an empty GUID`
- `Output supply item cannot be used as a package component`
- `Campaign does not have enough remaining budget for this cash support`
- `Delivery has already been completed`
- `ProofFileUrl is required`

> Text lỗi thực tế có thể khác đôi chút tùy exception middleware map ra response như thế nào. FE nên fallback về generic error message nếu backend không trả text đúng kỳ vọng.

---

## 7. Checklist cho FE

- thêm form extract budget ở campaign fundraising detail
- thêm field `cashSupportAmount` vào create/update package form
- hiển thị `cashSupportAmount` trong package list/detail
- hiển thị `cashSupportAmount` trong delivery list/detail
- thêm input override tiền mặt ở complete delivery modal/form
- thêm `cashSupportAmount` vào batch complete payload nếu UI hỗ trợ
- xử lý partial success cho batch complete API

---

## 8. Kết luận

Frontend chỉ cần nhớ 3 ý chính:

1. **Budget extraction** dùng để cấp tiền từ fundraising sang relief.
2. **`cashSupportAmount`** là một phần của relief package và delivery.
3. Khi **complete delivery**, backend sẽ kiểm tra ngân sách còn lại trước khi trừ tiền và đánh dấu delivered.
