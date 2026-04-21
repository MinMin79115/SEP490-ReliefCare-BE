# Frontend Handoff - Relief Campaign Task & Member Task APIs

Tài liệu này mô tả phần **campaign task** và **member task** để frontend có thể build màn hình giao việc trong campaign Relief.

---

## 1. Mục tiêu nghiệp vụ

Module này dùng để:

- tạo task cấp campaign/team
- theo dõi trạng thái task
- gán sub-task cho từng volunteer trong team
- xem tiến độ task qua số lượng member task đã hoàn thành

Phạm vi hiện tại của backend:

- quản lý **campaign task**
- assign **member task** cho volunteer
- xem detail task kèm danh sách member tasks

Backend hiện **chưa có API riêng để FE update status của từng member task** trong controller này.

---

## 2. Khái niệm FE cần hiểu

### 2.1 Campaign Task

Là task cấp campaign, thường gắn với một `CampaignTeam`.

Ví dụ:

- Phát quà tại điểm A
- Khảo sát hộ dân khu vực B
- Vận chuyển hàng đến trạm C

### 2.2 Member Task

Là sub-task giao cho một volunteer cụ thể bên trong campaign task.

Ví dụ:

- Gọi điện xác nhận danh sách hộ
- Chụp ảnh hiện trường
- Bàn giao hàng cho 10 hộ đầu tiên

---

## 3. Giới hạn nghiệp vụ quan trọng

FE cần biết các rule sau:

- task **chỉ hỗ trợ cho campaign loại Relief**
- `dueDate` không được nhỏ hơn `startDate`
- task có status `Completed` hoặc `Cancelled` thì không được update nữa
- chỉ task `Planned` hoặc `Cancelled` mới được delete
- việc chuyển sang `InProgress`, `Blocked`, `Completed` chỉ được phép khi campaign cha đang ở trạng thái `Active`
- volunteer được assign member task phải là thành viên của đúng team đang sở hữu campaign task đó

---

## 4. API overview

Base route:

`/api/campaigns`

Tất cả endpoint đều cần auth.

### Danh sách endpoint

| Method | Endpoint | Mục đích |
|---|---|---|
| POST | `/api/campaigns/{campaignId}/tasks` | Tạo campaign task |
| GET | `/api/campaigns/{campaignId}/tasks` | Lấy danh sách task theo campaign |
| GET | `/api/campaigns/tasks/{campaignTaskId}` | Lấy detail 1 task |
| PUT | `/api/campaigns/tasks/{campaignTaskId}` | Cập nhật task |
| PATCH | `/api/campaigns/tasks/{campaignTaskId}/status` | Đổi trạng thái task |
| POST | `/api/campaigns/tasks/{campaignTaskId}/members` | Assign member task cho volunteer |
| DELETE | `/api/campaigns/tasks/{campaignTaskId}` | Xóa task |

---

## 5. API Contracts

## API 01 — Create campaign task

### Endpoint

`POST /api/campaigns/{campaignId}/tasks`

### Request body

```json
{
  "campaignTeamId": "5af06b55-73d4-4eb4-b8f5-6cfd13d1f201",
  "title": "Phát quà tại điểm tập kết A",
  "description": "Team chịu trách nhiệm phát 200 suất quà trong ngày 21/04",
  "startDate": "2026-04-21T07:00:00Z",
  "dueDate": "2026-04-21T17:00:00Z",
  "priority": 1
}
```

### Field rules

- `campaignTeamId`: bắt buộc
- `title`: bắt buộc, tối đa 200 ký tự
- `description`: optional, tối đa 2000 ký tự
- `dueDate >= startDate`
- `priority`: enum `TaskPriority`

### Response body

```json
{
  "campaignTaskId": "a2cc5f34-50f0-4615-99d5-74b5a4f0c528",
  "campaignId": "2f9375b0-e7eb-49f0-b64a-f7e6c1dd0df9",
  "campaignTeamId": "5af06b55-73d4-4eb4-b8f5-6cfd13d1f201",
  "campaignTeamName": "Đội điều phối A",
  "title": "Phát quà tại điểm tập kết A",
  "description": "Team chịu trách nhiệm phát 200 suất quà trong ngày 21/04",
  "startDate": "2026-04-21T07:00:00Z",
  "dueDate": "2026-04-21T17:00:00Z",
  "status": 0,
  "priority": 1,
  "createdBy": "db466f91-92e1-45c0-b774-2b388418dfb1",
  "createdAt": "2026-04-20T09:30:00Z"
}
```

### FE nên hiểu

- task mới tạo sẽ có status mặc định là `Planned`
- response dùng luôn để update list local mà không cần refetch ngay nếu không muốn

---

## API 02 — Get paged campaign tasks

### Endpoint

`GET /api/campaigns/{campaignId}/tasks?pageIndex=1&pageSize=10&status=0&campaignTeamId={teamId}`

### Query params

- `pageIndex`: mặc định 1
- `pageSize`: mặc định 10
- `status`: optional
- `campaignTeamId`: optional

### Response shape

Response là `Pagination<CampaignTaskResponse>`.

FE trong mỗi item thường dùng:

- `campaignTaskId`
- `campaignTeamId`
- `campaignTeamName`
- `title`
- `description`
- `startDate`
- `dueDate`
- `status`
- `priority`
- `createdAt`

### Gợi ý UI

- filter theo team
- filter theo status
- sort local theo `startDate` hoặc `dueDate` nếu UI cần

---

## API 03 — Get task detail

### Endpoint

`GET /api/campaigns/tasks/{campaignTaskId}`

### Response body

Response là `CampaignTaskDetailResponse`:

```json
{
  "campaignTaskId": "a2cc5f34-50f0-4615-99d5-74b5a4f0c528",
  "campaignId": "2f9375b0-e7eb-49f0-b64a-f7e6c1dd0df9",
  "campaignTeamId": "5af06b55-73d4-4eb4-b8f5-6cfd13d1f201",
  "campaignTeamName": "Đội điều phối A",
  "title": "Phát quà tại điểm tập kết A",
  "description": "Team chịu trách nhiệm phát 200 suất quà trong ngày 21/04",
  "startDate": "2026-04-21T07:00:00Z",
  "dueDate": "2026-04-21T17:00:00Z",
  "status": 1,
  "priority": 1,
  "createdBy": "db466f91-92e1-45c0-b774-2b388418dfb1",
  "createdAt": "2026-04-20T09:30:00Z",
  "memberTaskCount": 3,
  "completedMemberTaskCount": 1,
  "memberTasks": [
    {
      "memberTaskId": "f8e3acac-d74f-4c88-831c-7fd1fce9e37b",
      "campaignTaskId": "a2cc5f34-50f0-4615-99d5-74b5a4f0c528",
      "volunteerProfileId": "36cbf8c2-8d85-43f2-b088-c49f69d8758c",
      "volunteerName": "Nguyen Van A",
      "subTaskTitle": "Bàn giao 50 suất đầu tiên",
      "taskNote": "Ưu tiên khu dân cư số 1",
      "assignedAt": "2026-04-20T10:00:00Z",
      "completedAt": null,
      "status": 0
    }
  ]
}
```

### FE nên dùng

- `memberTaskCount` và `completedMemberTaskCount` để render progress
- `memberTasks[]` để render danh sách người được giao việc

Ví dụ progress:

- `1 / 3 member tasks completed`

---

## API 04 — Update campaign task

### Endpoint

`PUT /api/campaigns/tasks/{campaignTaskId}`

### Request body

```json
{
  "title": "Phát quà tại điểm tập kết A - cập nhật",
  "description": "Cập nhật lại phạm vi công việc trong ngày",
  "startDate": "2026-04-21T07:30:00Z",
  "dueDate": "2026-04-21T18:00:00Z",
  "priority": 2
}
```

### FE cần biết

- không update được task đã `Completed` hoặc `Cancelled`
- nên disable nút edit nếu UI đã biết status hiện tại thuộc 2 trạng thái trên

---

## API 05 — Change campaign task status

### Endpoint

`PATCH /api/campaigns/tasks/{campaignTaskId}/status`

### Request body

```json
{
  "status": 1
}
```

### Luật chuyển trạng thái

- `Planned -> InProgress | Cancelled`
- `InProgress -> Blocked | Completed | Cancelled`
- `Blocked -> InProgress | Cancelled`
- các transition khác sẽ bị reject

### FE khuyến nghị

Không nên show mọi option status cho mọi task. Nên giới hạn dropdown/action theo trạng thái hiện tại:

- nếu `Planned`: chỉ cho chọn `InProgress`, `Cancelled`
- nếu `InProgress`: chỉ cho chọn `Blocked`, `Completed`, `Cancelled`
- nếu `Blocked`: chỉ cho chọn `InProgress`, `Cancelled`
- nếu `Completed` hoặc `Cancelled`: disable đổi trạng thái

### Rule quan trọng

Nếu chuyển sang `InProgress`, `Blocked`, `Completed` thì campaign cha phải đang `Active`.

Vì vậy FE nên:

- nếu đã có `campaign.status`, có thể disable sớm action
- nhưng vẫn phải xử lý case backend reject

---

## API 06 — Assign member task cho volunteer

### Endpoint

`POST /api/campaigns/tasks/{campaignTaskId}/members`

### Request body

```json
{
  "volunteerProfileId": "36cbf8c2-8d85-43f2-b088-c49f69d8758c",
  "subTaskTitle": "Bàn giao 50 suất đầu tiên",
  "taskNote": "Ưu tiên khu dân cư số 1"
}
```

### Field rules

- `volunteerProfileId`: bắt buộc
- `subTaskTitle`: bắt buộc, tối đa 200 ký tự
- `taskNote`: optional, tối đa 1000 ký tự

### Response body

```json
{
  "memberTaskId": "f8e3acac-d74f-4c88-831c-7fd1fce9e37b",
  "campaignTaskId": "a2cc5f34-50f0-4615-99d5-74b5a4f0c528",
  "volunteerProfileId": "36cbf8c2-8d85-43f2-b088-c49f69d8758c",
  "volunteerName": "Nguyen Van A",
  "subTaskTitle": "Bàn giao 50 suất đầu tiên",
  "taskNote": "Ưu tiên khu dân cư số 1",
  "assignedAt": "2026-04-20T10:00:00Z",
  "completedAt": null,
  "status": 0
}
```

### FE cần hiểu

- member task mới sẽ có status mặc định là `Assigned`
- volunteer được chọn phải thuộc đúng team của campaign task

### Gợi ý UX

- khi mở modal assign member task, FE nên filter volunteer theo team của task trước
- nếu đang có API members/team-members ở chỗ khác, nên chỉ cho chọn volunteer thuộc team đó

---

## API 07 — Delete campaign task

### Endpoint

`DELETE /api/campaigns/tasks/{campaignTaskId}`

### FE cần biết

- chỉ task `Planned` hoặc `Cancelled` mới xóa được
- nếu task đang `InProgress`, `Blocked`, `Completed` thì backend sẽ reject

### UX khuyến nghị

- ẩn hoặc disable nút delete nếu status không hợp lệ
- vẫn cần xử lý fallback nếu backend trả lỗi

---

## 6. TypeScript models FE có thể dùng

```ts
type CampaignTaskResponse = {
  campaignTaskId: string;
  campaignId: string;
  campaignTeamId: string;
  campaignTeamName: string;
  title: string;
  description?: string | null;
  startDate: string;
  dueDate?: string | null;
  status: number;
  priority: number;
  createdBy: string;
  createdAt: string;
};

type MemberTaskResponse = {
  memberTaskId: string;
  campaignTaskId: string;
  volunteerProfileId: string;
  volunteerName: string;
  subTaskTitle: string;
  taskNote?: string | null;
  assignedAt?: string | null;
  completedAt?: string | null;
  status: number;
};

type CampaignTaskDetailResponse = CampaignTaskResponse & {
  memberTaskCount: number;
  completedMemberTaskCount: number;
  memberTasks: MemberTaskResponse[];
};
```

---

## 7. Gợi ý màn hình FE

### 7.1 Task list screen

Hiển thị:

- title
- campaign team name
- status
- priority
- start date / due date

Actions:

- view detail
- edit
- change status
- delete

### 7.2 Task detail screen

Hiển thị:

- thông tin task
- progress member tasks
- danh sách member tasks
- nút assign thêm member task

### 7.3 Assign member task modal

Fields:

- volunteer
- sub task title
- note

---

## 8. Các lỗi nghiệp vụ FE có thể gặp

- `Campaign tasks are only supported for relief campaigns.`
- `Due date must be greater than or equal to start date.`
- `Completed or cancelled tasks cannot be updated.`
- `Only planned or cancelled tasks can be deleted.`
- `Invalid campaign task status transition: X -> Y.`
- `Task execution transitions are only allowed when the parent relief campaign is Active.`
- `Assigned volunteer must belong to the owning campaign team.`
- `Campaign team '{id}' was not found in campaign '{campaignId}'.`
- `Volunteer profile '{id}' was not found.`

---

## 9. Checklist cho FE

- build task list theo campaign
- build task create/edit form
- build task detail page
- build change status action theo transition hợp lệ
- build assign member task modal
- render progress từ `completedMemberTaskCount / memberTaskCount`
- disable các action invalid ngay từ UI nếu đã biết status
