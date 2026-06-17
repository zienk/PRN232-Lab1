using System.Text;
using Asp.Versioning;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using PRN232.LMS.API.Middleware;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Implementations;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Implementations;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Validators;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. DATABASE - Kết nối SQL Server
// ============================================================
builder.Services.AddDbContext<LmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================================
// 2. DEPENDENCY INJECTION - Đăng ký các services và repositories
// ============================================================

// Repository Layer
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Service Layer
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// Lab 2: Auth Service
// Inject IConfiguration để AuthService có thể đọc JWT settings từ appsettings.json
builder.Services.AddScoped<IAuthService, AuthService>();

// ============================================================
// 3. JWT AUTHENTICATION - Cấu hình xác thực JWT
// ============================================================
// Đọc JWT settings từ appsettings.json (hoặc environment variables)
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey chưa được cấu hình trong appsettings.json");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "LmsApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "LmsApiUsers";

builder.Services.AddAuthentication(options =>
{
    // Đặt scheme mặc định là JWT Bearer
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Cấu hình cách validate JWT token
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Validate chữ ký (signature) của token
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),

        // Validate issuer (ai cấp token)
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,

        // Validate audience (token dùng cho ai)
        ValidateAudience = true,
        ValidAudience = jwtAudience,

        // Validate thời hạn của token
        ValidateLifetime = true,

        // Không cho phép lệch thời gian quá 0 giây (strict)
        ClockSkew = TimeSpan.Zero
    };

    // Xử lý khi xác thực thất bại (trả về 401 JSON thay vì redirect)
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                "{\"success\":false,\"message\":\"Chưa đăng nhập hoặc token không hợp lệ\",\"errors\":null}");
        },
        OnForbidden = async context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                "{\"success\":false,\"message\":\"Bạn không có quyền thực hiện thao tác này\",\"errors\":null}");
        }
    };
});

// Authorization: chỉ version 2 (Lab 2) mới yêu cầu JWT/role.
// Version 1 giữ đúng tinh thần Lab 1: REST API không bắt authentication.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Lab2Authenticated", policy =>
        policy.RequireAssertion(context =>
            !IsLab2Request(context.Resource) ||
            context.User.Identity?.IsAuthenticated == true));

    options.AddPolicy("Lab2Admin", policy =>
        policy.RequireAssertion(context =>
            !IsLab2Request(context.Resource) ||
            (context.User.Identity?.IsAuthenticated == true && context.User.IsInRole("Admin"))));
});

// ============================================================
// 4. API VERSIONING - Hỗ trợ /api/v1/... và /api/v2/...
// ============================================================
builder.Services.AddApiVersioning(options =>
{
    // Phiên bản mặc định là 1.0 nếu client không chỉ định
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;

    // Báo cáo các version được hỗ trợ trong response header
    options.ReportApiVersions = true;

    // Cách đọc version từ URL: /api/v1/students
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),        // Từ URL: /api/v1/
        new QueryStringApiVersionReader("api-version"), // Từ query: ?api-version=1.0
        new HeaderApiVersionReader("X-API-Version")     // Từ header
    );
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

// ============================================================
// 5. CONTROLLERS + CONTENT NEGOTIATION (JSON và XML)
// ============================================================
builder.Services.AddControllers(options =>
{
    // Trả về 406 Not Acceptable khi client yêu cầu format không hỗ trợ
    options.RespectBrowserAcceptHeader = true;
    options.ReturnHttpNotAcceptable = true; // 406 nếu không hỗ trợ format
})
.AddJsonOptions(options =>
{
    // Bỏ qua các field null khi serialize
    options.JsonSerializerOptions.DefaultIgnoreCondition =
        System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    // Tránh lỗi vòng lặp reference (circular reference)
    options.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
})
// Content Negotiation: thêm XML formatter
// Client gửi Accept: application/xml => trả về XML
// Client gửi Accept: application/json => trả về JSON
.AddXmlSerializerFormatters();

// ============================================================
// 6. FLUENTVALIDATION - Tự động validate request models
// ============================================================
// Đăng ký tất cả validators trong assembly của Services
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<StudentRequestValidator>();

// ============================================================
// 7. SWAGGER / OPENAPI - Thêm JWT support
// ============================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Swagger doc cho version 1
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LMS API - Lab 1 REST API",
        Version = "v1",
        Description = "PRN232 Lab 1 - RESTful API, 3-layer architecture, query features, Swagger, Docker"
    });

    // Swagger doc cho version 2
    c.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "LMS API - Lab 2 Advanced REST API & Security",
        Version = "v2",
        Description = "PRN232 Lab 2 - Content negotiation, validation, middleware, JWT, authorization, refresh token"
    });

    // Cấu hình JWT Authentication trong Swagger
    // Thêm nút "Authorize" để nhập Bearer token
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Nhập: Bearer {token}\n\nVí dụ: Bearer eyJhbGciOiJIUzI1NiIs...",
        Name = "Authorization",        // Tên header
        In = ParameterLocation.Header, // Vị trí: trong header
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Áp dụng JWT security requirement cho tất cả endpoints
    c.AddSecurityRequirement(openApiDocument =>
    {
        if (!string.Equals(openApiDocument.Info?.Version, "v2", StringComparison.OrdinalIgnoreCase))
        {
            return new OpenApiSecurityRequirement();
        }

        return new OpenApiSecurityRequirement
        {
        {
            new OpenApiSecuritySchemeReference("Bearer", openApiDocument, null),
            []
        }
        };
    });
});

// ============================================================
// BUILD APPLICATION
// ============================================================
var app = builder.Build();

// Tự động apply migrations và seed data khi khởi động
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LmsDbContext>();
    db.Database.Migrate();
}

// ============================================================
// 8. MIDDLEWARE PIPELINE - Thứ tự rất quan trọng!
// ============================================================

// Global Exception Handling phải là middleware đầu tiên
// để bắt tất cả exception từ các middleware phía sau
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Request Logging theo sau Exception Handling
app.UseMiddleware<RequestLoggingMiddleware>();

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // Hiển thị cả 2 version trong Swagger UI
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "LMS API v1");
    c.SwaggerEndpoint("/swagger/v2/swagger.json", "LMS API v2");
    c.RoutePrefix = string.Empty; // Swagger UI tại root URL
});

// Authentication phải trước Authorization
// UseAuthentication: đọc và validate JWT token từ header
app.UseAuthentication();
// UseAuthorization: kiểm tra quyền dựa trên claims trong token
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();

static bool IsLab2Request(object? resource)
{
    if (resource is not HttpContext httpContext)
    {
        return false;
    }

    var version = httpContext.Request.RouteValues["version"]?.ToString();
    return string.Equals(version, "2", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(version, "2.0", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(version, "v2", StringComparison.OrdinalIgnoreCase);
}
