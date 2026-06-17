# PRN232 LMS REST API

Tài liệu này là phần ghi chú học tập và hướng dẫn demo cho project **Learning Management System REST API** của môn PRN232. Project gộp nội dung **Lab 1 - REST API nền tảng** và **Lab 2 - Advanced REST API & Security** vào một hệ thống có kiến trúc rõ ràng, chạy được local, chạy được Docker, có Swagger để demo và có README đủ chi tiết để ôn tập lại sau này.

Điểm quan trọng nhất của project: không chỉ "làm cho chạy", mà phải hiểu vì sao ngoài thực tế người ta cần REST, versioning, JWT, refresh token, middleware, validation, Docker và layered architecture.

<a id="table-of-contents"></a>
## Mục Lục

- [1. Project Này Giải Quyết Gì?](#1-project-purpose)
- [2. Cách Nhớ Tổng Quan](#2-mental-model)
- [3. Tech Stack](#3-tech-stack)
- [4. Cấu Trúc Solution](#4-solution-structure)
- [5. Kiến Trúc 3 Lớp](#5-layered-architecture)
- [6. Database Và Migration](#6-database-migration)
- [7. API Versioning: v1 Là Lab 1, v2 Là Lab 2](#7-api-versioning)
- [8. Authentication: Login, Refresh, Revoke](#8-authentication)
- [9. Authorization: User Làm Gì, Admin Làm Gì?](#9-authorization)
- [10. REST API Features Của Lab 1](#10-rest-features)
- [11. Lab 2 Features](#11-lab2-features)
- [12. Swagger Documentation](#12-swagger)
- [13. Flow Demo Có Giải Thích](#13-demo-flow)
- [14. Chạy Local](#14-run-local)
- [15. Chạy Docker](#15-run-docker)
- [16. Code Quality Và Checklist Trước Khi Nộp](#16-quality-checklist)
- [17. Troubleshooting](#17-troubleshooting)
- [18. Tóm Tắt Kiến Thức Cần Nhớ](#18-summary)

<a id="1-project-purpose"></a>
## 1. Project Này Giải Quyết Gì?

Project mô phỏng một hệ thống quản lý học tập đơn giản. Trong hệ thống có:

| Thành phần | Ý nghĩa thực tế |
|---|---|
| Semester | Học kỳ |
| Subject | Môn học, ví dụ PRN232 |
| Course | Lớp học cụ thể của một môn trong một học kỳ |
| Student | Sinh viên |
| Enrollment | Việc sinh viên đăng ký vào một course |
| User | Tài khoản dùng để đăng nhập API |
| RefreshToken | Token dài hạn dùng để cấp lại access token |

Nói ngắn gọn, project trả lời câu hỏi: nếu cần xây một backend Web API có CRUD, query, phân quyền, Swagger, Docker và database migration thì nên tổ chức code như thế nào để dễ đọc, dễ demo và dễ mở rộng?

<a id="2-mental-model"></a>
## 2. Cách Nhớ Tổng Quan

Một API backend dễ bị học theo kiểu nhớ chay: controller là gì, service là gì, token là gì, middleware là gì. Cách dễ hiểu hơn là gắn nó với một câu chuyện quen thuộc.

Hãy tưởng tượng hệ thống như một trường học:

| Khái niệm trong code | Cách hiểu bằng đời thường |
|---|---|
| Controller | Quầy tiếp nhận hồ sơ. Ai đến cũng gặp quầy trước. |
| Service | Phòng nghiệp vụ. Nơi quyết định logic đúng sai. |
| Repository | Kho hồ sơ. Chỉ lo lấy và lưu dữ liệu. |
| DbContext | Người giữ sổ database, biết bảng nào liên quan bảng nào. |
| Request Model | Mẫu đơn người dùng nộp vào. |
| Response Model | Phiếu trả kết quả đã được chuẩn hóa. |
| Validation | Bảo vệ cổng vào, kiểm tra đơn có thiếu/sai không. |
| Middleware | Các trạm kiểm soát trên đường request đi vào API. |
| JWT Access Token | Vé vào cổng có hạn ngắn. |
| Refresh Token | Phiếu đổi vé mới, sống lâu hơn access token. |
| Role User/Admin | Thẻ sinh viên và thẻ quản trị viên. |
| X-Request-Id | Mã vận đơn hoặc mã biên nhận để trace một request. |

Cách nhớ `401` và `403`:

- `401 Unauthorized`: giống như chưa trình thẻ, thẻ hết hạn hoặc thẻ giả. API chưa xác định được bạn là ai.
- `403 Forbidden`: giống như bạn có thẻ thật, nhưng thẻ sinh viên không được vào phòng dành cho admin.

Cách nhớ access token và refresh token:

- Access token giống vé vào cổng trong ngày: dùng để đi qua các cổng bảo vệ, nhưng sống ngắn để nếu lộ thì thiệt hại thấp.
- Refresh token giống phiếu đổi vé: không dùng để vào lớp trực tiếp, chỉ dùng để xin vé access token mới.
- Revoke refresh token giống xé phiếu đổi vé: từ lúc đó không thể đổi vé mới nữa.

<a id="3-tech-stack"></a>
## 3. Tech Stack

| Nhóm | Công nghệ |
|---|---|
| Runtime | .NET 10 |
| Web framework | ASP.NET Core Web API |
| Database | SQL Server |
| ORM | Entity Framework Core |
| Authentication | JWT Bearer |
| Password hashing | BCrypt |
| Validation | Data Annotations, FluentValidation |
| API documentation | Swagger/OpenAPI, Swashbuckle |
| Versioning | Asp.Versioning |
| Containerization | Docker, Docker Compose |

<a id="4-solution-structure"></a>
## 4. Cấu Trúc Solution

```text
PRN232.LMS.API
  Controllers
  Extensions
  Middleware
  Swagger
  Program.cs
  appsettings.json

PRN232.LMS.Services
  BusinessModels
  Helpers
  Implementations
  Interfaces
  Models
    Common
    Requests
    Responses
  Validators

PRN232.LMS.Repositories
  Common
  Data
  Entities
  Implementations
  Interfaces
  Migrations
```

Vai trò từng project:

| Project | Vai trò |
|---|---|
| `PRN232.LMS.API` | HTTP layer: controller, routing, Swagger, middleware, auth pipeline |
| `PRN232.LMS.Services` | Business layer: xử lý nghiệp vụ, validation logic, mapping response, JWT flow |
| `PRN232.LMS.Repositories` | Data access layer: EF Core, DbContext, entity, repository, unit of work, migration |

Nguyên tắc review code:

- Controller không chứa business logic phức tạp.
- Service không biết chi tiết HTTP status code.
- Repository không biết request/response DTO.
- Entity không được trả trực tiếp ra client.
- Secret, connection string và JWT config phải lấy từ configuration/environment.

<a id="5-layered-architecture"></a>
## 5. Kiến Trúc 3 Lớp

Luồng request đi qua hệ thống:

```text
Client
  -> Controller
  -> Service
  -> UnitOfWork / Repository
  -> DbContext
  -> SQL Server
```

Ví dụ tạo sinh viên:

```text
POST /api/v2/students
  -> StudentsController.Create
  -> StudentService.CreateStudentAsync
  -> UnitOfWork.Students.CreateAsync
  -> LmsDbContext.SaveChangesAsync
```

Tại sao phải tách lớp?

Nếu nhét hết vào controller, controller sẽ giống một quầy tiếp nhận vừa nhận hồ sơ, vừa kiểm tra nghiệp vụ, vừa tự chạy xuống kho lục hồ sơ, vừa ghi sổ. Lúc hệ thống nhỏ thì vẫn chạy, nhưng khi sửa sẽ rất dễ vỡ. Tách lớp giúp mỗi phần chỉ làm đúng việc của nó.

| Nếu cần sửa | Nên sửa ở đâu |
|---|---|
| Đổi format response | API hoặc Service response model |
| Đổi rule nghiệp vụ | Service |
| Đổi cách query database | Repository |
| Đổi cấu trúc bảng | Entity, DbContext, Migration |
| Đổi security pipeline | Program.cs, AuthService, policy |

<a id="6-database-migration"></a>
## 6. Database Và Migration

Các bảng chính:

```text
Semester 1 - n Course
Subject  1 - n Course
Student  1 - n Enrollment
Course   1 - n Enrollment
User     1 - n RefreshToken
```

Migration hiện tại gồm:

| Migration | Ý nghĩa |
|---|---|
| `DbLan1` | Tạo schema và seed data Lab 1 |
| `AddAuthenticationTables` | Tạo bảng `User`, `RefreshToken`, seed tài khoản demo |

Tài khoản demo:

| Username | Password | Role | Dùng để demo |
|---|---|---|---|
| `admin` | `Admin@123` | `Admin` | Gọi được API đọc và API ghi/xóa trong v2 |
| `user1` | `User@123` | `User` | Gọi được API đọc trong v2, bị `403` ở API Admin-only |

Lưu ý quan trọng về migration:

- Không sinh dữ liệu random trong `HasData`.
- Không dùng `DateTime.Now`, `Guid.NewGuid()` hoặc BCrypt random salt trực tiếp trong `HasData`.
- Seed data nên là dữ liệu tĩnh để EF Core không báo pending model changes mỗi lần build/run.

Kiểm tra model có lệch migration không:

```powershell
dotnet ef migrations has-pending-model-changes `
  --project .\PRN232.LMS.Repositories\PRN232.LMS.Repositories.csproj `
  --startup-project .\PRN232.LMS.API\PRN232.LMS.API.csproj `
  --context LmsDbContext
```

Kết quả tốt:

```text
No changes have been made to the model since the last migration.
```

<a id="7-api-versioning"></a>
## 7. API Versioning: v1 Là Lab 1, v2 Là Lab 2

Project dùng URL segment versioning:

```http
/api/v1/students
/api/v2/students
```

Quy ước:

| Version | Ý nghĩa | Authentication |
|---|---|---|
| `v1` | Lab 1 - REST API nền tảng | Public, không bắt JWT |
| `v2` | Lab 2 - Advanced REST API & Security | Bắt JWT ở resource APIs |

Tại sao phải version API?

Ngoài thực tế, API đã có client dùng rồi thì không thể sửa phá vỡ tùy tiện. Versioning giống như sách giáo trình có bản 1 và bản 2. Bản 1 vẫn để người đang học hoặc app cũ dùng, bản 2 thêm security hoặc behavior mới.

Trong project:

- `v1` giữ đúng tinh thần Lab 1: tập trung REST, query, response format, Docker, Swagger.
- `v2` thêm JWT, role-based authorization, refresh token, revoke token.
- Auth chỉ nằm ở `v2`: `/api/v2/auth/...`.
- Swagger không còn hiển thị route xấu kiểu `/api/v{version}/...`.

<a id="8-authentication"></a>
## 8. Authentication: Login, Refresh, Revoke

Authentication trả lời câu hỏi: bạn là ai?

Auth endpoints:

```http
POST /api/v2/auth/login
POST /api/v2/auth/refresh-token
POST /api/v2/auth/revoke
```

### 8.1. Login

Request:

```json
{
  "username": "admin",
  "password": "Admin@123"
}
```

Response trả về:

```json
{
  "success": true,
  "message": "Đăng nhập thành công",
  "data": {
    "accessToken": "...",
    "refreshToken": "...",
    "expiresIn": 3600,
    "username": "admin",
    "role": "Admin"
  }
}
```

Password không lưu plain text. Database chỉ lưu BCrypt hash. Khi login, API dùng `BCrypt.Verify` để so sánh password người dùng nhập với hash trong database.

### 8.2. Dùng Token Trong Swagger

Trên Swagger v2:

1. Gọi `POST /api/v2/auth/login`.
2. Copy `data.accessToken`.
3. Bấm nút **Authorize**.
4. Chỉ dán token, không nhập thêm chữ `Bearer`.

Swagger đã được cấu hình `http bearer`, nên nó tự gửi:

```http
Authorization: Bearer <accessToken>
```

Nếu dùng Postman hoặc curl thủ công thì vẫn phải tự set header theo chuẩn HTTP:

```http
Authorization: Bearer <accessToken>
```

### 8.3. Refresh Token

Access token nên sống ngắn. Nếu access token sống quá lâu, token bị lộ thì người khác có thể dùng quá lâu. Refresh token giải quyết việc đó.

Flow:

```text
Access token hết hạn
  -> Client gửi refresh token
  -> API kiểm tra token tồn tại, chưa revoke, chưa hết hạn
  -> API revoke refresh token cũ
  -> API cấp access token mới và refresh token mới
```

Request:

```json
{
  "refreshToken": "<refresh-token>"
}
```

### 8.4. Revoke Token

Revoke dùng khi logout hoặc khi muốn vô hiệu hóa một phiên đăng nhập.

Flow:

```text
Client gửi refresh token
  -> API tìm token trong database
  -> API đánh dấu IsRevoked = true
  -> Token đó không thể dùng để refresh nữa
```

Endpoint revoke được thiết kế idempotent:

| Tình huống | Response |
|---|---|
| Token hợp lệ | `200 OK` |
| Token đã revoke trước đó | `200 OK` |
| Token không tồn tại | `401 Unauthorized` |

<a id="9-authorization"></a>
## 9. Authorization: User Làm Gì, Admin Làm Gì?

Authorization trả lời câu hỏi: bạn được phép làm gì?

Cách nhớ:

- Authentication giống bảo vệ hỏi "Bạn là ai?".
- Authorization giống bảo vệ hỏi tiếp "Bạn có được vào phòng này không?".

Permission matrix:

| Nhóm endpoint | v1 | v2 User | v2 Admin |
|---|---:|---:|---:|
| `POST /api/v2/auth/login` | Không có | Public | Public |
| `POST /api/v2/auth/refresh-token` | Không có | Public | Public |
| `POST /api/v2/auth/revoke` | Không có | Public | Public |
| `GET /api/v1/...` | Public | Không cần token | Không cần token |
| `POST/PUT/DELETE /api/v1/...` | Public | Không cần token | Không cần token |
| `GET /api/v2/students` | Không áp dụng | Được | Được |
| `GET /api/v2/semesters` | Không áp dụng | Được | Được |
| `GET /api/v2/subjects` | Không áp dụng | Được | Được |
| `GET /api/v2/courses` | Không áp dụng | Được | Được |
| `GET /api/v2/enrollments` | Không áp dụng | Được | Được |
| `GET /api/v2/courses/{id}/students` | Không áp dụng | Được | Được |
| `GET /api/v2/courses/{id}/enrollments` | Không áp dụng | Được | Được |
| `GET /api/v2/admin/permission-check` | Không áp dụng | `403 Forbidden` | Được |
| `POST/PUT/DELETE /api/v2/students` | Không áp dụng | `403 Forbidden` | Được |
| `POST/PUT/DELETE /api/v2/semesters` | Không áp dụng | `403 Forbidden` | Được |
| `POST/PUT/DELETE /api/v2/subjects` | Không áp dụng | `403 Forbidden` | Được |
| `POST/PUT/DELETE /api/v2/courses` | Không áp dụng | `403 Forbidden` | Được |
| `POST/PUT/DELETE /api/v2/enrollments` | Không áp dụng | `403 Forbidden` | Được |

Policy trong code:

| Policy | Ý nghĩa |
|---|---|
| `Lab2Authenticated` | v1 cho qua, v2 yêu cầu JWT hợp lệ |
| `Lab2Admin` | v1 cho qua, v2 yêu cầu JWT hợp lệ và role Admin |

Vì sao v1 vẫn cho qua dù controller có `[Authorize]`?

Do policy được viết có điều kiện theo route version. Nếu request là v1, policy không bắt JWT. Nếu request là v2, policy mới kiểm tra user/role. Cách này giúp một controller phục vụ được cả Lab 1 và Lab 2 mà không duplicate endpoint.

<a id="10-rest-features"></a>
## 10. REST API Features Của Lab 1

### 10.1. RESTful Resource

Endpoint dùng danh từ số nhiều:

```http
GET    /api/v1/students
GET    /api/v1/students/{id}
POST   /api/v1/students
PUT    /api/v1/students/{id}
DELETE /api/v1/students/{id}
```

Không dùng kiểu RPC:

```http
/api/getStudents
/api/createStudent
```

Cách nhớ: REST xem dữ liệu như tài nguyên. Client không gọi "hàm", client thao tác lên "resource".

### 10.2. Search, Sort, Paging

Ví dụ:

```http
GET /api/v1/students?search=nguyen&sort=fullName&page=1&size=10
```

| Feature | Query | Ví dụ |
|---|---|---|
| Search | `search` | `?search=nguyen` |
| Sort tăng dần | `sort` | `?sort=fullName` |
| Sort giảm dần | `-field` | `?sort=-dateOfBirth` |
| Paging | `page`, `size` | `?page=2&size=10` |

Vì sao cần paging?

Nếu database có 1 triệu sinh viên mà API trả hết một lần, server chậm, client nặng, network tốn. Paging giống xem danh sách sản phẩm trên Shopee: mỗi trang chỉ lấy một phần vừa đủ.

### 10.3. Field Selection

```http
GET /api/v1/students?fields=studentId,fullName,email
```

Ý nghĩa: client chỉ lấy field cần dùng. Cách này giảm payload và làm response gọn hơn.

### 10.4. Expansion

```http
GET /api/v1/enrollments?expand=student,course
```

Ý nghĩa: bình thường enrollment chỉ cần thông tin đăng ký. Khi client cần thêm student/course liên quan thì dùng `expand`.

Cách nhớ: giống xem đơn hàng. Mặc định chỉ thấy mã đơn, khi bấm mở rộng mới thấy sản phẩm, địa chỉ, thanh toán.

### 10.5. Response Format Nhất Quán

Response thường:

```json
{
  "success": true,
  "message": "Request processed successfully",
  "data": {},
  "errors": null
}
```

Paged response:

```json
{
  "success": true,
  "message": "Request processed successfully",
  "data": [],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalItems": 50,
    "totalPages": 5
  },
  "errors": null
}
```

<a id="11-lab2-features"></a>
## 11. Lab 2 Features

### 11.1. Content Negotiation

API hỗ trợ JSON và XML:

```http
Accept: application/json
Accept: application/xml
```

Nếu client yêu cầu format không hỗ trợ, API trả:

```http
406 Not Acceptable
```

Ý nghĩa thực tế: cùng một endpoint nhưng client khác nhau có thể cần format khác nhau. Browser/app thường dùng JSON, một số hệ thống cũ có thể cần XML.

### 11.2. Model Binding

Project demo nhiều kiểu binding:

| Binding | Ví dụ | Ý nghĩa |
|---|---|---|
| Route | `/students/{id}` | Lấy id từ URL |
| Query | `?page=1&size=10` | Lấy filter/paging từ query string |
| Body | JSON khi POST/PUT | Lấy object client gửi lên |
| Header | `X-Request-Id` | Lấy metadata từ HTTP header |

### 11.3. X-Request-Id Để Làm Gì?

Demo:

```http
X-Request-Id: demo-001
```

API sẽ echo lại header này trong response.

Ý nghĩa:

Khi production có nhiều request chạy cùng lúc, log server rất dễ rối. `X-Request-Id` giống mã vận đơn. Nếu client báo lỗi "request demo-001 bị fail", backend có thể tìm log theo đúng mã đó để điều tra. Trong lab, nó chứng minh bạn biết header binding và correlation id.

### 11.4. Validation

Project dùng:

- Data Annotations cho rule cơ bản như required, email, string length.
- FluentValidation cho rule mạnh hơn, ví dụ tuổi tối thiểu, format student code, phone number.

Cách nhớ: validation là cổng bảo vệ dữ liệu. Dữ liệu bẩn càng đi sâu vào hệ thống thì càng khó sửa.

### 11.5. Middleware

Middleware là các trạm xử lý request trước khi vào controller.

Project có:

| Middleware | Mục đích |
|---|---|
| `ExceptionHandlingMiddleware` | Bắt lỗi toàn cục, trả response lỗi nhất quán |
| `RequestLoggingMiddleware` | Log method, path, status code, thời gian xử lý |

Thứ tự pipeline quan trọng:

```text
Exception Handling
Request Logging
Swagger
Authentication
Authorization
MapControllers
```

Authentication phải chạy trước Authorization vì API phải biết bạn là ai trước khi kiểm tra bạn được làm gì.

<a id="12-swagger"></a>
## 12. Swagger Documentation

Swagger chạy ở root URL:

```http
http://localhost:5000
```

Khi chạy local bằng Visual Studio hoặc `dotnet run`, xem URL cụ thể trong console.

Swagger docs:

```http
/swagger/v1/swagger.json
/swagger/v2/swagger.json
```

Swagger hiện có:

- Version `v1`: Lab 1, public endpoints.
- Version `v2`: Lab 2, JWT và role.
- Ghi chú tiếng Việt trên từng API.
- Nút Authorize chỉ cần dán `accessToken`, không gõ thêm `Bearer`.
- Endpoint Admin-only có note hướng dẫn dùng `user1` để test `403 Forbidden`.

<a id="13-demo-flow"></a>
## 13. Flow Demo Có Giải Thích

Demo tốt không chỉ là bấm cho chạy. Mỗi bước nên trả lời được: "Bước này chứng minh kiến thức gì?"

| Bước | Thao tác | Thấy được gì? | Bài học |
|---|---|---|---|
| 1 | Chạy API bằng Docker hoặc local | API start và tự apply migration | Backend cần môi trường lặp lại được |
| 2 | Mở Swagger | Có v1 và v2 riêng | API versioning giúp không lẫn Lab 1/Lab 2 |
| 3 | Gọi `GET /api/v1/students` không token | Trả `200 OK` | v1 là Lab 1 public, không bắt JWT |
| 4 | Gọi `GET /api/v2/students` không token | Trả `401 Unauthorized` | v2 resource APIs yêu cầu authentication |
| 5 | Login `admin / Admin@123` | Nhận access token và refresh token | Authentication: API xác định bạn là ai |
| 6 | Authorize Swagger bằng access token | Không cần nhập `Bearer` | Swagger đã cấu hình HTTP bearer đúng chuẩn |
| 7 | Gọi `GET /api/v2/students` bằng admin | Trả `200 OK` | Token hợp lệ thì qua authentication |
| 8 | Gọi `POST /api/v2/students` bằng admin | Nếu body hợp lệ thì tạo được | Admin có quyền ghi dữ liệu |
| 9 | Login `user1 / User@123` | Nhận token role User | Cùng là token nhưng role khác nhau |
| 10 | Dùng user1 gọi `GET /api/v2/students` | Trả `200 OK` | User được đọc dữ liệu |
| 11 | Dùng user1 gọi `GET /api/v2/admin/permission-check` | Trả `403 Forbidden` | Authorization chặn role không đủ quyền, test nhanh vì không cần body |
| 12 | Gọi `POST /api/v2/auth/refresh-token` | Nhận token mới | Refresh token giúp cấp lại access token |
| 13 | Gọi `POST /api/v2/auth/revoke` | Revoke thành công | Logout/vô hiệu hóa phiên đăng nhập |
| 14 | Dùng lại refresh token cũ | Trả `401 Unauthorized` | Token đã revoke không còn dùng được |
| 15 | Gửi `Accept: application/xml` | Response XML | Content negotiation |
| 16 | Gửi `X-Request-Id: demo-001` | Response echo lại header | Header binding và correlation id |

Body mẫu để tạo student khi demo Admin-only:

```json
{
  "studentCode": "SE12345",
  "fullName": "Nguyen Van Demo",
  "email": "demo.student@example.com",
  "dateOfBirth": "2003-01-15",
  "phoneNumber": "0901234567"
}
```

Nếu muốn test `403` nhanh, login bằng `user1`, authorize token user, rồi gọi:

```http
GET /api/v2/admin/permission-check
```

Nếu muốn test Admin-only bằng thao tác ghi dữ liệu thật, dùng `user1` gọi `POST /api/v2/students` hoặc các endpoint `POST/PUT/DELETE` ở v2.

<a id="14-run-local"></a>
## 14. Chạy Local

Build solution:

```powershell
dotnet build .\PRN232.LMS.slnx
```

Apply migration thủ công nếu cần:

```powershell
dotnet ef database update `
  --project .\PRN232.LMS.Repositories\PRN232.LMS.Repositories.csproj `
  --startup-project .\PRN232.LMS.API\PRN232.LMS.API.csproj `
  --context LmsDbContext
```

Run API:

```powershell
dotnet run --project .\PRN232.LMS.API\PRN232.LMS.API.csproj
```

Local database config nằm trong:

```text
PRN232.LMS.API/appsettings.Development.json
```

Mặc định hiện tại:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(local);Database=Lab1_LmsDb;User Id=sa;Password=12345;TrustServerCertificate=True"
  }
}
```

Nếu máy bạn dùng SQL Server khác, chỉ cần sửa connection string.

<a id="15-run-docker"></a>
## 15. Chạy Docker

Docker Compose chạy 2 service:

| Service | Vai trò |
|---|---|
| `sqlserver` | SQL Server 2022 |
| `api` | ASP.NET Core API |

Chạy:

```powershell
docker compose up --build
```

API:

```http
http://localhost:5000
```

SQL Server:

```text
Server: localhost,1433
User: sa
Password: YourStrong@Passw0rd
Database: LmsDb
```

Reset database container:

```powershell
docker compose down -v
docker compose up --build
```

JWT config trong Docker được truyền bằng environment variables:

```yaml
Jwt__SecretKey: PRN232-LMS-API-Super-Secret-Key-For-JWT-2026
Jwt__Issuer: LmsApi
Jwt__Audience: LmsApiUsers
Jwt__AccessTokenExpirationMinutes: 60
Jwt__RefreshTokenExpirationDays: 7
```

Vì sao dùng environment variables?

Ngoài thực tế không nên hard-code secret vào code. Docker/Kubernetes/CI thường truyền config qua environment để cùng một image có thể chạy ở nhiều môi trường: dev, staging, production.

<a id="16-quality-checklist"></a>
## 16. Code Quality Và Checklist Trước Khi Nộp

Lệnh nên chạy trước khi push:

```powershell
dotnet format .\PRN232.LMS.slnx --verify-no-changes --verbosity minimal
dotnet build .\PRN232.LMS.slnx
dotnet ef migrations has-pending-model-changes `
  --project .\PRN232.LMS.Repositories\PRN232.LMS.Repositories.csproj `
  --startup-project .\PRN232.LMS.API\PRN232.LMS.API.csproj `
  --context LmsDbContext
```

Checklist Lab 1:

| Yêu cầu | Trạng thái |
|---|---|
| 3-layer architecture | Đã có |
| Entity, Business, Request, Response Model | Đã có |
| RESTful endpoints dùng plural nouns | Đã có |
| CRUD API | Đã có |
| Get by id trả 404 khi không tồn tại | Đã có |
| Search, sort, paging | Đã có |
| Field selection | Đã có |
| Expansion | Đã có |
| Pagination metadata | Đã có |
| Consistent response format | Đã có |
| Swagger/OpenAPI | Đã có |
| Dockerfile, docker-compose.yml | Đã có |

Checklist Lab 2:

| Yêu cầu | Trạng thái |
|---|---|
| Reuse Lab 1 | Đã có |
| Content negotiation JSON/XML | Đã có |
| Return 406 for unsupported format | Đã cấu hình |
| Route, query, body, header binding | Đã có |
| Header binding `X-Request-Id` | Đã có |
| Data Annotations | Đã có |
| FluentValidation | Đã có |
| Attribute routing và route constraints | Đã có |
| Nested resource endpoint | Đã có |
| Named routes | Đã có |
| API versioning | Đã có |
| Global exception middleware | Đã có |
| Request logging middleware | Đã có |
| JWT login | Đã có |
| Refresh token | Đã có |
| Revoke refresh token | Đã có |
| BCrypt password hashing | Đã có |
| Role-based authorization | Đã có |
| GET Admin-only endpoint để test 403 nhanh | Đã có |
| Swagger JWT Authorize button | Đã có |
| Swagger ghi chú tiếng Việt | Đã có |

Các điểm nên giải thích nếu thầy hỏi hoặc tool review code:

- Controller chỉ điều phối request/response, không chứa business logic.
- Service xử lý nghiệp vụ và mapping sang response model.
- Repository chỉ làm data access.
- Password không lưu plain text.
- JWT secret nên lấy từ config/environment.
- Refresh token được lưu DB để revoke được.
- Access token không lưu DB vì JWT tự chứa claims và được validate bằng signature.
- `401` khác `403`.
- `X-Request-Id` giúp trace log.
- Migration seed dùng dữ liệu tĩnh, không sinh random trong `HasData`.

<a id="17-troubleshooting"></a>
## 17. Troubleshooting

### Swagger bắt nhập cả chữ Bearer

Swagger đã được cấu hình để không cần nhập `Bearer`. Nếu vẫn thấy phải nhập `Bearer`, hãy rebuild và reload Swagger:

```powershell
dotnet build .\PRN232.LMS.slnx
```

Sau đó lấy token mới từ login và chỉ dán `accessToken`.

### Gọi v2 bị 401

Nguyên nhân thường gặp:

- Chưa bấm Authorize trong Swagger.
- Dán nhầm refresh token thay vì access token.
- Token hết hạn.
- JWT secret/issuer/audience không khớp môi trường đang chạy.

### Gọi v2 bị 403

Điều này thường là đúng khi bạn đang dùng `user1` gọi API Admin-only. Muốn thành công, login bằng:

```text
admin / Admin@123
```

### Docker không chạy

Kiểm tra Docker Desktop đã bật chưa. Nếu gặp lỗi pipe như:

```text
open //./pipe/dockerDesktopLinuxEngine: The system cannot find the file specified
```

thì Docker engine chưa chạy.

### EF báo pending model changes

Kiểm tra seed data trong `HasData`. Nếu dùng `DateTime.Now`, `Guid.NewGuid()` hoặc hash random thì EF sẽ tưởng model thay đổi liên tục.

<a id="18-summary"></a>
## 18. Tóm Tắt Kiến Thức Cần Nhớ

Lab 1 là nền móng:

- Thiết kế REST resource.
- CRUD.
- Query features.
- Response format.
- 3-layer architecture.
- Swagger.
- Docker cơ bản.

Lab 2 là bước đưa API gần production hơn:

- Authentication bằng JWT.
- Authorization theo role.
- Refresh token và revoke token.
- Validation mạnh hơn.
- Middleware.
- Versioning.
- Content negotiation.
- Header binding và correlation id.
- Config qua environment variables khi chạy Docker.

Nếu chỉ nhớ endpoint thì rất nhanh quên. Nếu nhớ câu chuyện thì dễ hiểu hơn:

```text
Controller là quầy tiếp nhận.
Service là phòng nghiệp vụ.
Repository là kho hồ sơ.
JWT access token là vé vào cổng.
Refresh token là phiếu đổi vé mới.
Revoke là xé phiếu đổi vé.
401 là chưa qua cổng xác thực.
403 là có thẻ nhưng không đủ quyền.
X-Request-Id là mã vận đơn để lần theo request trong log.
```

Đó là bộ khung quan trọng để tiếp tục học testing, logging nâng cao, clean architecture, CI/CD, cloud deployment và monitoring.
