using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class NguoiDungController : Controller
    {
        private QuanLyMuaBanTraSuaEntities db = new QuanLyMuaBanTraSuaEntities();

        private string MaHoaMatKhau(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.Append("").ToString();
            }
        }

        [HttpGet]
        public ActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangKy(string name, string email, string phone, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ họ tên, email và mật khẩu.";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            bool emailTonTai = db.users.Any(u => u.email == email.Trim());
            if (emailTonTai)
            {
                ViewBag.Error = "Email này đã được sử dụng. Vui lòng chọn email khác.";
                return View();
            }

            users newUser = new users
            {
                name = name.Trim(),
                email = email.Trim(),
                phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
                password_hash = MaHoaMatKhau(password),
                role = "customer",
                status = true,
                created_at = DateTime.Now
            };

            db.users.Add(newUser);
            db.SaveChanges();

            TempData["Success"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
            return RedirectToAction("DangNhap");
        }

        [HttpGet]
        public ActionResult DangNhap()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangNhap(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập email và mật khẩu.";
                return View();
            }

            string hashedPassword = MaHoaMatKhau(password);
            var taiKhoan = db.users.FirstOrDefault(u => u.email == email.Trim() && u.password_hash == hashedPassword);

            if (taiKhoan == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không chính xác.";
                return View();
            }

            if (!taiKhoan.status)
            {
                ViewBag.Error = "Tài khoản của bạn đã bị khóa.";
                return View();
            }

            // Lưu đúng định dạng JSON mà LayoutPage.cshtml của TV2 đang đọc
            Session["TaiKhoan"] = new JavaScriptSerializer().Serialize(new
            {
                id = taiKhoan.id,
                name = taiKhoan.name,
                role = taiKhoan.role
            });

            // Lưu thêm biến đơn cho _AdminLayout.cshtml
            Session["UserId"] = taiKhoan.id;
            Session["UserName"] = taiKhoan.name;
            Session["UserRole"] = taiKhoan.role;

            if (taiKhoan.role == "admin")
            {
                return RedirectToAction("Index", "DanhMuc");
            }

            return RedirectToAction("Index", "TrangChu");
        }

        public ActionResult DangXuat()
        {
            Session.Clear();
            return RedirectToAction("Index", "TrangChu");
        }
    }
}