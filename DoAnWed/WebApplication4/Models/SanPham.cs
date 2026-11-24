using System;
using System.Collections.Generic;

namespace WebApplication4.Models
{
    // Partial class mở rộng cho entity products từ Entity Framework
    public partial class products
    {
        // Tên danh mục (fallback nếu navigation property null)
        public string CategoryName => categories != null ? categories.name : "Trà sữa";

        // Định dạng tiền tệ
        public string FormattedPrice => price.ToString("N0") + " đ";
    }

    // ViewModel cho trang chủ kết hợp dữ liệu EF từ products và categories (Phần 2)
    public class TrangChuViewModel
    {
        public List<products> SanPhamBanChay { get; set; }
        public List<products> SanPhamMoi { get; set; }
        public List<products> TatCaSanPham { get; set; }
        public List<categories> DanhMucNoiBat { get; set; }
    }

    // ViewModel cho trang danh sách sản phẩm & bộ lọc (Phần 3)
    public class DanhSachSanPhamViewModel
    {
        public List<products> DanhSachSanPham { get; set; }
        public List<categories> DanhMuc { get; set; }
        public int? MaLoaiHienTai { get; set; }
        public string TenLoaiHienTai { get; set; }
        public string TuKhoa { get; set; }
        public string Sort { get; set; }
        public string MucGia { get; set; }
        public int TongSoSanPham { get; set; }
        public int TrangHienTai { get; set; }
        public int TongSoTrang { get; set; }
    }

    // Model cho Tùy chọn Size (Phần 4)
    public class TuyChonSize
    {
        public string MaSize { get; set; }
        public string TenSize { get; set; }
        public decimal GiaCongThem { get; set; }
        public bool MacDinh { get; set; }
    }

    // Model cho Tùy chọn Topping (Phần 4)
    public class TuyChonTopping
    {
        public int Id { get; set; }
        public string TenTopping { get; set; }
        public decimal Gia { get; set; }
        public string HinhAnh { get; set; }
    }

    // ViewModel cho trang Chi tiết sản phẩm & Tùy chọn món (Phần 4 - TV2)
    public class ChiTietSanPhamViewModel
    {
        public products SanPham { get; set; }
        public List<products> SanPhamLienQuan { get; set; }
        public List<TuyChonSize> DanhSachSize { get; set; }
        public List<TuyChonTopping> DanhSachTopping { get; set; }
        public List<string> DanhSachDuong { get; set; }
        public List<string> DanhSachDa { get; set; }
    }
}
