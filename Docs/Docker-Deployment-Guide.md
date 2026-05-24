# 🐳 Hướng dẫn Deploy .NET API lên Docker

## Mục lục
1. [Docker là gì?](#1-docker-là-gì)
2. [Các khái niệm cốt lõi](#2-các-khái-niệm-cốt-lõi)
3. [Dockerfile – Giải thích chi tiết](#3-dockerfile--giải-thích-chi-tiết)
4. [Docker Compose – Giải thích chi tiết](#4-docker-compose--giải-thích-chi-tiết)
5. [Hướng dẫn chạy từng bước](#5-hướng-dẫn-chạy-từng-bước)
6. [Các lệnh Docker quan trọng](#6-các-lệnh-docker-quan-trọng)
7. [Troubleshooting](#7-troubleshooting)
8. [Kiến thức ứng dụng thực tế](#8-kiến-thức-ứng-dụng-thực-tế)

---

## 1. Docker là gì?

Docker là công cụ **đóng gói ứng dụng** cùng với mọi thứ nó cần (OS, runtime, dependencies) vào một **container** – giống như một "hộp" độc lập có thể chạy ở bất kỳ đâu.

### Tại sao cần Docker?

| Vấn đề không có Docker | Docker giải quyết |
|---|---|
| "Máy tôi chạy được mà?" | Container chạy giống nhau ở mọi nơi |
| Cài đặt SQL Server phức tạp | 1 lệnh duy nhất để chạy SQL Server |
| Xung đột phiên bản giữa các dự án | Mỗi container có environment riêng |
| Deploy lên server phức tạp | Build image → push → pull → run |

### So sánh: Docker Container vs Máy ảo (VM)

```
┌─────────────────────────┐    ┌─────────────────────────┐
│      Container          │    │      Virtual Machine     │
├─────────────────────────┤    ├─────────────────────────┤
│  App A  │  App B  │ App C│    │  App A  │  App B        │
│  Libs   │  Libs   │ Libs │    │  Libs   │  Libs         │
├─────────────────────────┤    ├─────────┼───────────────┤
│     Docker Engine       │    │ Guest OS│ Guest OS      │
├─────────────────────────┤    ├─────────────────────────┤
│     Host OS (chung)     │    │      Hypervisor         │
├─────────────────────────┤    ├─────────────────────────┤
│     Hardware            │    │      Host OS + Hardware  │
└─────────────────────────┘    └─────────────────────────┘
   Nhẹ, nhanh, vài MB            Nặng, chậm, vài GB
```

> **Container** dùng chung kernel OS → khởi động trong vài giây, nhẹ hơn VM rất nhiều.

---

## 2. Các khái niệm cốt lõi

### 2.1. Image vs Container

```
Image (bản thiết kế)          Container (thực thể đang chạy)
┌──────────────────┐          ┌──────────────────┐
│  .NET Runtime    │  ──────► │  .NET Runtime    │ (đang chạy)
│  App DLLs        │  docker  │  App DLLs        │
│  Config files    │   run    │  Config files    │
│  (Read-only)     │          │  (Read-write)    │
└──────────────────┘          └──────────────────┘
```

- **Image** = bản đóng gói tĩnh, read-only (giống file `.iso`)
- **Container** = instance đang chạy từ image (giống máy ảo boot từ `.iso`)
- Từ **1 image** có thể tạo **nhiều container**

### 2.2. Registry

Nơi lưu trữ và chia sẻ images:
- **Docker Hub** (`hub.docker.com`) – registry công cộng
- **Azure Container Registry**, **AWS ECR** – registry riêng

### 2.3. Volume

Dữ liệu trong container **bị mất khi container bị xóa**. Volume giúp lưu trữ dữ liệu bền vững:

```
Container (tạm thời)     Volume (bền vững)
┌──────────────┐         ┌──────────────┐
│  SQL Server  │ ◄─────► │  Database    │
│  (có thể xóa)│         │  files       │
└──────────────┘         │  (giữ lại)   │
                         └──────────────┘
```

### 2.4. Network

Docker tạo **mạng nội bộ** để các container giao tiếp với nhau:

```
Docker Network (bridge)
┌─────────────────────────────────────┐
│                                     │
│  ┌─────────┐      ┌─────────────┐  │
│  │   API   │─────►│  SQL Server │  │
│  │ :8080   │      │  :1433      │  │
│  └─────────┘      └─────────────┘  │
│                                     │
└─────────────────────────────────────┘
        │
        ▼ port mapping (5000:8080)
   Host machine :5000
```

---

## 3. Dockerfile – Giải thích chi tiết

Dockerfile là **script** hướng dẫn Docker cách build image.

### File của project ta:

```dockerfile
# ============================================
# STAGE 1: base – Image chạy ứng dụng (nhẹ)
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
#     ▲ Lấy image ASP.NET Runtime (chỉ chạy, không build)
#     Từ Microsoft Container Registry (mcr.microsoft.com)
WORKDIR /app
#     ▲ Đặt thư mục làm việc trong container = /app
EXPOSE 8080
#     ▲ Khai báo container lắng nghe port 8080 (chỉ document, không mở port)

# ============================================
# STAGE 2: build – Image có SDK để build code
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
#     ▲ Image SDK (nặng hơn, có compiler)
WORKDIR /src

# Copy file .csproj TRƯỚC → restore riêng (tận dụng cache)
COPY ["PRN232.LMS.API/PRN232.LMS.API.csproj", "PRN232.LMS.API/"]
COPY ["PRN232.LMS.Services/PRN232.LMS.Services.csproj", "PRN232.LMS.Services/"]
COPY ["PRN232.LMS.Repositories/PRN232.LMS.Repositories.csproj", "PRN232.LMS.Repositories/"]
RUN dotnet restore "PRN232.LMS.API/PRN232.LMS.API.csproj"
#     ▲ Tải NuGet packages. Layer này được CACHE nếu .csproj không đổi

COPY . .
#     ▲ Copy toàn bộ source code
WORKDIR "/src/PRN232.LMS.API"
RUN dotnet build "PRN232.LMS.API.csproj" -c Release -o /app/build
#     ▲ Build ở chế độ Release

# ============================================
# STAGE 3: publish – Tạo bản publish tối ưu
# ============================================
FROM build AS publish
RUN dotnet publish "PRN232.LMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false
#     ▲ Publish = build + tối ưu, bỏ file không cần thiết
#     UseAppHost=false: không tạo file .exe, dùng dotnet CLI để chạy

# ============================================
# STAGE 4: final – Image cuối cùng (nhẹ nhất)
# ============================================
FROM base AS final
#     ▲ Quay lại dùng image nhẹ (aspnet runtime, KHÔNG có SDK)
WORKDIR /app
COPY --from=publish /app/publish .
#     ▲ Copy kết quả publish từ stage trước
ENTRYPOINT ["dotnet", "PRN232.LMS.API.dll"]
#     ▲ Lệnh chạy khi container khởi động
```

### Tại sao dùng Multi-stage Build?

```
SDK Image:    ~800MB (có compiler, NuGet, msbuild...)
Runtime Image: ~200MB (chỉ có runtime)

Multi-stage = Build bằng SDK → Copy kết quả sang Runtime
→ Image cuối chỉ ~200MB thay vì ~800MB
```

### Docker Layer Cache

Mỗi lệnh `FROM`, `COPY`, `RUN` tạo một **layer**. Docker cache các layer không thay đổi:

```
Layer 1: FROM aspnet:10.0         ← cached (không đổi)
Layer 2: COPY *.csproj            ← cached nếu csproj không đổi
Layer 3: RUN dotnet restore       ← cached nếu layer trên cached
Layer 4: COPY . .                 ← THAY ĐỔI khi code thay đổi
Layer 5: RUN dotnet build         ← rebuild từ đây
```

> **Mẹo**: Copy `.csproj` và `restore` TRƯỚC khi copy source code → khi chỉ sửa code, bước restore được cache → build nhanh hơn rất nhiều.

---

## 4. Docker Compose – Giải thích chi tiết

Docker Compose quản lý **nhiều container** cùng lúc bằng 1 file YAML.

### File của project ta:

```yaml
version: '3.8'  # Phiên bản cú pháp Docker Compose

services:       # Danh sách các service (container)

  # ========== SERVICE 1: SQL Server ==========
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    #       ▲ Dùng image có sẵn từ Microsoft (không cần build)
    container_name: lms-sqlserver
    #       ▲ Đặt tên container (dễ nhận diện)
    environment:
      - ACCEPT_EULA=Y
      #   ▲ Chấp nhận license SQL Server (bắt buộc)
      - MSSQL_SA_PASSWORD=YourStrong@Passw0rd
      #   ▲ Mật khẩu cho tài khoản SA
    ports:
      - "1433:1433"
      #   ▲ Map port: HOST:CONTAINER
      #   Host machine port 1433 → Container port 1433
      #   → Có thể kết nối từ SSMS: localhost,1433
    volumes:
      - sqlserver-data:/var/opt/mssql
      #   ▲ Named volume: lưu data DB ra ngoài container
      #   Xóa container → data vẫn còn
    healthcheck:
      test: /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -C -Q "SELECT 1" || exit 1
      #   ▲ Kiểm tra SQL Server đã sẵn sàng chưa
      interval: 10s    # Kiểm tra mỗi 10 giây
      timeout: 5s      # Timeout sau 5 giây
      retries: 10      # Thử tối đa 10 lần
      start_period: 30s # Chờ 30 giây trước khi bắt đầu kiểm tra

  # ========== SERVICE 2: .NET API ==========
  api:
    build:
      context: .           # Build context = thư mục hiện tại
      dockerfile: Dockerfile
    container_name: lms-api
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8080
      #   ▲ API lắng nghe trên port 8080 trong container
      - ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=LmsDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True
      #   ▲ Connection string dùng TÊN SERVICE "sqlserver" thay vì localhost
      #   Vì Docker Compose tạo DNS nội bộ: tên service = hostname
    ports:
      - "5000:8080"
      #   ▲ Truy cập API từ máy host: http://localhost:5000
    depends_on:
      sqlserver:
        condition: service_healthy
        #   ▲ CHỈ khởi động API khi SQL Server healthy
        #   Tránh lỗi API start trước khi DB sẵn sàng

volumes:
  sqlserver-data:   # Khai báo named volume
```

### Luồng hoạt động khi chạy `docker-compose up`:

```
1. Docker đọc docker-compose.yml
2. Tạo network mặc định (lab1_default)
3. Tạo volume (sqlserver-data)
4. Khởi động sqlserver container
5. Chạy healthcheck mỗi 10s
6. Khi healthcheck pass → khởi động api container
7. API kết nối tới sqlserver:1433 (qua Docker DNS)
8. EF Core tự động Migrate + Seed data
9. API sẵn sàng tại http://localhost:5000
```

### Tại sao Connection String dùng `Server=sqlserver` không phải `localhost`?

```
┌─── Docker Network ───────────────────────┐
│                                          │
│  api container        sqlserver container│
│  ┌──────────┐         ┌──────────┐      │
│  │ localhost │         │ localhost │      │
│  │ = chính  │         │ = chính  │      │
│  │   nó     │         │   nó     │      │
│  └──────────┘         └──────────┘      │
│       │                    ▲             │
│       └── sqlserver:1433 ──┘             │
│           (Docker DNS)                   │
└──────────────────────────────────────────┘
```

> Trong Docker network, mỗi container có `localhost` riêng. Để container A gọi container B, dùng **tên service** làm hostname.

---

## 5. Hướng dẫn chạy từng bước

### Yêu cầu
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) đã cài và đang chạy

### Bước 1: Build và chạy

```powershell
# Di chuyển tới thư mục project
cd c:\Users\zien01\Desktop\SU2026\PRN232\Lab1

# Build và chạy tất cả services
docker-compose up --build
#   --build: force rebuild image (dùng khi code thay đổi)
```

### Bước 2: Kiểm tra

```powershell
# Xem containers đang chạy
docker ps

# Mở trình duyệt
# Swagger UI: http://localhost:5000
# Thử API:    http://localhost:5000/api/students
```

### Bước 3: Dừng

```powershell
# Dừng tất cả (giữ data)
docker-compose down

# Dừng và XÓA data (volume)
docker-compose down -v
```

---

## 6. Các lệnh Docker quan trọng

### Quản lý Container

```powershell
docker ps                    # Xem container đang chạy
docker ps -a                 # Xem tất cả (cả đã dừng)
docker logs lms-api          # Xem log của container
docker logs -f lms-api       # Xem log realtime (follow)
docker exec -it lms-api bash # Vào terminal container
docker stop lms-api          # Dừng container
docker rm lms-api            # Xóa container
```

### Quản lý Image

```powershell
docker images                # Liệt kê images
docker rmi <image_id>        # Xóa image
docker build -t myapp:v1 .   # Build image với tag
docker pull mcr.microsoft.com/mssql/server:2022-latest  # Tải image
```

### Quản lý Volume & Network

```powershell
docker volume ls             # Liệt kê volumes
docker volume rm <name>      # Xóa volume
docker network ls            # Liệt kê networks
```

### Docker Compose

```powershell
docker-compose up -d         # Chạy background (detached)
docker-compose up --build    # Rebuild rồi chạy
docker-compose down          # Dừng và xóa containers
docker-compose down -v       # Dừng + xóa volumes
docker-compose logs -f       # Xem log tất cả services
docker-compose ps            # Xem trạng thái services
docker-compose restart api   # Restart 1 service
```

---

## 7. Troubleshooting

### ❌ Lỗi "port already in use"

```powershell
# Tìm process đang dùng port 1433
netstat -ano | findstr :1433
# Kết thúc process hoặc đổi port trong docker-compose.yml:
# ports: "1434:1433"
```

### ❌ API không kết nối được DB

1. Kiểm tra SQL Server đã healthy: `docker ps` → cột STATUS
2. Kiểm tra logs: `docker logs lms-sqlserver`
3. Đảm bảo dùng **tên service** (`sqlserver`) trong connection string, KHÔNG dùng `localhost`

### ❌ Lỗi khi build image

```powershell
# Xóa cache và rebuild
docker-compose build --no-cache
```

### ❌ Container restart liên tục

```powershell
# Xem log để tìm nguyên nhân
docker logs lms-api --tail 50
```

---

## 8. Kiến thức ứng dụng thực tế

### 8.1. Quy trình CI/CD với Docker

```
Developer push code
        │
        ▼
   CI Server (GitHub Actions / Azure DevOps)
        │
   docker build → docker push (lên Registry)
        │
        ▼
   CD Server
        │
   docker pull → docker run (trên Production)
```

### 8.2. Ví dụ GitHub Actions deploy

```yaml
# .github/workflows/deploy.yml
name: Build and Deploy
on:
  push:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Build Docker image
        run: docker build -t myapp:${{ github.sha }} .
      - name: Push to Registry
        run: |
          docker tag myapp:${{ github.sha }} myregistry/myapp:latest
          docker push myregistry/myapp:latest
```

### 8.3. Environment Variables cho các môi trường

```yaml
# docker-compose.prod.yml (Production)
services:
  api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=prod-db;...
```

```powershell
# Chạy với file compose khác nhau
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up
```

### 8.4. Tóm tắt bản đồ tư duy

```
Docker
├── Image (bản thiết kế)
│   ├── Dockerfile → docker build
│   └── Registry (Docker Hub, ACR)
│
├── Container (instance chạy)
│   ├── docker run
│   ├── Isolated environment
│   └── Ephemeral (tạm thời)
│
├── Volume (lưu trữ bền vững)
│   └── Database data, uploads...
│
├── Network (giao tiếp container)
│   └── Service name = hostname
│
└── Docker Compose (quản lý nhiều container)
    ├── docker-compose.yml
    ├── depends_on + healthcheck
    └── 1 lệnh = chạy toàn bộ hệ thống
```

---

> **Ghi nhớ quan trọng**: Docker không chỉ dùng cho deployment. Nó giúp bạn tạo môi trường phát triển nhất quán, chạy database nhanh chóng, và là nền tảng của Kubernetes (K8s) – hệ thống orchestration container ở quy mô lớn.
