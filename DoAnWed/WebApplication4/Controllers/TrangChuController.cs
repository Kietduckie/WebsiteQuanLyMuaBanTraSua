using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class TrangChuController : Controller
    {
        private QuanLyMuaBanTraSuaEntities db = new QuanLyMuaBanTraSuaEntities();

        // GET: TrangChu
        public ActionResult Index()
        {
            var viewModel = new TrangChuViewModel();

            var mockData = GetMockViewModel();

            try
            {
                // Lấy sản phẩm và danh mục đang hoạt động (status = true) từ CSDL qua EF
                var dsProducts = db.products
                                   .Include(p => p.categories)
                                   .Where(p => p.status)
                                   .ToList();

                var dsCategories = db.categories
                                     .Where(c => c.status)
                                     .ToList();

                if (dsProducts.Any())
                {
                    // Bổ sung đầy đủ tất cả các món từ thư mục Images nếu CSDL chưa có đủ
                    var existingNames = new HashSet<string>(dsProducts.Select(p => p.name.ToLower().Trim()));
                    foreach (var mp in mockData.TatCaSanPham)
                    {
                        if (!existingNames.Contains(mp.name.ToLower().Trim()))
                        {
                            dsProducts.Add(mp);
                        }
                    }

                    var existingCatIds = new HashSet<int>(dsCategories.Select(c => c.id));
                    foreach (var mc in mockData.DanhMucNoiBat)
                    {
                        if (!existingCatIds.Contains(mc.id))
                        {
                            dsCategories.Add(mc);
                        }
                    }

                    viewModel.SanPhamBanChay = dsProducts.Where(p => p.is_featured).Take(4).ToList();
                    viewModel.SanPhamMoi = dsProducts.OrderByDescending(p => p.id).Take(8).ToList();
                    viewModel.TatCaSanPham = dsProducts;
                    viewModel.DanhMucNoiBat = dsCategories;

                    return View(viewModel);
                }
            }
            catch (Exception)
            {
              
            }

            return View(mockData);
        }

        // Tạo dữ liệu mẫu dự phòng
        private TrangChuViewModel GetMockViewModel()
        {
            var cat1 = new categories { id = 1, name = "Trà sữa", description = "Các loại trà sữa thơm béo", status = true };
            var cat2 = new categories { id = 2, name = "Trà trái cây", description = "Trà trái cây tươi mát giải nhiệt", status = true };
            var cat3 = new categories { id = 3, name = "Cà phê", description = "Các loại cà phê đậm đà", status = true };
            var cat4 = new categories { id = 4, name = "Topping thêm", description = "Topping đa dạng hấp dẫn", status = true };

            var dsMock = new List<products>
            {
                // Trà sữa
                new products { id = 1, name = "Trà sữa truyền thống", price = 30000, image = "TraSuaTruyenThong.jpg", description = "Trà sữa thơm béo chuẩn vị truyền thống.", is_featured = true, status = true, category_id = 1, categories = cat1 },
                new products { id = 2, name = "Trà sữa Matcha", price = 35000, image = "TraSuaMatCha.jpg", description = "Matcha Nhật Bản thơm dịu ngọt mát.", is_featured = true, status = true, category_id = 1, categories = cat1 },
                new products { id = 3, name = "Trà sữa Ô Long Nướng", price = 39000, image = "TraSuaOLongNuong.jpg", description = "Vị trà ô long nướng thơm lừng đậm đà.", is_featured = true, status = true, category_id = 1, categories = cat1 },
                new products { id = 4, name = "Trà sữa trân châu đường đen", price = 38000, image = "TraSuaTranChauDuongDen.jpg", description = "Trà sữa béo ngậy kèm đường đen ngọt ngào.", is_featured = true, status = true, category_id = 1, categories = cat1 },
                new products { id = 5, name = "Trà sữa khoai môn", price = 35000, image = "TraSuaKhoaiMon.jpg", description = "Hương khoai môn bùi bùi thơm ngon khó cưỡng.", is_featured = false, status = true, category_id = 1, categories = cat1 },
                new products { id = 6, name = "Trà sữa Thái xanh", price = 32000, image = "TraSuaThaiXanh.jpg", description = "Trà thái xanh thanh mát, béo thơm chuẩn vị.", is_featured = false, status = true, category_id = 1, categories = cat1 },
                new products { id = 7, name = "Trà sữa Socola", price = 36000, image = "TraSuaSocola.jpg", description = "Socola đậm đà hòa quyện trà sữa thơm ngọt.", is_featured = false, status = true, category_id = 1, categories = cat1 },

                // Trà trái cây
                new products { id = 8, name = "Trà đào cam sả", price = 32000, image = "TraDaoCamSa.jpg", description = "Trà đào tươi kết hợp cùng hương cam sả thơm nồng.", is_featured = true, status = true, category_id = 2, categories = cat2 },
                new products { id = 9, name = "Trà vải thanh mát", price = 30000, image = "TraVaiThanhMat.jpg", description = "Trà vải ngọt thanh dịu mát cho ngày hè.", is_featured = false, status = true, category_id = 2, categories = cat2 },
                new products { id = 10, name = "Trà xoài nhiệt đới", price = 35000, image = "TraXoai.jpg", description = "Trà xoài thơm lừng hòa quyện vị thanh khiết.", is_featured = true, status = true, category_id = 2, categories = cat2 },
                new products { id = 11, name = "Trà chanh dây tươi", price = 30000, image = "TraChanhDay.jpg", description = "Chua ngọt sảng khoái đánh tan cơn khát.", is_featured = false, status = true, category_id = 2, categories = cat2 },
                new products { id = 12, name = "Trà ổi hồng", price = 34000, image = "TraOiHong.jpg", description = "Hương ổi hồng ngọt dịu, màu sắc bắt mắt.", is_featured = false, status = true, category_id = 2, categories = cat2 },
                new products { id = 13, name = "Trà dâu tây tươi", price = 35000, image = "TraDau.jpg", description = "Vị dâu tây chua chua ngọt ngọt thơm mát.", is_featured = true, status = true, category_id = 2, categories = cat2 },

                // Cà phê
                new products { id = 14, name = "Cà phê sữa đá", price = 28000, image = "CaPheSuaDa.jpg", description = "Cà phê đậm vị hòa quyện sữa béo ngậy.", is_featured = true, status = true, category_id = 3, categories = cat3 },
                new products { id = 15, name = "Cà phê đen đá", price = 25000, image = "CaPheDen.jpg", description = "Đậm đà hương vị cà phê phin truyền thống.", is_featured = false, status = true, category_id = 3, categories = cat3 },
                new products { id = 16, name = "Bạc xỉu 3 tầng", price = 29000, image = "BacXiu.jpg", description = "Vị ngọt béo từ sữa hòa quyện chút đắng cà phê.", is_featured = true, status = true, category_id = 3, categories = cat3 },
                new products { id = 17, name = "Cà phê muối", price = 32000, image = "CaPheMuoi.jpg", description = "Lớp kem muối béo mặn hòa tan cà phê đậm đà.", is_featured = false, status = true, category_id = 3, categories = cat3 },

                // Topping
                new products { id = 18, name = "Trân châu đen dẻo", price = 8000, image = "TranChauDen.jpg", description = "Trân châu truyền thống dai dai dẻo dẻo.", is_featured = false, status = true, category_id = 4, categories = cat4 },
                new products { id = 19, name = "Trân châu trắng giòn", price = 10000, image = "TranChauTrang.jpg", description = "Trân châu trắng giòn sần sật vui miệng.", is_featured = false, status = true, category_id = 4, categories = cat4 },
                new products { id = 20, name = "Pudding trứng", price = 10000, image = "puudingTrung.jpg", description = "Pudding mềm mịn, béo ngậy tan ngay đầu lưỡi.", is_featured = false, status = true, category_id = 4, categories = cat4 },
                new products { id = 21, name = "Thạch trái cây", price = 8000, image = "ThachTraiCay.jpg", description = "Thạch thanh mát nhiều hương vị trái cây.", is_featured = false, status = true, category_id = 4, categories = cat4 },
                new products { id = 22, name = "Kem Cheese Macchiato", price = 12000, image = "KemChesse.jpg", description = "Lớp kem phô mai béo mặn sánh mịn thơm lừng.", is_featured = false, status = true, category_id = 4, categories = cat4 }
            };

            return new TrangChuViewModel
            {
                SanPhamBanChay = dsMock.Where(p => p.is_featured).ToList(),
                SanPhamMoi = dsMock.Take(8).ToList(),
                TatCaSanPham = dsMock,
                DanhMucNoiBat = new List<categories> { cat1, cat2, cat3, cat4 }
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
