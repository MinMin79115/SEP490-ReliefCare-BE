## Why

Backend API đã có nhiều module và endpoint nhưng chưa có một đặc tả thống nhất theo OpenSpec để đội ngũ triển khai, test, và tích hợp client dựa vào cùng một hợp đồng hành vi. Cần chuẩn hóa đặc tả ngay bây giờ để giảm sai lệch giữa implementation, tài liệu, và kỳ vọng của các bên liên quan.

## What Changes

- Tạo change OpenSpec mô tả **hợp đồng API tổng** cho toàn bộ backend hiện có.
- Bổ sung capability mới cho đặc tả endpoint catalog, quy tắc authorization, và chuẩn response/error ở mức hành vi.
- Định nghĩa các yêu cầu bắt buộc cho các nhóm endpoint chính: Auth, User, Team, Volunteer Profile, Inventory, Supply Allocation, Rescue Request, Relief Station, Vehicle, VehicleType, Skill, Location.
- Thiết lập checklist triển khai để đối chiếu code hiện tại với đặc tả và lấp khoảng trống tài liệu/behavior.

## Capabilities

### New Capabilities
- `backend-api-contract`: Đặc tả chuẩn hóa toàn bộ hợp đồng hành vi API backend (route/method, authz, request/response, và error semantics).

### Modified Capabilities
- (none)

## Impact

- Ảnh hưởng trực tiếp tới quy trình phát triển API trong `ReliefManagementSystem.API/Controllers`.
- Ảnh hưởng tới tầng Application/Infrastructure do yêu cầu đồng nhất hành vi response, lỗi, và phân quyền.
- Tạo nền tảng cho test contract/regression và đồng bộ tài liệu tích hợp client.
