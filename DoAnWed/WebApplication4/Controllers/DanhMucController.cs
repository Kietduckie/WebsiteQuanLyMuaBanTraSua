using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class DanhMucController : Controller
    {
        private QuanLyMuaBanTraSuaEntities db = new QuanLyMuaBanTraSuaEntities();

        // Kiểm tra quyền Admin trước khi vào các trang quản lý danh mục
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["UserRole"] == null || Session["UserRole"].ToString() != "admin")
            {
                TempData["Error"] = "Bạn cần đăng nhập bằng tài khoản Quản trị viên (Admin) để truy cập trang này.";
                filterContext.Result = RedirectToAction("DangNhap", "NguoiDung");
                return;
            }
            base.OnActionExecuting(filterContext);
        }

        // 1. Danh sách danh mục
        public ActionResult Index()
        {
            var dsDanhMuc = db.categories.OrderBy(c => c.id).ToList();
            ViewBag.ParentNames = dsDanhMuc.ToDictionary(c => c.id, c => c.name);
            return View(dsDanhMuc);
        }

        // 2. Thêm danh mục (GET)
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.ParentList = new SelectList(
                db.categories.Where(c => c.status == true && c.parent_category_id == null).ToList(),
                "id",
                "name"
            );
            return View();
        }

        // 2. Thêm danh mục (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(categories model)
        {
            if (string.IsNullOrWhiteSpace(model.name))
            {
                ModelState.AddModelError("name", "Tên danh mục không được để trống.");
            }

            if (ModelState.IsValid)
            {
                model.name = model.name.Trim();
                model.status = true;
                db.categories.Add(model);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Thêm danh mục mới thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.ParentList = new SelectList(
                db.categories.Where(c => c.status == true && c.parent_category_id == null).ToList(),
                "id",
                "name",
                model.parent_category_id
            );
            return View(model);
        }

        // 3. Sửa danh mục (GET)
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var danhMuc = db.categories.Find(id);
            if (danhMuc == null) return HttpNotFound();

            ViewBag.ParentList = new SelectList(
                db.categories.Where(c => c.id != id && c.parent_category_id == null).ToList(),
                "id",
                "name",
                danhMuc.parent_category_id
            );
            return View(danhMuc);
        }

        // 3. Sửa danh mục (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(categories model)
        {
            if (string.IsNullOrWhiteSpace(model.name))
            {
                ModelState.AddModelError("name", "Tên danh mục không được để trống.");
            }

            if (ModelState.IsValid)
            {
                model.name = model.name.Trim();
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.ParentList = new SelectList(
                db.categories.Where(c => c.id != model.id && c.parent_category_id == null).ToList(),
                "id",
                "name",
                model.parent_category_id
            );
            return View(model);
        }

        // 4. Xóa danh mục (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var danhMuc = db.categories.Find(id);
            if (danhMuc == null) return HttpNotFound();

            bool coSanPham = db.products.Any(p => p.category_id == id);
            bool coDanhMucCon = db.categories.Any(c => c.parent_category_id == id);

            if (coSanPham || coDanhMucCon)
            {
                danhMuc.status = false;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Danh mục đang có sản phẩm hoặc danh mục con nên đã được chuyển sang trạng thái Ẩn.";
            }
            else
            {
                db.categories.Remove(danhMuc);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Đã xóa danh mục thành công!";
            }

            return RedirectToAction("Index");
        }
    }
}