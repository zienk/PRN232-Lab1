# Tài liệu ôn tập vấn đáp bảo vệ Lab 1 – REST API (PRN232)
## Phiên bản giải thích siêu dễ hiểu theo phương pháp Feynman (Bản gốc ẩn dụ thực tế)

Chào bạn! Tài liệu này được thiết kế đặc biệt để giúp bạn vượt qua buổi vấn đáp một cách xuất sắc nhất. Mọi định nghĩa lập trình phức tạp đều được **đơn giản hóa bằng các ví dụ thực tế cực kỳ dễ nhớ** (phương pháp Feynman). Khi bạn dùng các phép ẩn dụ này để trả lời, giảng viên sẽ lập tức nhận ra bạn hiểu sâu bản chất vấn đề chứ không chỉ học vẹt.

---

## Phần 1: Kiến trúc 3 lớp & Mô hình Dữ liệu (3-Layer & Models)

### Q1: Tại sao em phải chia dự án thành 3 lớp (API, Services, Repositories)?
* **Giải thích kiểu Feynman (Ẩn dụ Nhà Hàng)**:
  * Bạn hãy tưởng tượng hệ thống giống như một nhà hàng:
    * **API Layer (Phục vụ/Bồi bàn)**: Là người trực tiếp giao tiếp với khách hàng. Họ nhận order (HTTP Request), kiểm tra xem menu có món đó không (Validation), chuyển order vào bếp, rồi mang món ăn ra bàn cho khách (HTTP Response). Người phục vụ *không nấu ăn*.
    * **Service Layer (Đầu bếp)**: Ở trong bếp và là người nấu ăn chính (Business Logic). Họ nhận nguyên liệu, chế biến thành món ăn hoàn chỉnh theo đúng công thức. Đầu bếp *không chạy ra ngoài mua rau* cũng *không trực tiếp bưng bê*.
    * **Repository Layer (Thủ kho/Người cung cấp nguyên liệu)**: Đứng ở kho chứa nguyên liệu (Database). Khi đầu bếp cần thịt bò, thủ kho sẽ lấy ra đúng loại thịt bò và giao cho đầu bếp qua EF Core (Data Access). Thủ kho *không biết nấu ăn*.
* **Áp dụng vào code**:
  * Sự phân tách này giúp code cực kỳ gọn gàng. Nếu công thức nấu ăn (logic nghiệp vụ) thay đổi, bạn chỉ cần sửa ở tầng **Services** mà không làm ảnh hưởng đến cách bồi bàn bưng bê (**API**) hay cách thủ kho sắp xếp kệ hàng (**Repositories**).

### Q2: Tại sao dự án của em cần tới 4 loại Model khác nhau?
* **Giải thích kiểu Feynman (Ẩn dụ Hộp Đồ Chơi)**:
  * Hãy tưởng tượng bạn có một món đồ chơi lắp ráp bằng gỗ trong kho (Database).
    * **Entity Model (Món đồ chơi nguyên bản trong kho)**: Rất nặng, có nhiều chi tiết góc cạnh và ốc vít liên kết phức tạp. Nếu bạn bưng nguyên món đồ này ship trực tiếp cho khách hàng, có thể làm gãy các chốt liên kết và cồng kềnh.
    * **Request Model (Hộp carton đóng gói gửi tới)**: Là vỏ hộp thiết kế để khách gửi yêu cầu mua. Nó có kích thước cố định, chống va đập, chỉ chứa những thông tin khách điền.
    * **Response Model (Mô hình nhựa trưng bày đẹp đẽ)**: Là phiên bản đồ chơi đã được mài nhẵn, sơn màu bắt mắt, tháo bỏ các ốc vít sắc nhọn nguy hiểm để khách đặt trên bàn học ngắm nghía an toàn.
    * **Business Model (Bản vẽ thiết kế kỹ thuật)**: Dùng riêng cho thợ mộc trong xưởng (tầng Service) để tính toán tỉ lệ và chỉnh sửa kích thước.
* **Áp dụng vào code**:
  * Việc dùng riêng Request/Response Model giúp chúng ta không bao giờ trả thẳng thực thể Database (**Entity**) ra ngoài. Nhờ đó, bảo mật được DB (tránh hack ghi đè dữ liệu cấm) và tránh được lỗi truyền dữ liệu vòng lặp tuần hoàn.

---

## Phần 2: Dependency Injection (DI) & Vòng đời của Service

### Q3: Dependency Injection (DI) là gì? Em hãy giải thích 3 vòng đời Transient, Scoped, và Singleton bằng ví dụ thực tế.
* **Giải thích kiểu Feynman (Ẩn dụ Dịch vụ Khách sạn)**:
  * Bình thường, nếu bạn cần một đồ vật (Dependency), bạn phải tự đi mua hoặc tự tạo mới bằng lệnh `new`. Với **DI**, giống như bạn ở trong một khách sạn 5 sao: khi bạn cần bàn chải, nước uống hay dọn phòng, bạn chỉ cần yêu cầu và hệ thống khách sạn sẽ tự mang đến tận phòng cho bạn.
  * Trong `Program.cs`, chúng ta đăng ký dịch vụ với 3 loại vòng đời (Lifetimes):
    1. **Transient (Bàn chải đánh răng dùng một lần)**: Mỗi lần bạn gọi yêu cầu, khách sạn sẽ mang đến một cái bàn chải mới tinh. Hai lần gọi là hai cái bàn chải hoàn toàn khác nhau.
    2. **Scoped (Nhân viên phục vụ riêng cho phòng)**: Khi bạn check-in, khách sạn cử một nhân viên hỗ trợ bạn. Suốt thời gian bạn ở đó và thực hiện các yêu cầu trong một lượt request, người nhân viên này sẽ xử lý từ đầu đến cuối. Nhưng khi khách hàng ở phòng khác (Request khác) gọi điện, khách sạn sẽ cử một nhân viên khác cho họ.
    3. **Singleton (Bác bảo vệ ở cổng chính)**: Cả khách sạn chỉ có duy nhất một bác bảo vệ. Bất kể bạn là ai, ở phòng nào, gọi lúc nào, suốt từ năm này qua năm khác, mọi người đều tương tác với đúng một bác bảo vệ duy nhất đó.
* **Áp dụng vào code**:
  * Dịch vụ của chúng ta (như `IStudentService`, `IUnitOfWork`) được đăng ký dạng **Scoped** (dòng 16-23 ở [Program.cs](file:///c:/Users/zien01/Desktop/SU2026/PRN232/Lab1/PRN232.LMS.API/Program.cs#L16-L23)). Điều này cực kỳ tối ưu vì mỗi lượt truy cập (HTTP Request) của người dùng sẽ sử dụng một instance dịch vụ riêng biệt và tự động giải phóng khi xử lý xong request đó.

---

## Phần 3: REST API & HTTP Protocols

### Q4: Query String (Query Parameter) là gì và khác gì Path Parameter?
* **Giải thích kiểu Feynman (Ẩn dụ Địa chỉ và Bộ lọc)**:
  * Hãy tưởng tượng bạn đi tìm căn hộ trong một chung cư lớn:
    * **Path Parameter (`/api/students/{id}`)**: Là số phòng cụ thể (ví dụ: Phòng 101). Nó xác định **chính xác duy nhất** một tài nguyên cụ thể. Không có số phòng, bạn không biết gõ cửa nhà ai.
    * **Query String (`/api/students?search=An`)**: Giống như bộ lọc tìm kiếm. Bạn đứng giữa sảnh chung cư và hét lớn: *"Ai tên An thì bước ra"* hoặc *"Ai ở tầng 2 thì giơ tay"*. Nó không thay đổi địa chỉ sảnh chung cư, chỉ giúp bạn thu hẹp và lọc danh sách người hiển thị.
* **Áp dụng vào code**:
  * Path parameter dùng để lấy chi tiết một học sinh cụ thể qua ID. Query string dùng ở API danh sách để thực hiện tìm kiếm (`search`), sắp xếp (`sort`) và phân trang (`page`, `size`) thông qua class [QueryParameters.cs](file:///c:/Users/zien01/Desktop/SU2026/PRN232/Lab1/PRN232.LMS.Repositories/Common/QueryParameters.cs).

### Q5: HTTP Header là gì? Em dùng nó như thế nào?
* **Giải thích kiểu Feynman (Ẩn dụ Tem nhãn dán ngoài thùng hàng)**:
  * Khi bạn gửi một thùng quà qua bưu điện:
    * **HTTP Body**: Là món quà thật sự nằm bên trong thùng (ví dụ: chuỗi JSON thông tin học sinh gửi lên).
    * **HTTP Header**: Là tờ hóa đơn dán bên ngoài thùng chứa thông tin vận chuyển, ví dụ: *"Hàng dễ vỡ"*, *"Gửi bằng đường hàng không"*, *"Người nhận nói tiếng Việt"*.
* **Áp dụng vào code**:
  * Header `Content-Type: application/json` báo cho server biết dữ liệu trong thùng hàng là định dạng JSON để server tự động phân tích (parse).
  * Khi tạo mới học sinh thành công, ta trả về mã `201 Created` kèm theo Header `Location` (chứa URL dẫn trực tiếp tới học sinh vừa tạo) để bưu tá (Client) biết đường đi lấy hàng.

---

## Phần 4: Cơ sở Dữ liệu & Tối ưu hiệu năng (Performance)

### Q6: Việc chèn (Insert) 500 bản ghi Enrollment lúc khởi chạy mất bao lâu? Tối ưu như thế nào?
* **Giải thích kiểu Feynman (Ẩn dụ Đi chợ mua rau)**:
  * Hãy tưởng tượng bạn cần mua 500 bó rau ngoài chợ:
    * **Cách tệ hại (Insert từng dòng một trong vòng lặp)**: Bạn đi ra chợ mua 1 bó rau, mang về nhà cất. Rồi bạn lại đi bộ ra chợ mua bó thứ 2 mang về... Lặp lại 500 lần. Bạn sẽ mệt đứt hơi vì tốn thời gian đi lại trên đường (gọi là Network Round-trip).
    * **Cách tối ưu (Batch Insert trong dự án)**: Bạn cầm một chiếc giỏ siêu to khổng lồ, ra chợ gom đủ 500 bó rau bỏ vào giỏ và xách về nhà trong **1 chuyến đi duy nhất**.
* **Áp dụng vào code**:
  * Trong file seed data [LmsDbContext.cs](file:///c:/Users/zien01/Desktop/SU2026/PRN232/Lab1/PRN232.LMS.Repositories/Data/LmsDbContext.cs#L147), chúng ta đưa 500 dòng đăng ký học tập vào một `List` rồi gọi `HasData()`. EF Core sẽ biên dịch thành một câu lệnh SQL chứa các cụm chèn theo lô (Batching), gửi xuống SQL Server xử lý trong nháy mắt (khoảng vài chục phần trăm giây) thay vì thực hiện 500 kết nối độc lập.

### Q7: Em hãy phân biệt IQueryable và IEnumerable?
* **Giải thích kiểu Feynman (Ẩn dụ Order đồ ăn trong nhà hàng)**:
  * Bạn bước vào nhà hàng và muốn ăn mì Ý không hành, nhiều phô mai:
    * **IQueryable (Yêu cầu đầu bếp chuẩn bị)**: Bạn gọi món với bồi bàn. Bồi bàn mang yêu cầu vào bếp, đầu bếp chọn đúng sợi mì Ý, lọc hành ra, bỏ thêm phô mai rồi mới nấu chín và bưng ra đĩa cho bạn. Mọi thao tác lọc và chế biến đều diễn ra trong bếp (**Database**). Khi bạn nhận đĩa thức ăn, nó đã sẵn sàng để ăn luôn.
    * **IEnumerable (Tự mang cả đống đồ về bàn rồi tự lọc)**: Đầu bếp nấu sẵn một nồi mì Ý khổng lồ có cả hành. Bạn bắt đầu múc cả nồi ra bàn ăn của mình, rồi tự dùng đũa gắp từng cọng hành vứt đi, sau đó tự rắc thêm phô mai vào. Điều này gây lãng phí năng lượng của bạn và bàn ăn của bạn sẽ bị ngập trong đống thức ăn thừa (**Memory/RAM của API**).
* **Áp dụng vào code**:
  * Tầng Repositories của chúng ta luôn trả về `IQueryable` để câu lệnh SQL khi sinh ra sẽ thực hiện lọc, sắp xếp, phân trang ngay trực tiếp ở SQL Server trước khi trả dữ liệu nhẹ nhàng về bộ nhớ RAM của ứng dụng.

---

## Phần 5: Tính năng nâng cao động (Dynamic API)

### Q8: Hãy giải thích cách em lập trình Sắp xếp động (Dynamic Sort) bằng ví dụ trực quan nhất.
* **Giải thích kiểu Feynman (Ẩn dụ Người quản lý thư viện)**:
  * Giả sử bạn có hàng ngàn cuốn sách trên kệ và người đọc yêu cầu: *"Hãy xếp sách theo tên tác giả tăng dần, nếu trùng tên thì xếp theo năm xuất bản giảm dần"*.
    * Nếu làm thủ công: Bạn phải viết hàng chục hàm sắp xếp cứng cho từng trường hợp.
    * Cách làm động của chúng ta ([GenericRepository.cs dòng 107](file:///c:/Users/zien01/Desktop/SU2026/PRN232/Lab1/PRN232.LMS.Repositories/Implementations/GenericRepository.cs#L107)): Bạn có một "cánh tay robot" cực kỳ thông minh:
      1. Robot đọc yêu cầu dạng chữ viết: `"fullName,-dateOfBirth"`.
      2. Robot dùng kính hiển vi (**Reflection**) soi vào gáy sách xem sách có thông tin "fullName" hay "dateOfBirth" không.
      3. Robot tự tạo ra một bộ quy tắc xếp sách tạm thời (**Expression Tree**).
      4. Robot chạy chương trình sắp xếp theo quy tắc vừa vẽ ra để sắp gọn tủ sách chỉ trong một nốt nhạc.
* **Áp dụng vào code**:
  * Nhờ C# Expression Tree, chúng ta biến một chuỗi chữ thường truyền từ URL thành các câu lệnh sắp xếp `.OrderBy().ThenByDescending()` của LINQ một cách tự động mà không cần viết các câu lệnh `if/else` thủ công cho từng thuộc tính.

### Q9: Lambda Expression và Expression Tree khác nhau như thế nào?
* **Giải thích kiểu Feynman (Ẩn dụ Tách cà phê và Công thức pha chế)**:
  * Bạn muốn uống một tách cà phê sữa đá:
    * **Lambda Expression (`Func<T, bool>`)**: Là một **tách cà phê sữa đá đã được pha sẵn** đặt trên bàn. Bạn chỉ việc bưng lên và uống ngay lập tức (là đoạn code đã được biên dịch thành mã máy, máy tính chỉ việc chạy).
    * **Expression Tree (`Expression<Func<T, bool>>`)**: Là **tờ giấy viết công thức pha chế** cà phê sữa đá. Bạn có thể cầm tờ giấy này đọc, chỉnh sửa công thức (ví dụ: bớt đường, thêm sữa), hoặc đem tờ giấy này dịch sang tiếng Anh gửi cho một người nước ngoài pha giúp (chuyển đổi mã C# sang câu lệnh SQL và gửi xuống SQL Server chạy).
* **Áp dụng vào code**:
  * Chúng ta chuyển các câu điều kiện tìm kiếm dạng `Expression<Func<T, bool>>` từ tầng Service xuống Repository (dòng 27 ở [StudentService.cs](file:///c:/Users/zien01/Desktop/SU2026/PRN232/Lab1/PRN232.LMS.Services/Implementations/StudentService.cs#L27)). EF Core sẽ đọc "công thức" này và dịch nó thành câu truy vấn SQL `WHERE FullName LIKE '%...'` chuẩn xác để cơ sở dữ liệu thực thi.

---

## Phần 6: Triển khai Hệ thống (Docker Deployment)

### Q10: Tại sao em phải dùng Docker? File docker-compose.yml kết nối API và DB như thế nào?
* **Giải thích kiểu Feynman (Ẩn dụ Căn hộ Container di động)**:
  * Bình thường, để chạy ứng dụng của bạn trên máy thầy, thầy phải cài đặt đúng phiên bản .NET 10, cài SQL Server đúng cấu hình, đặt mật khẩu y hệt... Nếu lệch một chút sẽ báo lỗi *"Trên máy em chạy được mà máy thầy lỗi"*.
  * **Docker** giống như một chiếc container vận chuyển hàng hóa di động. Chúng ta đóng gói toàn bộ hệ điều hành thu nhỏ, thư viện ứng dụng, cấu hình bảo mật vào trong container đó. Thầy chỉ cần bấm nút, chiếc container sẽ tự bung ra hoạt động trơn tru y hệt như trên máy bạn.
  * Trong [docker-compose.yml](file:///c:/Users/zien01/Desktop/SU2026/PRN232/Lab1/docker-compose.yml):
    * Chúng ta tạo ra 2 container: Một chiếc chứa Database SQL Server (`sqlserver`), một chiếc chứa Web API (`api`).
    * Chúng ta nối chúng vào một mạng ảo nội bộ. Cấu hình Connection String của API (dòng 29) thay vì trỏ đến `localhost` thì trỏ thẳng đến tên dịch vụ `sqlserver` (tức là tên của container database). Hai căn hộ này sẽ nói chuyện riêng tư với nhau qua mạng ảo nội bộ cực kỳ an toàn.
