## Context

Backend hiện có nhiều controller và DTO theo module (Auth, User, Team, Volunteer, Inventory, Supply, Rescue, Relief Station, Vehicle, Skill, Location). Dù đã có Swagger runtime, contract hành vi chưa được chuẩn hóa thành OpenSpec để làm nguồn sự thật cho thiết kế, kiểm thử, và tích hợp client. Hiện trạng có nhiều điểm không đồng nhất giữa module: status code cho create/delete, response body shape, error shape, route naming, và authorization policy trên endpoint mutate.

## Goals / Non-Goals

**Goals:**
- Xây dựng capability OpenSpec `backend-api-contract` mô tả hợp đồng hành vi API tổng cho toàn backend hiện tại.
- Chuẩn hóa yêu cầu ở mức behavior: endpoint catalog, auth/authz tối thiểu, request binding, response/status semantics, error semantics, pagination/filter, route conventions.
- Tạo checklist công việc để đối chiếu implementation hiện tại với spec và lấp khoảng trống.

**Non-Goals:**
- Không refactor hoặc đổi logic nghiệp vụ trong code ở change này.
- Không tái thiết kế kiến trúc data/service layer.
- Không mở rộng tính năng mới ngoài phạm vi chuẩn hóa contract hiện hữu.

## Decisions

### 1) Dùng một capability spec tổng cho API contract
**Quyết định:** Tạo duy nhất capability `backend-api-contract` trong change này thay vì tách theo từng domain.

**Lý do:**
- Mục tiêu trước mắt là baseline đồng bộ toàn hệ thống.
- Giảm overhead tạo nhiều capability nhỏ khi chưa có spec nền.

**Alternatives considered:**
- Tách 10+ capability theo module: chi tiết hơn nhưng chậm và khó hoàn thành một lượt.

### 2) Định nghĩa requirement ở mức chuẩn hóa hành vi, không bám vào từng field endpoint
**Quyết định:** Requirement tập trung vào chuẩn bắt buộc (status code, authorization, response/error shape, route conventions), đồng thời yêu cầu có endpoint catalog đầy đủ.

**Lý do:**
- Phù hợp giai đoạn bootstrap OpenSpec.
- Giữ spec ổn định dù field-level DTO thay đổi nhỏ theo business.

**Alternatives considered:**
- Ghi đầy đủ field-level schema cho mọi endpoint ngay trong lần đầu: độ chính xác cao nhưng tốn nhiều thời gian, khó maintain ở lần đầu.

### 3) Chốt chuẩn HTTP semantics thống nhất
**Quyết định:**
- Create SHALL trả `201 Created` kèm định danh resource.
- Delete SHALL trả `204 No Content` nếu không cần body.

**Lý do:**
- Giảm không nhất quán hiện trạng và giúp client dự đoán chính xác hành vi.

**Alternatives considered:**
- Giữ nguyên per-module status code hiện tại: ít thay đổi nhưng tiếp tục tạo contract drift.

### 4) Chốt chuẩn error contract thống nhất toàn API
**Quyết định:** Mọi lỗi SHALL tuân theo một schema lỗi thống nhất, có mã trạng thái + thông điệp + trace/correlation id.

**Lý do:**
- Dễ observability, logging, client-side handling, và test tự động.

**Alternatives considered:**
- Cho phép mỗi module trả lỗi riêng: nhanh nhưng tăng chi phí tích hợp và vận hành.

### 5) Bắt buộc authorization policy rõ ràng cho mutating endpoints
**Quyết định:** Endpoint mutate SHALL khai báo policy/role hoặc lý do explicit nếu cho phép anonymous.

**Lý do:**
- Đóng khoảng trống bảo mật đang thấy ở một số controller.

**Alternatives considered:**
- Chỉ review thủ công: dễ bỏ sót và không tạo tiêu chuẩn lâu dài.

## Risks / Trade-offs

- **[Risk]** Spec chuẩn hóa có thể khác implementation hiện tại ở nhiều điểm → **Mitigation:** tasks tách rõ: lập inventory khác biệt, ưu tiên fix theo rủi ro cao trước.
- **[Risk]** Scope toàn bộ API lớn, dễ thiếu endpoint/DTO trong lần đầu → **Mitigation:** tạo endpoint matrix + review chéo theo controller list.
- **[Risk]** Chuẩn hóa status/error có thể ảnh hưởng backward compatibility client → **Mitigation:** đánh dấu breaking change, rollout theo module và truyền thông rõ timeline.
- **[Risk]** Áp đặt chuẩn quá sớm khi nghiệp vụ còn đổi nhanh → **Mitigation:** spec tập trung vào nguyên tắc contract cốt lõi, tránh over-spec chi tiết không ổn định.

## Migration Plan

1. Hoàn tất proposal + specs + tasks cho `backend-api-contract`.
2. Đối chiếu toàn bộ controller hiện tại với requirement matrix.
3. Tạo danh sách gap theo mức độ: security-critical, contract-critical, cosmetic.
4. Triển khai cập nhật code theo từng module, ưu tiên security và error contract.
5. Cập nhật Swagger annotations và integration tests để phản ánh contract mới.
6. Rollout theo giai đoạn; theo dõi telemetry/log và rollback bằng cách giữ compatibility path tạm thời khi cần.

## Open Questions

- Có cần chốt ngay response envelope dạng `ApiResponse<T>` cho toàn bộ success responses không?
- Có cần version API (`/api/v2`) cho các thay đổi breaking về status/error hay dùng compatibility mode tạm thời?
- Error schema sẽ map domain/business errors theo mã lỗi chuẩn nào (string code set) hay chỉ dựa HTTP status?
