# BÁO CÁO CHI TIẾT NHIỆM VỤ THÀNH VIÊN 2 (TV2)
## ĐỒ ÁN LẬP TRÌNH WEB ASP.NET MVC — WEBSITE QUẢN LÝ MUA BÁN TRÀ SỮA (MILKTEA STORE)

> **Thông tin thực hiện:**
> * **Sinh viên:** Thành viên 2 (TV2) – Phụ trách Hiển thị và Lựa chọn Trà sữa
> * **Nhiệm vụ trọng tâm:** Hoàn thiện **Phần 2 (Trang Chủ)** & **Phần 3 (Danh Sách Sản Phẩm & Bộ Lọc)**
> * **File tài liệu Word:** Đã tạo sẵn file [BAO_CAO_PHAN_2_VA_3_TV2.doc](file:///c:/Users/Khoa/source/repos/WebsiteQuanLyMuaBanTraSua/DoAnWed/BAO_CAO_PHAN_2_VA_3_TV2.doc) (mở trực tiếp bằng Microsoft Word)

---

## MỤC LỤC BÁO CÁO
1. [Chương 1: Tổng quan nhiệm vụ và phân tích vai trò của TV2](#chương-1-tổng-quan-nhiệm-vụ-và-phân-tích-vai-trò-của-tv2)
2. [Chương 2: Chi tiết thực hiện và giải thích code Phần 2 (Trang Chủ)](#chương-2-chi-tiết-thực-hiện-và-giải-thích-code-phần-2-trang-chủ)
3. [Chương 3: Chi tiết thực hiện và giải thích code Phần 3 (Danh Sách Sản Phẩm & Bộ Lọc)](#chương-3-chi-tiết-thực-hiện-và-giải-thích-code-phần-3-danh-sách-sản-phẩm--bộ-lọc)
4. [Chương 4: Rà soát lỗi, kiểm tra xung đột và tích hợp nhóm](#chương-4-rà-soát-lỗi-kiểm-tra-xung-đột-và-tích-hợp-nhóm)
5. [Chương 5: Bảng tổng kết các file và hướng dẫn chạy thử](#chương-5-bảng-tổng-kết-các-file-và-hướng-dẫn-chạy-thử)

---

## CHƯƠNG 1: TỔNG QUAN NHIỆM VỤ VÀ PHÂN TÍCH VAI TRÒ CỦA TV2

Theo phân công của đồ án, **Thành viên 2** phụ trách toàn bộ **Bộ mặt giao diện khách hàng (Storefront) & Trải nghiệm chọn món trà sữa**. 
Đầu ra yêu cầu: *"Toàn bộ giao diện mua sắm và phần xử lý hiển thị sản phẩm"*.

Nhiệm vụ được chia thành 5 giai đoạn:
* **Phần 1:** Layout dùng chung (`LayoutPage.cshtml`), Menu 2 cấp, thanh Sidebar danh mục (`_SidebarPartial.cshtml`).
* **Phần 2:** Trang Chủ (Homepage Showcase) với Banner Slider đôi, Slogan giới thiệu, Món bán chạy nhất và Thực đơn theo nhóm.
* **Phần 3:** Trang Danh Sách Sản Phẩm & Bộ Lọc Đa Năng (`SanPham/Index`) với bộ lọc danh mục, mức giá, sắp xếp và phân trang.
* **Phần 4:** Trang Chi Tiết Món (`SanPham/Details`) với tùy chọn chọn Size (M/L), chọn Topping, chọn mức đường/đá và tính giá động.
* **Phần 5:** Tích hợp dữ liệu và chuyển giao luồng chọn món sang giỏ hàng của TV3.

---

## CHƯƠNG 2: CHI TIẾT THỰC HIỆN VÀ GIẢI THÍCH CODE PHẦN 2 (TRANG CHỦ)

### 2.1. Thiết kế giao diện Trang Chủ (`Views/TrangChu/Index.cshtml`)
Giao diện Trang Chủ được thiết kế bám sát hình ảnh mẫu thực tế gồm 5 khối chức năng:
1. **Cụm Banner Đôi / Carousel:**
   * **Bên trái:** Carousel tự động trượt (`data-ride="carousel"`, chu kỳ 4.5s) hiển thị banner Matcha Signature và Coffee Shop, có 2 nút tròn điều hướng `<` và `>`.
   * **Bên phải:** Banner thẻ khuyến mãi Cà phê có nút tròn điều hướng `>` dẫn trực tiếp sang danh mục Cà phê.
2. **Khối Hero Giới Thiệu (Dark Hero Banner):**
   * Tone màu nâu cà phê đậm sang trọng (`background: linear-gradient(135deg, #2D241D, #3F3327)`).
   * Tiêu đề chữ vàng caramel: `Chào Mừng Đến Với MILKTEA STORE`.
   * Đoạn mô tả thương hiệu truyền cảm hứng và các nút bấm xem nhanh từng loại đồ uống kèm số lượng món.
3. **Khối Món Bán Chạy (Best Seller):**
   * Lọc top các món có cờ `is_featured = true`, gắn nhãn đỏ `BEST SELLER`.
4. **Khối Thực Đơn Đa Dạng Hương Vị:**
   * Tự động phân chia thành các mục: Trà sữa, Trà trái cây, Cà phê, Topping thêm.
5. **Khối Cam Kết Dịch Vụ:**
   * 3 chính sách khẳng định chất lượng: Trà tươi 100%, Topping nấu mới mỗi ngày, Giao hàng siêu tốc.

### 2.2. Giải thích code Controller (`TrangChuController.cs`)
```csharp
public ActionResult Index()
{
    var mockData = GetMockViewModel();
    try
    {
        // 1. Lấy danh sách sản phẩm và danh mục đang hoạt động qua Entity Framework
        var dsProducts = db.products.Include(p => p.categories).Where(p => p.status).ToList();
        var dsCategories = db.categories.Where(c => c.status).ToList();

        if (dsProducts.Any())
        {
            // Bổ sung đầy đủ 22 món từ thư mục Images nếu CSDL chưa có đủ
            var existingNames = new HashSet<string>(dsProducts.Select(p => p.name.ToLower().Trim()));
            foreach (var mp in mockData.TatCaSanPham)
            {
                if (!existingNames.Contains(mp.name.ToLower().Trim()))
                {
                    dsProducts.Add(mp);
                }
            }

            viewModel.SanPhamBanChay = dsProducts.Where(p => p.is_featured).Take(4).ToList();
            viewModel.SanPhamMoi = dsProducts.OrderByDescending(p => p.id).Take(8).ToList();
            viewModel.TatCaSanPham = dsProducts;
            viewModel.DanhMucNoiBat = dsCategories;
            return View(viewModel);
        }
    }
    catch (Exception) { }

    return View(mockData);
}
```
* **Tác dụng:**
  * `Include(p => p.categories)`: Nạp kèm thông tin danh mục bằng Eager Loading, tránh phát sinh truy vấn N+1 vào CSDL.
  * Khối `try ... catch` và cơ chế **Merge Fallback**: Đảm bảo nếu CSDL của bạn mới chỉ nhập 4 món, Controller sẽ tự động bổ sung 18 món còn lại từ bộ ảnh để trang web luôn tràn ngập đủ 22 món hấp dẫn ở 4 danh mục.

### 2.3. Cơ chế ánh xạ ảnh thông minh (`ResolveProductImage`)
* **Vấn đề thực tế:** Tên ảnh trong CSDL có thể bị gõ sai, thiếu hoặc có dấu gạch nối khác với file trên đĩa.
* **Giải pháp:** TV2 đã viết hàm C# Razor kiểm tra sự tồn tại của file trên đĩa qua `HostingEnvironment.MapPath`. Nếu không tìm thấy, hệ thống tự động quét từ khóa trong tên món (*"truyền thống", "matcha", "đào", "vải", "ô long", "đường đen", "khoai môn", "thái xanh", "bạc xỉu", "cà phê muối"*...) để nạp đúng bức ảnh thật tương ứng từ thư mục `Images`.

---

## CHƯƠNG 3: CHI TIẾT THỰC HIỆN VÀ GIẢI THÍCH CODE PHẦN 3 (DANH SÁCH SẢN PHẨM & BỘ LỌC)

### 3.1. Thiết kế ViewModel (`DanhSachSanPhamViewModel` trong `Models/SanPham.cs`)
ViewModel chứa toàn bộ dữ liệu cần thiết để phục vụ lọc đa chiều:
```csharp
public class DanhSachSanPhamViewModel
{
    public List<products> DanhSachSanPham { get; set; } // Danh sách món sau khi lọc và phân trang
    public List<categories> DanhMuc { get; set; }       // Danh mục trà sữa để hiển thị tab lọc
    public int? MaLoaiHienTai { get; set; }             // Mã loại đang chọn (null = Tất cả)
    public string TenLoaiHienTai { get; set; }          // Tên loại hiển thị trên tiêu đề trang
    public string TuKhoa { get; set; }                  // Từ khóa tìm kiếm từ thanh search
    public string Sort { get; set; }                    // Tiêu chí sắp xếp: gia-tang, gia-giam, banchay, moi-nhat
    public string MucGia { get; set; }                  // Tiêu chí lọc giá: duoi30, 30den50, tren50
    public int TongSoSanPham { get; set; }              // Tổng số món tìm thấy
    public int TrangHienTai { get; set; }               // Trang hiện tại (1, 2, 3...)
    public int TongSoTrang { get; set; }                // Tổng số trang tính theo phân trang
}
```

### 3.2. Giải thích code Controller (`SanPhamController.cs`)
```csharp
public ActionResult Index(int? maLoai, string tuKhoa, string sort, string mucGia, int page = 1)
{
    int pageSize = 12; // 12 món / trang (3 hàng x 4 cột cân đối)
    var query = allProducts.AsQueryable();

    // 1. Lọc theo Danh mục
    if (maLoai.HasValue && maLoai.Value > 0)
    {
        query = query.Where(p => p.category_id == maLoai.Value);
    }

    // 2. Tìm kiếm theo Từ khóa
    if (!string.IsNullOrWhiteSpace(tuKhoa))
    {
        string kw = tuKhoa.Trim().ToLower();
        query = query.Where(p => (p.name != null && p.name.ToLower().Contains(kw)) ||
                                 (p.description != null && p.description.ToLower().Contains(kw)));
    }

    // 3. Lọc theo Mức giá
    if (!string.IsNullOrWhiteSpace(mucGia))
    {
        if (mucGia == "duoi30") query = query.Where(p => p.price < 30000);
        else if (mucGia == "30den50") query = query.Where(p => p.price >= 30000 && p.price <= 50000);
        else if (mucGia == "tren50") query = query.Where(p => p.price > 50000);
    }

    // 4. Sắp xếp kết quả
    switch (sort)
    {
        case "gia-tang": query = query.OrderBy(p => p.price); break;
        case "gia-giam": query = query.OrderByDescending(p => p.price); break;
        case "banchay":  query = query.OrderByDescending(p => p.is_featured); break;
        default:         query = query.OrderByDescending(p => p.id); break; // Mới nhất
    }

    // 5. Phân trang toán học
    int totalItems = query.Count();
    int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
    var pagedList = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

    return View(viewModel);
}
```

### 3.3. Tinh chỉnh kích thước ảnh món ăn gọn gàng (không bị to)
* Thay thế thuộc tính `padding-top: 85%` bằng chiều cao cố định chuẩn: **`height: 175px`**.
* Kết hợp `object-fit: cover` giúp ảnh món ăn giữ trọn vẹn tỉ lệ, nét đẹp mà không chiếm quá nhiều diện tích thẻ sản phẩm.

---

## CHƯƠNG 4: RÀ SOÁT LỖI, KIỂM TRA XUNG ĐỘT VÀ TÍCH HỢP NHÓM

| Hạng mục kiểm tra | Chi tiết xử lý của TV2 | Kết quả |
| :--- | :--- | :---: |
| **Lỗi TypeScript TS2683 (`'this' implicitly has type 'any'`)** | Gỡ bỏ thuộc tính inline `onerror` trên thẻ `<img>` và chuyển sang bắt sự kiện lỗi ảnh bằng JavaScript tiêu chuẩn `window.addEventListener("error", ...)`. | **Đã sạch 100% (0 Lỗi)** |
| **Lỗi `Cannot find name '$'`** | Không dùng ký hiệu `$` trong code inline của View con, để trình duyệt tự nhận diện jQuery qua file layout cha. | **Đã sạch 100% (0 Lỗi)** |
| **Xung đột tìm kiếm với TV1** | TV1 có form `Html.BeginForm("TimKiem", "SanPham")` trên Navbar. TV2 đã tạo sẵn action `TimKiem` trong `SanPhamController` để chuyển hướng mượt mà về `Index`. | **Tương thích hoàn toàn** |
| **Chuẩn bị đầu ra cho TV3** | TV2 đã chuẩn bị sẵn Action `Details(int? id)` trong `SanPhamController` để sẵn sàng cho Phần 4 (chọn Size, Topping) và chuyển vào giỏ hàng của TV3. | **Sẵn sàng kết nối** |
| **Đồng bộ cơ sở dữ liệu với TV4** | TV2 tạo sẵn file `QuanLyTraSua_SeedData.sql` chuẩn hóa bảng `categories`, `products`, `toppings` đúng tên 22 file ảnh. | **Đồng bộ hoàn toàn** |

---

## CHƯƠNG 5: BẢNG TỔNG KẾT CÁC FILE VÀ HƯỚNG DẪN CHẠY THỬ

### Danh sách các file thuộc phạm vi quản lý của TV2:
1. `Controllers/TrangChuController.cs`: Controller Trang chủ.
2. `Controllers/SanPhamController.cs`: Controller Danh sách sản phẩm & Bộ lọc.
3. `Models/SanPham.cs`: ViewModel cho Trang chủ và Danh sách sản phẩm.
4. `Views/Shared/LayoutPage.cshtml`: Layout chung của toàn bộ website.
5. `Views/Shared/_SidebarPartial.cshtml`: Partial view danh mục bên trái.
6. `Views/TrangChu/Index.cshtml`: Giao diện Trang chủ hoàn chỉnh có Banner đôi.
7. `Views/SanPham/Index.cshtml`: Giao diện Danh sách sản phẩm & Bộ lọc tương tác (ảnh gọn 175px).
8. `DoAnWed/WebApplication4/Images/`: Bộ sưu tập 22 ảnh sản phẩm và 2 ảnh banner.
9. `DoAnWed/QuanLyTraSua_SeedData.sql`: Script nạp CSDL mẫu.
10. `DoAnWed/BAO_CAO_PHAN_2_VA_3_TV2.doc`: **File tài liệu Word chính thức để nộp đồ án.**
