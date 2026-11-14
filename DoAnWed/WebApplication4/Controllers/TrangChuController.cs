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

           
            return View(GetMockViewModel());
        }

        // Tạo dữ liệu mẫu dự phòng
        private TrangChuViewModel GetMockViewModel()
        {
            var cat1 = new categories { id = 1, name = "Trà sữa", description = "Các loại trà sữa", status = true };
            var cat2 = new categories { id = 2, name = "Trà trái cây", description = "Trà trái cây tươi mát", status = true };
            var cat3 = new categories { id = 3, name = "Cà phê", description = "Các loại cà phê", status = true };

            var dsMock = new List<products>
            {
                new products { id = 1, name = "Trà sữa truyền thống", price = 30000, image = "/images/tra-sua-truyen-thong.jpg", description = "Trà sữa thơm béo chuẩn vị truyền thống.", is_featured = true, status = true, category_id = 1, categories = cat1 },
                new products { id = 2, name = "Trà sữa matcha", price = 35000, image = "/images/tra-sua-matcha.jpg", description = "Matcha Nhật Bản thơm dịu ngọt mát.", is_featured = true, status = true, category_id = 1, categories = cat1 },
                new products { id = 3, name = "Trà đào cam sả", price = 32000, image = "/images/tra-dao.jpg", description = "Trà đào tươi kết hợp cùng hương cam sả thơm nồng.", is_featured = true, status = true, category_id = 2, categories = cat2 },
                new products { id = 4, name = "Trà vải thanh mát", price = 30000, image = "/images/tra-vai.jpg", description = "Trà vải ngọt thanh dịu mát cho ngày hè.", is_featured = false, status = true, category_id = 2, categories = cat2 },
                new products { id = 5, name = "Cà phê sữa đá", price = 28000, image = "/images/ca-phe-sua.jpg", description = "Cà phê đậm vị hòa quyện sữa béo ngậy.", is_featured = false, status = true, category_id = 3, categories = cat3 },
                new products { id = 6, name = "Trà sữa Ô Long Nướng", price = 39000, image = "/images/tra-olong-nuong.jpg", description = "Vị trà ô long nướng thơm lừng đậm đà.", is_featured = true, status = true, category_id = 1, categories = cat1 }
            };

            return new TrangChuViewModel
            {
                SanPhamBanChay = dsMock.Where(p => p.is_featured).ToList(),
                SanPhamMoi = dsMock,
                TatCaSanPham = dsMock,
                DanhMucNoiBat = new List<categories> { cat1, cat2, cat3 }
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
