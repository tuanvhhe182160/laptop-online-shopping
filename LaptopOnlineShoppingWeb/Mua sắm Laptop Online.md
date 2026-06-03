#### **Member 1: Core System, Auth & Danh mục (Người khởi tạo)**

*Người này sẽ chịu trách nhiệm setup bộ khung ban đầu cho cả nhóm làm việc, xử lý luồng đăng nhập và các dữ liệu cơ bản.*

* **Thiết lập Base:** \* Tạo Solution, 2 Projects.  
  * Cấu hình `DbContext`, viết base class cho **Repository Pattern** (`IGenericRepository`, `GenericRepository`).  
  * Thiết lập một class dùng **Singleton Pattern** (VD: Cấu hình phân trang mặc định).  
* **Module Users & Roles (Phân quyền):**  
  * Làm API đăng nhập, tạo Cookie/Session.  
  * Xây dựng UI màn hình Đăng nhập. Thiết lập chặn quyền Admin/Staff trên Client.  
  * Quản lý CRUD Users (Staff): 1 pop up để thêm mới staff (cấp username, password, role) \+ isActive.   
  * **Task bổ sung:**  
    * ​Viết API upload file cho Avatar (Xử lý IFormFile trong ASP.NET Core).  
    * ​Viết API đổi mật khẩu (Change Password).  
    * ​Viết API và luồng Forgot Password (Tạo token, gửi email bằng MailKit hoặc SmtpClient).  
    * ​Xây dựng giao diện trang Profile và màn hình Quên mật khẩu.  
* **Module Categories (Danh mục Hãng):**  
  * Làm Fullstack CRUD cho bảng `Categories` (Có dùng Pop-up thêm/sửa).

#### **Member 2: Quản lý Sản phẩm & OData (Nòng cốt kỹ thuật)**

*Người này sẽ tập trung giải quyết bảng phức tạp nhất là `Laptops`, xử lý upload hình ảnh (nếu có) và tích hợp công nghệ OData.*

* **Module Categories (Danh mục Hãng):**  
  * Làm Fullstack CRUD cho bảng `Categories` (Có dùng Pop-up thêm/sửa).  
* **Module Laptops (Sản phẩm):**  
  * Làm Fullstack CRUD cho bảng `Laptops`.  
  * Hiển thị sản phẩm ra dạng card cho khách xem  
  * Xử lý UI Pop-up Create/Update (Validate giá \> 0, số lượng tồn kho \> 0).  
  * Xử lý UI Pop-up Confirm Delete (Sử dụng thư viện như SweetAlert2).  
  * **Logic Xóa:** Viết code kiểm tra xem Laptop đã có trong `OrderDetails` chưa. Nếu có thì chỉ update `Status = 0`, ngược lại cho xóa cứng.  
* **Tìm kiếm & Lọc OData:**  
  * Cấu hình OData trên Web API cho endpoint Laptops.  
  * Xây dựng giao diện thanh tìm kiếm và bộ lọc nâng cao (Giá, Hãng, Trạng thái) trên Client.

#### **Member 3: Khách hàng & Giao dịch Tài chính (Nghiệp vụ)**

*Người này sẽ xử lý luồng nghiệp vụ tạo đơn hàng, trừ kho và tính toán tổng tiền.*

* **Module Customers (Khách hàng):**  
  * **Giao diện Frontend:** Phải tách biệt UI làm 2 phần. Cần xây dựng giao diện Storefront (Trang chủ, Chi tiết sản phẩm, Giỏ hàng, Đăng ký/Đăng nhập khách hàng) song song với giao diện Quản lý đơn hàng bên trong Back-office.  – Hoàng làm 1 trang storefront hiển thị cơ bản rồi  
  * Làm Fullstack CRUD cơ bản cho danh sách Khách hàng.

**Module Orders (Đơn hàng & Giao dịch Tài chính):** Module này được chia làm 2 phân hệ rõ rệt: Storefront (Giao diện khách mua) và Back-office (Giao diện Staff quản lý).

* **Phân hệ Storefront (Khách hàng tự thanh toán):**  
  * **Xây dựng UI Checkout:** Tạo trang Thanh toán cho khách hàng (hiển thị tóm tắt giỏ hàng, form điền địa chỉ giao hàng, chọn phương thức thanh toán như COD/Chuyển khoản).  
  * **Thiết kế API Checkout (Giao dịch khép kín):** Viết API nhận request thanh toán từ Khách hàng. Bắt buộc sử dụng **Database Transaction** để xử lý một chuỗi hành động liền mạch:  
    1. Đọc dữ liệu từ `CartItems` của khách hàng để tính `TotalAmount` (Tổng tiền).  
    2. `INSERT` dữ liệu tạo bản ghi mới vào bảng `Orders`.  
    3. `INSERT` danh sách sản phẩm vào bảng `OrderDetails` (Bắt buộc lưu lại `UnitPrice` \- giá gốc tại thời điểm chốt đơn).  
    4. **Trừ kho:** `UPDATE` bảng `Laptops`, lấy `StockQuantity = StockQuantity - Quantity`. Nếu không đủ hàng, Rollback toàn bộ tiến trình và báo lỗi.  
    5. **Dọn giỏ hàng:** `DELETE` toàn bộ sản phẩm của khách hàng đó trong bảng `CartItems`.  
    6. Commit Transaction và trả về mã đơn hàng thành công.  
* **Phân hệ Back-office (Admin/Staff quản lý):**  
  * **Quản lý danh sách Đơn hàng:** Làm Fullstack UI màn hình danh sách Đơn hàng. Tích hợp OData để Staff có thể lọc nhanh đơn hàng theo: Khoảng thời gian (Từ ngày \- Đến ngày), Trạng thái đơn hàng (Pending/Shipped...), Trạng thái thanh toán.  
  * **Cập nhật Trạng thái (Pop-up):** Xây dựng Pop-up cho phép Staff thay đổi `OrderStatus` (Pending \-\> Processing \-\> Shipped \-\> Cancelled) và đánh dấu `PaymentStatus` (Đã thanh toán/Chưa thanh toán).  
  * **Xem chi tiết Đơn (Pop-up hoặc trang riêng):** Truy xuất và hiển thị chi tiết các sản phẩm bên trong một mã đơn hàng cụ thể, tính toán lại tổng tiền dựa trên `UnitPrice` \* `Quantity` để đối chiếu.  
* **Module Giỏ hàng (Cart):** Xây dựng API và giao diện để Khách hàng (đã đăng nhập) thêm/sửa/xóa sản phẩm vào giỏ (`CartItems`). Xử lý chặn logic nếu khách thêm quá số lượng tồn kho. 

