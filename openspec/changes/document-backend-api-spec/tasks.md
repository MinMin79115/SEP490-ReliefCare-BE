## 1. Baseline Contract Inventory

- [ ] 1.1 Lập endpoint matrix chính thức cho toàn bộ controllers (method, route, auth mode, request binding, response semantics)
- [ ] 1.2 Rà soát và chuẩn hóa danh mục mutating endpoints công khai/thiếu policy thành danh sách vi phạm có mức độ ưu tiên
- [ ] 1.3 Chốt route naming convention chuẩn (canonical style) và lập danh sách endpoint lệch chuẩn

## 2. HTTP Semantics Standardization

- [ ] 2.1 Chuẩn hóa create endpoints sang `201 Created` kèm resource identifier/location theo contract
- [ ] 2.2 Chuẩn hóa delete endpoints sang `204 No Content` cho các trường hợp không cần payload
- [ ] 2.3 Cập nhật annotation/ProducesResponseType để phản ánh đúng hành vi trả về sau chuẩn hóa

## 3. Authorization Contract Enforcement

- [ ] 3.1 Áp policy/role rõ ràng cho toàn bộ mutating endpoints nội bộ đang chỉ dùng `[Authorize]` chung
- [ ] 3.2 Bổ sung/điều chỉnh `[Authorize]` cho các mutating endpoints hiện chưa bảo vệ
- [ ] 3.3 Tài liệu hóa ngoại lệ `AllowAnonymous` hợp lệ (lý do nghiệp vụ + ràng buộc bảo vệ bổ sung)

## 4. Unified Response and Error Contract

- [ ] 4.1 Định nghĩa schema lỗi thống nhất (status, code, message, trace/correlation id, details)
- [ ] 4.2 Đồng nhất đường đi xử lý lỗi về middleware toàn cục, giảm xử lý ad-hoc trong controller
- [ ] 4.3 Chuẩn hóa success payload shape theo nhóm endpoint (single resource, collection, paginated)

## 5. Documentation and Verification

- [ ] 5.1 Cập nhật OpenAPI/Swagger docs để khớp với backend-api-contract sau khi chuẩn hóa
- [ ] 5.2 Bổ sung integration/contract test cho các quy tắc bắt buộc (auth, status code, error schema)
- [ ] 5.3 Chạy `openspec validate --change document-backend-api-spec --strict` và xử lý toàn bộ lỗi validation
