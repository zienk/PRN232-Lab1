# PRN232 LMS REST API

Tài liệu này hệ thống hóa nội dung Lab 1 và Lab 2 của môn PRN232 thông qua project **Learning Management System REST API**. Mục tiêu không chỉ là chạy được bài lab, mà còn hiểu rõ vì sao từng phần được thiết kế như vậy để có thể demo, bảo vệ bài và ôn tập sau này.

## 1. Tổng Quan Project

Project xây dựng một RESTful API cho hệ thống quản lý học tập đơn giản gồm:

- Semester: học kỳ.
- Subject: môn học.
- Course: lớp học/phần học cụ thể trong một học kỳ.
- Student: sinh viên.
- Enrollment: đăng ký học của sinh viên vào course.
- User: tài khoản đăng nhập hệ thống.
- RefreshToken: refresh token dùng để cấp lại access token.

Các yêu cầu chính đã được tích hợp:

- Kiến trúc 3 lớp: API, Services, Repositories.
- Entity Model, Business Model, Request Model, Response Model.
- CRUD RESTful API.
- Search, sort, paging, field selection, expansion.
- Response format nhất quán.
- Swagger/OpenAPI.
- Docker Compose cho API và SQL Server.
- Content negotiation JSON/XML.
- Model binding: route, query, body, header.
- Data validation bằng Data Annotations và FluentValidation.
- API versioning.
- Custom middleware.
- JWT authentication, authorization, role-based authorization.
- Refresh token flow.
- BCrypt password hashing.

## 2. Tài Khoản Demo

Hai tài khoản mặc định được seed vào database bằng migration:

| Username | Password | Role | Mục đích demo |
|---|---|---|---|
| `admin` | `Admin@123` | `Admin` | Có quyền gọi các API admin-only như create/update/delete |
| `user1` | `User@123` | `User` | Có quyền đọc dữ liệu, nhưng bị chặn ở API yêu cầu Admin |

Endpoint đăng nhập:

```http
POST /api/v1/auth/login
POST /api/auth/login
```

Body mẫu:

```json
{
  "username": "admin",
  "password": "Admin@123"
}
```

Sau khi login, copy `accessToken`, bấm nút **Authorize** trên Swagger và nhập:

```text
Bearer <accessToken>
```

## 3. Cấu Trúc Solution

```text
PRN232.LMS.API
  Controllers
  Middleware
  Program.cs
  appsettings.json

PRN232.LMS.Services
  Interfaces
  Implementations
  Models
    Requests
    Responses
    Common
  BusinessModels
  Validators
  Helpers

PRN232.LMS.Repositories
  Data
  Entities
  Interfaces
  Implementations
  Common
  Migrations
```

Vai trò từng project:

| Project | Vai trò |
|---|---|
| `PRN232.LMS.API` | Nhận HTTP request, routing, auth middleware, controller, Swagger |
| `PRN232.LMS.Services` | Xử lý business logic, mapping, validation nghiệp vụ, JWT/auth flow |
| `PRN232.LMS.Repositories` | Entity Framework Core, DbContext, repository, unit of work, migrations |

Nguyên tắc quan trọng:

- Controller không chứa business logic.
- Repository không chứa business logic.
- Service là nơi xử lý nghiệp vụ.
- API không trả Entity trực tiếp ra client.
- Repository không phụ thuộc Request/Response Model.

## 4. Kiến Trúc 3 Lớp

Luồng xử lý cơ bản:

```text
Client
  -> Controller
  -> Service
  -> UnitOfWork / Repository
  -> DbContext
  -> SQL Server
```

Ví dụ với API tạo sinh viên:

```text
POST /api/v1/students
  -> StudentsController.Create
  -> StudentService.CreateStudentAsync
  -> UnitOfWork.Students.CreateAsync
  -> LmsDbContext.SaveChangesAsync
```

Lợi ích:

- Dễ test từng lớp riêng.
- Dễ thay đổi database access mà không ảnh hưởng controller.
- Dễ mở rộng business logic.
- Tránh controller bị phình to.
- Code dễ đọc hơn khi thầy hoặc tool review.

## 5. Bốn Loại Model Cần Nhớ

| Model Type | Nằm ở đâu | Mục đích |
|---|---|---|
| Entity Model | `Repositories/Entities` | Mapping trực tiếp với database table |
| Business Model | `Services/BusinessModels` | Biểu diễn dữ liệu phục vụ xử lý nghiệp vụ |
| Request Model | `Services/Models/Requests` | Dữ liệu client gửi lên API |
| Response Model | `Services/Models/Responses` | Dữ liệu API trả về client |

Ví dụ:

- `Student` là Entity.
- `StudentRequestModel` là input model khi create/update.
- `StudentResponseModel` là output model trả về client.

Không nên:

```csharp
return Ok(studentEntity);
```

Nên:

```csharp
return Ok(ResponseModel<StudentResponseModel>.SuccessResponse(response));
```

## 6. Database Design

Các bảng chính từ Lab 1:

```text
Semester
  SemesterId
  SemesterName
  StartDate
  EndDate

Subject
  SubjectId
  SubjectCode
  SubjectName
  Credit

Course
  CourseId
  CourseName
  SemesterId
  SubjectId

Student
  StudentId
  FullName
  Email
  DateOfBirth

Enrollment
  EnrollmentId
  StudentId
  CourseId
  EnrollDate
  Status
```

Các bảng bổ sung từ Lab 2:

```text
User
  UserId
  Username
  PasswordHash
  Role

RefreshToken
  Id
  UserId
  Token
  ExpiresAt
  CreatedAt
  IsRevoked
```

Quan hệ chính:

```text
Semester 1 - n Course
Subject  1 - n Course
Student  1 - n Enrollment
Course   1 - n Enrollment
User     1 - n RefreshToken
```

Seed data:

- 5 semesters.
- 10 subjects.
- 20 courses.
- 50 students.
- 500 enrollments.
- 2 users mặc định cho demo JWT.

## 7. Migration Sạch

Project hiện có migration nền từ Lab 1 và một migration auth rõ nghĩa:

```text
DbLan1
AddAuthenticationTables
```

`AddAuthenticationTables` làm các việc:

- Tạo bảng `User`.
- Tạo bảng `RefreshToken`.
- Tạo unique index cho `User.Username`.
- Tạo foreign key `RefreshToken.UserId -> User.UserId`.
- Seed `admin` và `user1` bằng BCrypt hash cố định.

Lưu ý quan trọng:

- Không dùng `DateTime.Now`, `Guid.NewGuid()` hoặc BCrypt salt random trong `HasData`.
- Seed data trong EF migration nên là dữ liệu tĩnh.
- Nếu seed thay đổi mỗi lần chạy, EF Core có thể báo pending model changes khi `Database.Migrate()`.

## 8. RESTful API Design

Project dùng plural nouns cho resource:

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

Các resource chính:

```http
/api/v1/students
/api/v1/semesters
/api/v1/subjects
/api/v1/courses
/api/v1/enrollments
/api/v1/auth
```

Nested resource:

```http
GET /api/v1/courses/{courseId}/students
GET /api/v1/courses/{id}/enrollments
```

## 9. Query Features

List API hỗ trợ các tính năng của Lab 1:

| Feature | Query | Ví dụ |
|---|---|---|
| Search | `search` | `/api/v1/students?search=nguyen` |
| Sort | `sort` | `/api/v1/students?sort=fullName` |
| Desc sort | `-field` | `/api/v1/students?sort=-dateOfBirth` |
| Paging | `page`, `size` | `/api/v1/students?page=2&size=10` |
| Field selection | `fields` | `/api/v1/students?fields=studentId,fullName,email` |
| Expansion | `expand` | `/api/v1/enrollments?expand=student,course` |

Ví dụ tổng hợp:

```http
GET /api/v1/enrollments?status=Active&sort=-enrollDate&page=1&size=20&fields=enrollmentId,status&expand=student,course
```

Pagination metadata mẫu:

```json
{
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalItems": 100,
    "totalPages": 10
  }
}
```

## 10. Response Format Nhất Quán

Response thường có format:

```json
{
  "success": true,
  "message": "Request processed successfully",
  "data": {},
  "errors": null
}
```

Paged response có thêm metadata:

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

HTTP status code cần nhớ:

| Status | Ý nghĩa |
|---|---|
| `200 OK` | Request thành công |
| `201 Created` | Tạo resource thành công |
| `400 Bad Request` | Input không hợp lệ |
| `401 Unauthorized` | Chưa đăng nhập hoặc token sai |
| `403 Forbidden` | Đăng nhập rồi nhưng không đủ quyền |
| `404 Not Found` | Không tìm thấy resource |
| `406 Not Acceptable` | Client yêu cầu format không hỗ trợ |
| `500 Internal Server Error` | Lỗi server |

## 11. Content Negotiation

API hỗ trợ:

```http
Accept: application/json
Accept: application/xml
```

Cấu hình nằm trong `Program.cs`:

```csharp
options.RespectBrowserAcceptHeader = true;
options.ReturnHttpNotAcceptable = true;
```

Và thêm XML formatter:

```csharp
.AddXmlSerializerFormatters();
```

Khi demo:

```http
GET /api/v1/students
Accept: application/xml
Authorization: Bearer <token>
```

Nếu client gửi format không hỗ trợ, API trả `406 Not Acceptable`.

## 12. Model Binding

Lab 2 yêu cầu thể hiện nhiều kiểu binding.

Route binding:

```csharp
[HttpGet("{id:int}")]
public async Task<IActionResult> GetById([FromRoute] int id)
```

Query binding:

```csharp
public async Task<IActionResult> Get(
    [FromQuery] string? search,
    [FromQuery] int page = 1,
    [FromQuery] int size = 10)
```

Body binding:

```csharp
public async Task<IActionResult> Create([FromBody] StudentRequestModel model)
```

Header binding:

```csharp
[FromHeader(Name = "X-Request-Id")] string? requestId = null
```

Project có helper echo lại `X-Request-Id` trong response header để header binding không chỉ nằm cho có.

## 13. Validation

Project dùng hai lớp validation:

- Data Annotations trên Request Model.
- FluentValidation cho `StudentRequestModel`.

Ví dụ Data Annotations:

```csharp
[Required]
[StringLength(100)]
[EmailAddress]
public string Email { get; set; } = string.Empty;
```

Custom validation rule:

```text
StudentCode format: 2 chữ cái hoa + 5 chữ số
Ví dụ: SE19886, CE18793
```

FluentValidation kiểm tra:

- FullName không rỗng, độ dài hợp lệ.
- Email đúng định dạng.
- DateOfBirth hợp lệ, sinh viên tối thiểu 16 tuổi.
- StudentCode đúng format FPTU.
- PhoneNumber đúng ký tự cho phép.

## 14. API Versioning

Project hỗ trợ:

```http
/api/v1/students
/api/v2/students
```

Cấu hình version reader:

- URL segment: `/api/v1/...`
- Query string: `?api-version=1.0`
- Header: `X-API-Version`

Ví dụ:

```http
GET /api/v1/students
GET /api/v2/students
GET /api/students?api-version=1.0
```

Trong controller:

```csharp
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/students")]
```

## 15. Middleware

Project có hai custom middleware.

### 15.1. ExceptionHandlingMiddleware

Mục tiêu:

- Bắt lỗi chưa xử lý toàn cục.
- Trả response lỗi nhất quán.
- Không expose stack trace hoặc chi tiết nội bộ cho client.
- Log lỗi chi tiết ở server.

Ví dụ response:

```json
{
  "success": false,
  "message": "Đã xảy ra lỗi nội bộ. Vui lòng thử lại sau.",
  "errors": null
}
```

### 15.2. RequestLoggingMiddleware

Log các thông tin:

- HTTP method.
- Request path.
- Trace identifier.
- Response status code.
- Execution time.

Thứ tự middleware quan trọng:

```text
Exception Handling
Request Logging
Swagger
Authentication
Authorization
MapControllers
```

Authentication phải đặt trước Authorization.

## 16. JWT Authentication

JWT flow:

```text
Client login bằng username/password
  -> API kiểm tra user và BCrypt password hash
  -> API tạo access token
  -> API tạo refresh token và lưu DB
  -> Client dùng access token gọi protected API
```

Access token chứa claims:

- UserId.
- Username.
- Role.
- JWT ID.

Config JWT:

```json
{
  "Jwt": {
    "SecretKey": "PRN232-LMS-API-Super-Secret-Key-For-JWT-2026",
    "Issuer": "LmsApi",
    "Audience": "LmsApiUsers",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

Trong Docker, JWT config được truyền bằng environment variables:

```yaml
Jwt__SecretKey: ...
Jwt__Issuer: LmsApi
Jwt__Audience: LmsApiUsers
Jwt__AccessTokenExpirationMinutes: 60
Jwt__RefreshTokenExpirationDays: 7
```

## 17. Authorization

Controller dữ liệu chính được bảo vệ bằng:

```csharp
[Authorize]
```

Các thao tác admin-only dùng:

```csharp
[Authorize(Roles = "Admin")]
```

Demo phân quyền:

1. Login bằng `admin / Admin@123`.
2. Gọi `POST /api/v1/students`: thành công nếu body hợp lệ.
3. Login bằng `user1 / User@123`.
4. Gọi cùng endpoint admin-only: nhận `403 Forbidden`.

## 18. Refresh Token Flow

Endpoint:

```http
POST /api/v1/auth/refresh-token
```

Body:

```json
{
  "refreshToken": "<refresh-token>"
}
```

Flow:

```text
Client gửi refresh token
  -> API tìm token trong DB
  -> Kiểm tra token tồn tại, chưa revoke, chưa hết hạn
  -> Revoke token cũ
  -> Tạo access token mới
  -> Tạo refresh token mới
```

Refresh token được lưu trong bảng `RefreshToken`.

## 19. Swagger/OpenAPI

Swagger chạy tại root URL của API:

```http
http://localhost:5000
```

Hoặc khi chạy local bằng profile khác, xem URL trong console/Visual Studio.

Swagger hỗ trợ:

- Xem endpoint.
- Test request/response.
- Xem status code.
- Login lấy JWT.
- Authorize bằng Bearer token.
- Test protected API.

Swagger docs:

```http
/swagger/v1/swagger.json
/swagger/v2/swagger.json
```

## 20. Docker

Project có:

```text
Dockerfile
docker-compose.yml
```

Docker Compose chạy:

- SQL Server 2022 container.
- LMS API container.

Chạy Docker:

```powershell
docker compose up --build
```

API sau khi chạy Docker:

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

Reset database container khi cần test lại từ đầu:

```powershell
docker compose down -v
docker compose up --build
```

Lưu ý:

- Phải bật Docker Desktop trước.
- API trong container kết nối SQL Server bằng hostname `sqlserver`.
- JWT secret trong Docker Compose được truyền bằng environment variables.

## 21. Chạy Local Không Docker

Build solution:

```powershell
dotnet build .\PRN232.LMS.slnx
```

Apply migration:

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

Kiểm tra EF model có pending migration không:

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

## 22. Demo Flow Cho Thầy

Nên demo theo thứ tự sau:

1. Chạy `docker compose up --build`.
2. Mở Swagger ở `http://localhost:5000`.
3. Gọi `POST /api/v1/auth/login` bằng `admin / Admin@123`.
4. Copy access token.
5. Bấm **Authorize** và nhập `Bearer <token>`.
6. Gọi `GET /api/v1/students?page=1&size=10`.
7. Gọi `GET /api/v1/students?search=nguyen&sort=fullName&fields=studentId,fullName,email`.
8. Gọi `GET /api/v1/enrollments?expand=student,course`.
9. Gọi nested endpoint `GET /api/v1/courses/1/students`.
10. Gửi header `Accept: application/xml` để demo content negotiation.
11. Gửi header `X-Request-Id: demo-001` để demo header binding.
12. Gọi `POST /api/v1/students` bằng admin để demo role Admin.
13. Login `user1 / User@123`, thử gọi endpoint admin-only để thấy `403 Forbidden`.
14. Gọi `POST /api/v1/auth/refresh-token` để demo refresh token.

## 23. Checklist Lab 1

| Yêu cầu | Trạng thái |
|---|---|
| 3-layer architecture | Đã có |
| Entity, Business, Request, Response Model | Đã có |
| RESTful endpoint dùng plural nouns | Đã có |
| CRUD API | Đã có |
| Get by id trả 404 khi không tồn tại | Đã có |
| Search | Đã có |
| Sort | Đã có |
| Paging | Đã có |
| Field selection | Đã có |
| Expansion | Đã có |
| Pagination metadata | Đã có |
| Consistent response format | Đã có |
| Swagger/OpenAPI | Đã có |
| Dockerfile | Đã có |
| docker-compose.yml | Đã có |

## 24. Checklist Lab 2

| Yêu cầu | Trạng thái |
|---|---|
| Reuse Lab 1 | Đã có |
| Content negotiation JSON/XML | Đã có |
| Return 406 for unsupported format | Đã cấu hình |
| Route binding | Đã có |
| Query binding | Đã có |
| Body binding | Đã có |
| Header binding `X-Request-Id` | Đã có |
| Data Annotations | Đã có |
| FluentValidation | Đã có |
| Custom validation rule | Đã có |
| Attribute routing | Đã có |
| Route constraints | Đã có |
| Nested resource endpoint | Đã có |
| Named routes | Đã có |
| API versioning | Đã có |
| Global exception middleware | Đã có |
| Request logging middleware | Đã có |
| Login API | Đã có |
| JWT access token | Đã có |
| Refresh token | Đã có |
| BCrypt password hashing | Đã có |
| `[Authorize]` protected API | Đã có |
| `[Authorize(Roles = "Admin")]` | Đã có |
| Swagger JWT Authorize button | Đã có |
| JWT secret qua Docker env | Đã có |

## 25. Ghi Nhớ Cho Code Review / SonarQube

Các điểm nên giải thích khi bị hỏi:

- Controller chỉ điều phối request/response, không chứa business logic.
- Service xử lý nghiệp vụ và mapping sang response model.
- Repository chỉ làm data access.
- Entity không được return trực tiếp ra client.
- Password không lưu plain text, chỉ lưu BCrypt hash.
- JWT secret nên cấu hình bằng environment variables khi deploy.
- Migration auth seed hash cố định, không sinh random trong `HasData`.
- `X-Request-Id` được echo lại trong response header.
- `CreatedAtRoute` có truyền `version` để Location header đúng với API versioning.
- Exception middleware không trả stack trace cho client.
- Docker Compose có healthcheck cho SQL Server trước khi API start.

## 26. Lệnh Nhanh

Build:

```powershell
dotnet build .\PRN232.LMS.slnx
```

Run local:

```powershell
dotnet run --project .\PRN232.LMS.API\PRN232.LMS.API.csproj
```

Run Docker:

```powershell
docker compose up --build
```

Stop Docker:

```powershell
docker compose down
```

Reset Docker database:

```powershell
docker compose down -v
docker compose up --build
```

Check pending migration:

```powershell
dotnet ef migrations has-pending-model-changes `
  --project .\PRN232.LMS.Repositories\PRN232.LMS.Repositories.csproj `
  --startup-project .\PRN232.LMS.API\PRN232.LMS.API.csproj `
  --context LmsDbContext
```

## 27. Tóm Tắt Kiến Thức Cần Nắm

Lab 1 tập trung vào nền tảng REST API:

- Thiết kế resource.
- CRUD.
- 3-layer architecture.
- EF Core.
- Query features.
- Response format.
- Swagger.
- Docker cơ bản.

Lab 2 nâng cấp API gần production hơn:

- Security bằng JWT.
- Authorization theo role.
- Refresh token.
- Validation mạnh hơn.
- Middleware.
- Versioning.
- Content negotiation.
- Header binding.
- Docker config bằng environment variables.

Nếu hiểu được project này, bạn đã nắm được skeleton quan trọng để xây dựng một ASP.NET Core Web API có cấu trúc rõ ràng, dễ mở rộng và đủ tốt để tiếp tục học authentication, testing, logging, monitoring và CI/CD.
