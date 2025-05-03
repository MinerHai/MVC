# Phân tích thiết kế hướng đối tượng (Object-Oriented Design Analysis)

Dựa trên cấu trúc dự án và các định nghĩa lớp được tìm thấy, đây là phân tích ban đầu về các đối tượng chính và mối quan hệ của chúng.

## Các Lớp (Classes)

Dưới đây là danh sách các lớp chính được xác định trong dự án:

*   **AppUser**: Đại diện cho người dùng hệ thống, kế thừa từ `IdentityUser`. Chứa thông tin người dùng.
*   **ContactModel**: Đại diện cho thông tin liên hệ.
*   **Category**: Đại diện cho danh mục bài viết hoặc sản phẩm. Có thuộc tính `Slug` duy nhất.
*   **Post**: Đại diện cho bài viết. Có thuộc tính `Slug` duy nhất.
*   **PostCategory**: Bảng trung gian thể hiện mối quan hệ nhiều-nhiều giữa `Post` và `Category`.
*   **CategoryProduct**: Đại diện cho danh mục sản phẩm. Có thuộc tính `Slug` duy nhất.
*   **ProductModel**: Đại diện cho sản phẩm. Có thuộc tính `Slug` duy nhất.
*   **ProductCategoryProduct**: Bảng trung gian thể hiện mối quan hệ nhiều-nhiều giữa `ProductModel` và `CategoryProduct`.
*   **ProductPhoto**: Đại diện cho ảnh của sản phẩm.
*   **PostComment**: Đại diện cho bình luận trên bài viết.
*   **OrderModel**: Đại diện cho một đơn đặt hàng.
*   **OrderItem**: Đại diện cho một mặt hàng trong đơn đặt hàng.
*   **PaymentInformationModel**: Model chứa thông tin cần thiết cho thanh toán (ví dụ: VNPay).
*   **CartItem**: Đại diện cho một mặt hàng trong giỏ hàng (thường được lưu trữ trong session hoặc database).

## Mối Quan Hệ Giữa Các Lớp (Relationships Between Classes)

Dựa trên cấu hình trong `AppDbContext`, các mối quan hệ chính bao gồm:

*   **AppUser** <---> **OrderModel**: Một người dùng (`AppUser`) có thể có nhiều đơn đặt hàng (`OrderModel`). Mối quan hệ một-nhiều.
*   **OrderModel** <---> **OrderItem**: Một đơn đặt hàng (`OrderModel`) bao gồm nhiều mặt hàng (`OrderItem`). Mối quan hệ một-nhiều. Khi xóa Order, các OrderItem liên quan sẽ bị xóa (Cascade Delete).
*   **Post** <---> **PostComment**: Một bài viết (`Post`) có thể có nhiều bình luận (`PostComment`). Mối quan hệ một-nhiều. Khi xóa Post, các PostComment liên quan sẽ bị xóa (Cascade Delete).
*   **PostComment** <---> **AppUser**: Một bình luận (`PostComment`) được viết bởi một tác giả (`AppUser`). Mối quan hệ nhiều-một. Khi xóa tác giả, AuthorId trong bình luận sẽ được đặt thành NULL (Set Null).
*   **Post** <---> **Category** (qua **PostCategory**): Một bài viết có thể thuộc nhiều danh mục, và một danh mục có thể chứa nhiều bài viết. Mối quan hệ nhiều-nhiều.
*   **ProductModel** <---> **CategoryProduct** (qua **ProductCategoryProduct**): Một sản phẩm có thể thuộc nhiều danh mục sản phẩm, và một danh mục sản phẩm có thể chứa nhiều sản phẩm. Mối quan hệ nhiều-nhiều.

## Phân tích chi tiết hơn

Để phân tích thiết kế hướng đối tượng đầy đủ, chúng ta cần xem xét chi tiết hơn các thuộc tính và phương thức của từng lớp, cũng như cách chúng tương tác trong các Controller và Services.

Ví dụ:
- Lớp `OrderController` sử dụng `OrderModel` và `OrderItem` để xử lý logic liên quan đến đơn hàng.
- Lớp `CartServices` sử dụng `CartItem` để quản lý giỏ hàng.
- Lớp `PaymentController` sử dụng `PaymentInformationModel` và tương tác với `VnPayService` để xử lý thanh toán.

Việc phân tích sâu hơn sẽ bao gồm:
- Xác định các thuộc tính cụ thể của từng lớp (ví dụ: `OrderModel` có `OrderDate`, `TotalAmount`, `Status`, v.v.).
- Xác định các phương thức chính của từng lớp hoặc các Service tương tác với lớp đó (ví dụ: `CartServices.AddCartItem`, `OrderController.Checkout`, `VnPayService.CreatePaymentUrl`).
- Vẽ biểu đồ lớp (Class Diagram) để minh họa rõ ràng cấu trúc và mối quan hệ.
- Mô tả luồng hoạt động (Sequence Diagram) cho các chức năng quan trọng (ví dụ: quy trình đặt hàng, quy trình thanh toán).

Tài liệu này cung cấp cái nhìn tổng quan ban đầu. Để có phân tích thiết kế hướng đối tượng hoàn chỉnh, cần đi sâu vào mã nguồn của từng lớp, Controller và Service liên quan.

## Các Chức Năng và Use Case Tổng Quát (General Functionalities and Use Cases)

Dựa trên cấu trúc dự án và các Controller đã phân tích, các chức năng và use case tổng quát của hệ thống bao gồm:

### Chức năng Quản lý Sản phẩm (Product Management)
*   **Use Case:** Quản lý danh mục sản phẩm (CRUD - Create, Read, Update, Delete).
*   **Use Case:** Quản lý thông tin sản phẩm (CRUD).
*   **Use Case:** Xem danh sách sản phẩm theo danh mục.
*   **Use Case:** Xem chi tiết sản phẩm.
*   **Use Case:** Tìm kiếm sản phẩm.

### Chức năng Giỏ hàng và Đặt hàng (Cart and Ordering)
*   **Use Case:** Thêm sản phẩm vào giỏ hàng.
*   **Use Case:** Cập nhật số lượng sản phẩm trong giỏ hàng.
*   **Use Case:** Xóa sản phẩm khỏi giỏ hàng.
*   **Use Case:** Xem giỏ hàng.
*   **Use Case:** Tiến hành thanh toán (Checkout).
*   **Use Case:** Quản lý đơn đặt hàng (dành cho Admin).
*   **Use Case:** Xem lịch sử đơn hàng (dành cho người dùng).

### Chức năng Thanh toán (Payment)
*   **Use Case:** Tạo yêu cầu thanh toán (ví dụ: tích hợp VNPay).
*   **Use Case:** Xử lý kết quả thanh toán.

### Chức năng Quản lý Nội dung (Content Management)
*   **Use Case:** Quản lý danh mục bài viết (CRUD).
*   **Use Case:** Quản lý bài viết (CRUD).
*   **Use Case:** Quản lý bình luận bài viết.
*   **Use Case:** Xem danh sách bài viết theo danh mục.
*   **Use Case:** Xem chi tiết bài viết.

### Chức năng Người dùng và Liên hệ (User and Contact)
*   **Use Case:** Đăng ký tài khoản.
*   **Use Case:** Đăng nhập/Đăng xuất.
*   **Use Case:** Quản lý thông tin người dùng.
*   **Use Case:** Gửi thông tin liên hệ.
*   **Use Case:** Quản lý thông tin liên hệ (dành cho Admin).

### Chức năng Hệ thống (System Functionalities)
*   **Use Case:** Quản lý người dùng (dành cho Admin).
*   **Use Case:** Cấu hình hệ thống (ví dụ: thông tin thanh toán trong appsettings.json).

Phần này cung cấp cái nhìn tổng quan về các chức năng chính của hệ thống, làm cơ sở để xây dựng các use case chi tiết hơn và phân tích thiết kế hướng đối tượng sâu sắc hơn.

## Mô tả hoạt động Use Case (Use Case Flow Description)

Dưới đây là mô tả luồng hoạt động tổng quát cho một số use case chính:

### Use Case: Thêm sản phẩm vào giỏ hàng

1.  **Actor:** Người dùng (User)
2.  **Mục tiêu:** Thêm một sản phẩm vào giỏ hàng của mình.
3.  **Luồng hoạt động:**
    *   Người dùng duyệt qua danh sách sản phẩm hoặc xem chi tiết một sản phẩm.
    *   Người dùng chọn sản phẩm và số lượng muốn thêm.
    *   Hệ thống kiểm tra tính hợp lệ của yêu cầu (ví dụ: số lượng tồn kho).
    *   Hệ thống thêm sản phẩm vào giỏ hàng của người dùng (thường được lưu trữ trong Session hoặc cơ sở dữ liệu tạm).
    *   Hệ thống cập nhật hiển thị giỏ hàng (ví dụ: số lượng mặt hàng trong biểu tượng giỏ hàng).
    *   Hệ thống thông báo cho người dùng biết sản phẩm đã được thêm vào giỏ hàng thành công.

### Use Case: Tiến hành thanh toán (Checkout)

1.  **Actor:** Người dùng (User)
2.  **Mục tiêu:** Hoàn tất quá trình mua hàng và tạo đơn đặt hàng.
3.  **Luồng hoạt động:**
    *   Người dùng xem lại các mặt hàng trong giỏ hàng.
    *   Người dùng xác nhận đơn hàng và cung cấp/kiểm tra thông tin giao hàng (địa chỉ, số điện thoại, v.v.).
    *   Người dùng chọn phương thức thanh toán (ví dụ: VNPay, thanh toán khi nhận hàng).
    *   Hệ thống tạo một bản ghi đơn hàng mới (`OrderModel`) và các mặt hàng chi tiết (`OrderItem`) trong cơ sở dữ liệu.
    *   Nếu phương thức thanh toán yêu cầu xử lý trực tuyến (ví dụ: VNPay), hệ thống sẽ gọi dịch vụ thanh toán (`VnPayService`) để tạo yêu cầu thanh toán và chuyển hướng người dùng đến cổng thanh toán.
    *   Sau khi xử lý thanh toán (thành công hoặc thất bại), cổng thanh toán gửi kết quả về cho hệ thống.
    *   Hệ thống cập nhật trạng thái của đơn hàng (`OrderModel.Status`) dựa trên kết quả thanh toán.
    *   Hệ thống hiển thị trang xác nhận đơn hàng hoặc thông báo kết quả thanh toán cho người dùng.
    *   Hệ thống có thể gửi email xác nhận đơn hàng cho người dùng.

### Use Case: Xem chi tiết sản phẩm

1.  **Actor:** Người dùng (User)
2.  **Mục tiêu:** Xem thông tin chi tiết về một sản phẩm cụ thể.
3.  **Luồng hoạt động:**
    *   Người dùng duyệt danh sách sản phẩm hoặc tìm kiếm sản phẩm.
    *   Người dùng nhấp vào tên hoặc hình ảnh của một sản phẩm.
    *   Hệ thống xác định sản phẩm được chọn (dựa trên ID hoặc Slug).
    *   Hệ thống truy vấn thông tin chi tiết của sản phẩm (`ProductModel`) và các ảnh liên quan (`ProductPhoto`) từ cơ sở dữ liệu.
    *   Hệ thống hiển thị trang chi tiết sản phẩm, bao gồm tên, mô tả, giá, ảnh, và các thông tin khác.

Mô tả này cung cấp cái nhìn cơ bản về cách các chức năng chính hoạt động. Để có phân tích đầy đủ cho thiết kế hướng đối tượng, cần đi sâu vào từng bước, xác định các đối tượng tham gia và sự tương tác giữa chúng.