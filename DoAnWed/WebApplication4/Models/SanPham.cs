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

    // ViewModel cho trang chủ kết hợp dữ liệu EF từ products và categories
    public class TrangChuViewModel
    {
        public List<products> SanPhamBanChay { get; set; }
        public List<products> SanPhamMoi { get; set; }
        public List<products> TatCaSanPham { get; set; }
        public List<categories> DanhMucNoiBat { get; set; }
    }
}
