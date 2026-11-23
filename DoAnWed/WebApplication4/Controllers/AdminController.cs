using System.Web.Mvc;

namespace WebApplication4.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            if (Session["UserRole"] == null || Session["UserRole"].ToString() != "admin")
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }
            return RedirectToAction("Index", "DanhMuc");
        }
    }
}