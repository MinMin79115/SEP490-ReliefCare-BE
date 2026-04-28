# SEP490-ReliefCare-BE

Backend cho hệ thống **ReliefCare** - nền tảng quản lý cứu trợ thiên tai, điều phối cứu hộ, quản lý chiến dịch, quyên góp, kho vật tư, phân phối hàng cứu trợ và phân tích mức độ thiên tai.

## 1. Mục tiêu dự án

Project này cung cấp API backend cho các nghiệp vụ chính:

- Xác thực người dùng, phân quyền và quản lý tài khoản
- Quản lý chiến dịch cứu trợ/cứu hộ
- Tiếp nhận và xử lý yêu cầu cứu hộ
- Quản lý kho, điều chuyển vật tư, cấp phát hàng hóa
- Quản lý điểm phát, hộ dân và giao hàng cứu trợ
- Tiếp nhận quyên góp và tích hợp thanh toán
- Quản lý đội cứu trợ, tình nguyện viên, phương tiện
- Gửi thông báo và realtime token
- Phân tích thiên tai bằng dữ liệu thời tiết + LLM

## 2. Kiến trúc solution

Solution được tổ chức theo kiểu nhiều lớp:

```text
ReliefManagementSystem.API.sln
├── ReliefManagementSystem.API/
├── ReliefManagementSystem.Application/
├── ReliefManagementSystem.Domain/
└── ReliefManagementSystem.Infrastructure/
```

### `ReliefManagementSystem.API`

Layer Web API, entrypoint của hệ thống.

- Chứa `Program.cs`
- Chứa toàn bộ controllers
- Cấu hình Swagger, JWT, CORS, rate limit, middleware

### `ReliefManagementSystem.Application`

Layer nghiệp vụ/use case.

- Chứa services
- Chứa interfaces/contracts
- Chứa DTOs theo từng feature
- Chứa validation và models cấu hình dùng trong app

### `ReliefManagementSystem.Domain`

Layer domain model.

- Chứa entities
- Chứa enums
- Chứa domain/common classes

### `ReliefManagementSystem.Infrastructure`

Layer persistence và tích hợp ngoài.

- EF Core + PostgreSQL
- Repositories
- Identity/JWT helpers
- Email, Cloudinary, PayOS, Goong, Weather, Centrifugo, LLM provider

## 3. Các module nghiệp vụ chính

Dựa trên các controller trong `ReliefManagementSystem.API/Controllers`:

### Authentication & User

- `AuthController`
- `UserController`

Chức năng:

- Đăng ký / đăng nhập / refresh token / logout
- OTP xác thực email
- Quên mật khẩu bằng OTP
- Google OAuth
- Quản lý profile, manager, moderator, ban/unban user

### Campaign Management

- `CampaignController`
- `CampaignTaskController`

Chức năng:

- Tạo và quản lý chiến dịch
- Gắn trạm cứu trợ vào campaign
- Gán team vào campaign
- Đăng ký volunteer vào campaign
- Campaign inventory, campaign tasks, budget extraction

### Rescue Management

- `RescueRequestController`

Chức năng:

- Tạo yêu cầu cứu hộ
- Xác minh yêu cầu
- Ưu tiên hóa và điều phối cứu hộ
- Gán team / vehicle / batch cứu hộ
- Cập nhật trạng thái operation

### Relief Distribution

- `ReliefDistributionController`

Chức năng:

- Import hộ dân thuộc chiến dịch
- Tạo điểm phát tập trung
- Gán hộ dân vào điểm phát / team
- Định nghĩa gói cứu trợ
- Đóng gói cứu trợ
- Hoàn tất giao hàng cứu trợ
- Quản lý shortage request

### Donation / Fund / Payment

- `DonationController`
- `FundController`

Chức năng:

- Tạo checkout donation
- Theo dõi trạng thái thanh toán
- Nhận webhook từ PayOS
- Reconcile / cancel donation
- Tổng hợp fund, contribution, fund transaction

### Inventory / Supply / Logistics

- `InventoryController`
- `InventoryTransactionController`
- `SupplyItemController`
- `SupplyTransferController`
- `SupplyAllocationController`

Chức năng:

- Quản lý kho và tồn kho
- Nhập/xuất/điều chỉnh transaction
- Quản lý vật phẩm
- Điều chuyển hàng giữa các trạm
- Phân bổ vật tư cho chiến dịch / mục tiêu

### Team / Volunteer / Station / Vehicle

- `TeamController`
- `TeamJoinRequestController`
- `StationJoinRequestController`
- `ReliefStationController`
- `VolunteerProfileController`
- `VehicleController`
- `VehicleTypeController`
- `SkillController`

Chức năng:

- Quản lý đội cứu trợ
- Quản lý thành viên, tracking vị trí đội
- Team xin vào trạm / volunteer xin vào team
- Quản lý hồ sơ tình nguyện viên và kỹ năng
- Quản lý phương tiện và loại phương tiện

### Notification / Realtime / Disaster Analysis

- `NotificationController`
- `RealtimeController`
- `DisasterAnalysisController`
- `PriorityCriteriaController`
- `ProcurementController`
- `LocationController`

Chức năng:

- Thông báo và realtime token
- Phân tích mức độ thiên tai
- Tiêu chí ưu tiên
- Procurement order
- Dữ liệu location phân cấp

## 4. Tech stack

### Nền tảng chính

- **.NET 8**
- **ASP.NET Core Web API**
- **Entity Framework Core 8**
- **PostgreSQL / Npgsql**
- **ASP.NET Core Identity**
- **JWT Bearer Authentication**
- **Google OAuth**
- **Swagger / OpenAPI**
- **FluentValidation**

### Tích hợp ngoài

- **Centrifugo**: realtime
- **Cloudinary**: upload ảnh
- **Brevo SMTP / Mail**: gửi email/OTP
- **PayOS**: thanh toán quyên góp
- **Goong**: bản đồ / khoảng cách / định tuyến
- **WeatherAPI** + **Visual Crossing**: dữ liệu thời tiết
- **LLM Provider**: phân tích thiên tai kiểu OpenAI-compatible

## 5. Entry point và khởi động hệ thống

Entry point chính nằm tại:

```text
ReliefManagementSystem.API/Program.cs
```

Khi app khởi động, hệ thống sẽ:

- Đăng ký controllers, Swagger, health checks
- Đăng ký DbContext, Identity, JWT, Google auth
- Bind các config section: JWT, Email, Cloudinary, PayOS, Goong, Weather, Centrifugo, LLM...
- Bật rate limiting cho một số endpoint
- Auto migrate database
- Seed dữ liệu mặc định

Health endpoints:

- `GET /health`
- `GET /healthz`

Swagger được bật trong môi trường `Development` và `Staging`.

## 6. Chạy local bằng Docker Compose

File local compose:

```text
compose.yaml
```

Các service local chính:

- `postgres`
- `centrifugo`
- `reliefcare-api`
- `pgadmin`

### Lệnh chạy

```bash
docker compose up --build
```

Chạy background:

```bash
docker compose up --build -d
```

Dừng:

```bash
docker compose down
```

Dừng và xóa volume:

```bash
docker compose down -v
```

Xem log:

```bash
docker compose logs -f
```

### Các địa chỉ local mặc định

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- PostgreSQL: `localhost:5432`
- pgAdmin: `http://localhost:5050`
- Centrifugo: `http://localhost:8000`

## 7. Chạy local không dùng Docker

Project API có profile launch trong:

```text
ReliefManagementSystem.API/Properties/launchSettings.json
```

Bạn có thể chạy trực tiếp bằng:

```bash
dotnet restore
dotnet build
dotnet run --project ReliefManagementSystem.API
```

Hoặc mở solution bằng Visual Studio / Rider và chạy project `ReliefManagementSystem.API`.

Theo launch settings hiện tại, app local sẽ chạy gần giống:

- `http://localhost:5205`

## 8. Cấu hình môi trường

### Local Docker

`compose.yaml` dùng biến môi trường từ file:

```text
.env
```

Bạn cần chuẩn bị tối thiểu:

- PostgreSQL config
- JWT config
- Centrifugo API key / secret
- Email config
- Cloudinary config
- Goong API key
- Weather API key
- LLM provider config
- PayOS config

### Các nhóm config chính

Trong `Program.cs`, app bind các section sau:

- `ConnectionStrings:Default`
- `Jwt`
- `AuthenticationGoogle`
- `Centrifugo`
- `CloudinarySettings`
- `EmailSettings`
- `PayOs`
- `Goong`
- `WeatherApi`
- `VisualCrossing`
- `DisasterAnalysis`
- `LlmProvider`
- `CorsSettings`

## 9. Seed dữ liệu và migration

Khi app khởi động, backend sẽ tự động:

- chạy EF Core migration
- seed role
- seed user
- seed skill
- seed team
- seed location
- seed relief station
- seed supply item
- seed vehicle type / vehicle
- seed campaign
- seed test campaign
- seed manager profile
- seed priority criteria

Điều này có nghĩa là nếu DB hoặc config có lỗi, app có thể fail ngay khi startup.

## 10. Deploy

Repo có workflow deploy cho staging và production:

```text
.github/workflows/deploy-staging.yaml
.github/workflows/deploy-production.yaml
```

Ngoài local compose, repo còn có:

- `compose.staging.yaml`
- `compose.production.yaml`

Các môi trường này có thể dùng thêm:

- GHCR image
- Cloudflare Tunnel
- Centrifugo
- PostgreSQL
- pgAdmin (staging)

## 11. Lưu ý quan trọng về bảo mật

Hiện tại file `ReliefManagementSystem.API/appsettings.json` đang chứa nhiều giá trị cấu hình nhạy cảm như:

- database password
- Google OAuth client secret
- SMTP credentials
- Cloudinary keys
- PayOS keys
- Goong API key
- Weather API key

> Khuyến nghị mạnh: **không commit secret thật vào repo**.

Nên chuyển toàn bộ secret sang:

- `.env`
- user secrets
- secret manager của CI/CD
- biến môi trường trên server

Nếu các secret này từng được commit công khai, nên **rotate toàn bộ**.

## 12. Gợi ý luồng nghiệp vụ chính để đọc code

Nếu bạn mới vào project, nên đọc theo thứ tự sau:

1. **Authentication**
   - `AuthController`
   - `AuthService`
   - `IdentityAuthService`
   - `TokenService`

2. **Campaign**
   - `CampaignController`
   - `CampaignService`

3. **Rescue**
   - `RescueRequestController`
   - `RescueRequestService`

4. **Relief Distribution**
   - `ReliefDistributionController`
   - `ReliefDistributionService`

5. **Donation / Payment**
   - `DonationController`
   - `DonationService`
   - `PayOsGateway`

6. **Inventory / Supply Transfer**
   - `Inventory*Controller`
   - `SupplyTransferController`
   - các services tương ứng

## 13. Tóm tắt

ReliefCare-BE là một backend .NET 8 có phạm vi nghiệp vụ lớn, tập trung vào:

- cứu hộ khẩn cấp
- cứu trợ hàng hóa
- chiến dịch và tình nguyện viên
- kho và logistics
- quyên góp và thanh toán
- phân tích thiên tai

Kiến trúc dự án rõ theo 4 layer và đã tích hợp khá nhiều external services để phục vụ vận hành thực tế.
