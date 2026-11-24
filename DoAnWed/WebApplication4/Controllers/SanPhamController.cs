using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class SanPhamController : Controller
    {
        private QuanLyMuaBanTraSuaEntities db = new QuanLyMuaBanTraSuaEntities();

        // GET: SanPham (Danh sách & Bộ lọc sản phẩm - Nhiệm vụ TV2 Phần 3)
        public ActionResult Index(int? maLoai, string tuKhoa, string sort, string mucGia, int page = 1)
        {
            int pageSize = 12; // Hiển thị 12 món / trang (3 hàng x 4 cột)
            if (page < 1) page = 1;

            List<products> allProducts;
            List<categories> allCategories;

            var mockProds = GetMockProducts();
            var mockCats = GetMockCategories();

            try
            {
                // Thử đọc từ CSDL Entity Framework
                var dbProducts = db.products.Include(p => p.categories).Where(p => p.status).ToList();
                var dbCategories = db.categories.Where(c => c.status).ToList();

                if (dbProducts.Any())
                {
                    allProducts = dbProducts;
                    // Bổ sung đầy đủ tất cả các món từ thư mục Images nếu CSDL chưa có đủ
                    var existingNames = new HashSet<string>(allProducts.Select(p => p.name.ToLower().Trim()));
                    foreach (var mp in mockProds)
                    {
                        if (!existingNames.Contains(mp.name.ToLower().Trim()))
                        {
                            allProducts.Add(mp);
                        }
                    }
                }
                else
                {
                    allProducts = mockProds;
                }

                if (dbCategories.Any())
                {
                    allCategories = dbCategories;
                    var existingCatIds = new HashSet<int>(allCategories.Select(c => c.id));
                    foreach (var mc in mockCats)
                    {
                        if (!existingCatIds.Contains(mc.id))
                        {
                            allCategories.Add(mc);
                        }
                    }
                }
                else
                {
                    allCategories = mockCats;
                }
            }
            catch
            {
                // Dự phòng trường hợp CSDL chưa kết nối
                allProducts = mockProds;
                allCategories = mockCats;
            }

            var query = allProducts.AsQueryable();

            // 1. Lọc theo Danh mục (maLoai)
            string tenLoaiHienTai = "Tất Cả Sản Phẩm";
            if (maLoai.HasValue && maLoai.Value > 0)
            {
                query = query.Where(p => p.category_id == maLoai.Value);
                var cat = allCategories.FirstOrDefault(c => c.id == maLoai.Value);
                if (cat != null)
                {
                    tenLoaiHienTai = cat.name;
                }
            }

            // 2. Tìm kiếm theo Từ khóa (tuKhoa)
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string kw = tuKhoa.Trim().ToLower();
                query = query.Where(p => (p.name != null && p.name.ToLower().Contains(kw)) ||
                                         (p.description != null && p.description.ToLower().Contains(kw)));
            }

            // 3. Lọc theo Mức giá (mucGia)
            if (!string.IsNullOrWhiteSpace(mucGia))
            {
                switch (mucGia.ToLower())
                {
                    case "duoi30":
                        query = query.Where(p => p.price < 30000);
                        break;
                    case "30den50":
                        query = query.Where(p => p.price >= 30000 && p.price <= 50000);
                        break;
                    case "tren50":
                        query = query.Where(p => p.price > 50000);
                        break;
                }
            }

            // 4. Sắp xếp (sort)
            switch (sort)
            {
                case "gia-tang":
                    query = query.OrderBy(p => p.price);
                    break;
                case "gia-giam":
                    query = query.OrderByDescending(p => p.price);
                    break;
                case "banchay":
                    query = query.OrderByDescending(p => p.is_featured).ThenBy(p => p.name);
                    break;
                case "moi-nhat":
                default:
                    query = query.OrderByDescending(p => p.id);
                    break;
            }

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var pagedList = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var viewModel = new DanhSachSanPhamViewModel
            {
                DanhSachSanPham = pagedList,
                DanhMuc = allCategories,
                MaLoaiHienTai = maLoai,
                TenLoaiHienTai = tenLoaiHienTai,
                TuKhoa = tuKhoa,
                Sort = sort ?? "moi-nhat",
                MucGia = mucGia,
                TongSoSanPham = totalItems,
                TrangHienTai = page,
                TongSoTrang = totalPages
            };

            return View(viewModel);
        }

        // Action nhận tìm kiếm từ Navbar (TV1 / Shared)
        public ActionResult TimKiem(string tuKhoa)
        {
            return RedirectToAction("Index", new { tuKhoa = tuKhoa });
        }

        // GET: SanPham/Details/5 (Chi tiết sản phẩm & Tùy chọn món - Nhiệm vụ TV2 Phần 4)
        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Index");
            }

            products product = null;
            try
            {
                product = db.products.Include(p => p.categories).FirstOrDefault(p => p.id == id.Value && p.status);
            }
            catch { }

            if (product == null)
            {
                product = GetMockProducts().FirstOrDefault(p => p.id == id.Value) ?? GetMockProducts().First();
            }

            // 1. Sản phẩm liên quan (cùng danh mục)
            List<products> lienQuan = new List<products>();
            try
            {
                lienQuan = db.products.Include(p => p.categories)
                    .Where(p => p.category_id == product.category_id && p.id != product.id && p.status)
                    .Take(4).ToList();
            }
            catch { }

            if (!lienQuan.Any())
            {
                lienQuan = GetMockProducts()
                    .Where(p => p.category_id == product.category_id && p.id != product.id)
                    .Take(4).ToList();
            }

            if (lienQuan.Count < 4)
            {
                var others = GetMockProducts().Where(p => p.id != product.id && !lienQuan.Any(l => l.id == p.id)).Take(4 - lienQuan.Count).ToList();
                lienQuan.AddRange(others);
            }

            // 2. Danh sách Size (nếu là thức uống; nếu là topping thì không có size)
            var dsSize = new List<TuyChonSize>();
            if (product.category_id != 4)
            {
                dsSize.Add(new TuyChonSize { MaSize = "M", TenSize = "Size Vừa (M)", GiaCongThem = 0, MacDinh = true });
                dsSize.Add(new TuyChonSize { MaSize = "L", TenSize = "Size Lớn (L)", GiaCongThem = 6000, MacDinh = false });
            }

            // 3. Danh sách Topping (Lấy từ danh mục Topping thêm)
            var dsTopping = new List<TuyChonTopping>();
            if (product.category_id != 4)
            {
                try
                {
                    var dbToppings = db.products.Where(p => p.category_id == 4 && p.status).ToList();
                    if (dbToppings.Any())
                    {
                        dsTopping = dbToppings.Select(t => new TuyChonTopping
                        {
                            Id = t.id,
                            TenTopping = t.name,
                            Gia = t.price,
                            HinhAnh = t.image
                        }).ToList();
                    }
                }
                catch { }

                if (!dsTopping.Any())
                {
                    dsTopping = GetMockProducts().Where(p => p.category_id == 4).Select(t => new TuyChonTopping
                    {
                        Id = t.id,
                        TenTopping = t.name,
                        Gia = t.price,
                        HinhAnh = t.image
                    }).ToList();
                }
            }

            // 4. Danh sách Đường và Đá
            var dsDuong = new List<string> { "100% đường (Chuẩn)", "70% đường", "50% đường", "30% đường", "Không đường (0%)" };
            var dsDa = new List<string> { "100% đá (Chuẩn)", "70% đá", "50% đá", "Không đá (0%)", "Để đá riêng" };

            var viewModel = new ChiTietSanPhamViewModel
            {
                SanPham = product,
                SanPhamLienQuan = lienQuan,
                DanhSachSize = dsSize,
                DanhSachTopping = dsTopping,
                DanhSachDuong = dsDuong,
                DanhSachDa = dsDa
            };

            return View(viewModel);
        }

        // Dữ liệu mẫu dự phòng đồng bộ 22 món khớp với thư mục Images
        private List<categories> GetMockCategories()
        {
            return new List<categories>
            {
                new categories { id = 1, name = "Trà sữa", description = "Các loại trà sữa thơm béo chuẩn vị", status = true },
                new categories { id = 2, name = "Trà trái cây", description = "Trà trái cây tươi mát thanh nhiệt", status = true },
                new categories { id = 3, name = "Cà phê", description = "Cà phê đậm vị truyền thống và hiện đại", status = true },
                new categories { id = 4, name = "Topping thêm", description = "Topping dẻo dai béo ngậy nấu mới mỗi ngày", status = true }
            };
        }

        private List<products> GetMockProducts()
        {
            var cats = GetMockCategories();
            var cat1 = cats[0];
            var cat2 = cats[1];
            var cat3 = cats[2];
            var cat4 = cats[3];

            return new List<products>
            {
                // Trà sữa (Cat 1)
                new products { id = 1, name = "Trà sữa truyền thống", price = 30000, image = "TraSuaTruyenThong.jpg", description = "Trà sữa thơm béo chuẩn vị truyền thống pha từ hồng trà nguyên lá.", is_featured = true, status = true, category_id = 1, categories = cat1 },
                new products { id = 2, name = "Trà sữa Matcha", price = 35000, image = "TraSuaMatCha.jpg", description = "Matcha Nhật Bản thơm dịu ngọt mát đậm đà tinh túy trà xanh.", is_featured = true, status = true, category_id = 1, categories = cat1 },
                new products { id = 3, name = "Trà sữa Ô Long Nướng", price = 39000, image = "TraSuaOLongNuong.jpg", description = "Vị trà ô long nướng thơm lừng kết hợp cốt sữa đậm đà ngậy béo.", is_featured = true, status = true, category_id = 1, categories = cat1 },
                new products { id = 4, name = "Trà sữa trân châu đường đen", price = 38000, image = "TraSuaTranChauDuongDen.jpg", description = "Trà sữa béo ngậy kèm trân châu nấu đường đen dẻo bùi chuẩn vị Đài Loan.", is_featured = true, status = true, category_id = 1, categories = cat1 },
                new products { id = 5, name = "Trà sữa khoai môn", price = 35000, image = "TraSuaKhoaiMon.jpg", description = "Hương khoai môn bùi bùi thơm ngon khó cưỡng màu tím dịu mắt.", is_featured = false, status = true, category_id = 1, categories = cat1 },
                new products { id = 6, name = "Trà sữa Thái xanh", price = 32000, image = "TraSuaThaiXanh.jpg", description = "Trà thái xanh thanh mát thơm nồng hương hoa hồi thảo mộc.", is_featured = false, status = true, category_id = 1, categories = cat1 },
                new products { id = 7, name = "Trà sữa Socola", price = 36000, image = "TraSuaSocola.jpg", description = "Socola Bỉ nguyên chất đậm đà hòa quyện trà sữa ngọt ngào khó quên.", is_featured = false, status = true, category_id = 1, categories = cat1 },

                // Trà trái cây (Cat 2)
                new products { id = 8, name = "Trà đào cam sả", price = 32000, image = "TraDaoCamSa.jpg", description = "Trà đào thanh mát miếng đào giòn sần sật hương cam sả thơm nồng.", is_featured = true, status = true, category_id = 2, categories = cat2 },
                new products { id = 9, name = "Trà vải thanh mát", price = 30000, image = "TraVaiThanhMat.jpg", description = "Trà vải ngọt thanh dịu mát cho ngày hè tràn đầy năng lượng.", is_featured = false, status = true, category_id = 2, categories = cat2 },
                new products { id = 10, name = "Trà xoài nhiệt đới", price = 35000, image = "TraXoai.jpg", description = "Trà xoài thơm lừng hòa quyện cùng cốt trà lài thanh khiết sảng khoái.", is_featured = true, status = true, category_id = 2, categories = cat2 },
                new products { id = 11, name = "Trà chanh dây tươi", price = 30000, image = "TraChanhDay.jpg", description = "Vị chua ngọt sảng khoái đánh tan cơn khát ngày nắng nóng.", is_featured = false, status = true, category_id = 2, categories = cat2 },
                new products { id = 12, name = "Trà ổi hồng", price = 34000, image = "TraOiHong.jpg", description = "Hương ổi hồng ngọt dịu thơm ngát màu sắc bắt mắt tươi trẻ.", is_featured = false, status = true, category_id = 2, categories = cat2 },
                new products { id = 13, name = "Trà dâu tây tươi", price = 35000, image = "TraDau.jpg", description = "Dâu tây tươi mọng nước chua chua ngọt ngọt tràn đầy vitamin C.", is_featured = true, status = true, category_id = 2, categories = cat2 },

                // Cà phê (Cat 3)
                new products { id = 14, name = "Cà phê sữa đá", price = 28000, image = "CaPheSuaDa.jpg", description = "Cà phê Robusta Đắk Lắk đậm vị hòa quyện sữa đặc béo ngậy.", is_featured = true, status = true, category_id = 3, categories = cat3 },
                new products { id = 15, name = "Cà phê đen đá", price = 25000, image = "CaPheDen.jpg", description = "Đậm đà hương vị cà phê phin truyền thống cho buổi sáng tỉnh táo.", is_featured = false, status = true, category_id = 3, categories = cat3 },
                new products { id = 16, name = "Bạc xỉu 3 tầng", price = 29000, image = "BacXiu.jpg", description = "Vị ngọt béo từ sữa hòa quyện chút đắng cà phê hài hòa tinh tế.", is_featured = true, status = true, category_id = 3, categories = cat3 },
                new products { id = 17, name = "Cà phê muối", price = 32000, image = "CaPheMuoi.jpg", description = "Lớp kem muối béo mặn hòa tan cà phê đậm đà thơm béo độc đáo.", is_featured = false, status = true, category_id = 3, categories = cat3 },

                // Topping (Cat 4)
                new products { id = 18, name = "Trân châu đen dẻo", price = 8000, image = "TranChauDen.jpg", description = "Trân châu truyền thống dai dai dẻo dẻo thơm mùi mật ong.", is_featured = false, status = true, category_id = 4, categories = cat4 },
                new products { id = 19, name = "Trân châu trắng giòn", price = 10000, image = "TranChauTrang.jpg", description = "Trân châu trắng giòn sần sật vui miệng vị ngọt dịu nhẹ.", is_featured = false, status = true, category_id = 4, categories = cat4 },
                new products { id = 20, name = "Pudding trứng", price = 10000, image = "puudingTrung.jpg", description = "Pudding mềm mịn béo ngậy tan ngay đầu lưỡi chuẩn vị bánh flan.", is_featured = false, status = true, category_id = 4, categories = cat4 },
                new products { id = 21, name = "Thạch trái cây", price = 8000, image = "ThachTraiCay.jpg", description = "Thạch thanh mát nhiều hương vị trái cây giòn dai thơm lừng.", is_featured = false, status = true, category_id = 4, categories = cat4 },
                new products { id = 22, name = "Kem Cheese Macchiato", price = 12000, image = "KemChesse.jpg", description = "Lớp kem phô mai béo mặn sánh mịn phủ trên ly trà béo ngậy khó cưỡng.", is_featured = false, status = true, category_id = 4, categories = cat4 }
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && db != null)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
