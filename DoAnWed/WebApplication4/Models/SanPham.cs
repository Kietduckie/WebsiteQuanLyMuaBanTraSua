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
}
