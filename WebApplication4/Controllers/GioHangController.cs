using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class GioHangController : Controller
    {
        private QuanLyMuaBanTraSuaEntities db = new QuanLyMuaBanTraSuaEntities();

        // Lấy user_id hiện tại từ Session["TaiKhoan"]
        private int? GetCurrentUserId()
        {
            var tk = Session["TaiKhoan"];
            if (tk == null) return null;

            if (tk is string jsonStr)
            {
                try
                {
                    var dict = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(jsonStr);
                    if (dict != null && dict.ContainsKey("id"))
                        return Convert.ToInt32(dict["id"]);
                }
                catch { }
                return null;
            }

            var prop = tk.GetType().GetProperty("id") ?? tk.GetType().GetProperty("Id");
            return prop != null ? (int?)Convert.ToInt32(prop.GetValue(tk, null)) : null;
        }

        // Lấy hoặc khởi tạo giỏ hàng theo user_id hoặc session_key
        private carts GetOrCreateCart()
        {
            int? userId = GetCurrentUserId();
            carts userCart;

            if (userId.HasValue)
            {
                userCart = db.carts.FirstOrDefault(c => c.user_id == userId.Value);
            }
            else
            {
                string sessionKey = Session["CartSessionKey"] as string;
                if (string.IsNullOrEmpty(sessionKey))
                {
                    sessionKey = Guid.NewGuid().ToString("N");
                    Session["CartSessionKey"] = sessionKey;
                }
                userCart = db.carts.FirstOrDefault(c => c.session_key == sessionKey);
            }

            if (userCart == null)
            {
                userCart = new carts
                {
                    user_id = userId,
                    session_key = userId.HasValue ? null : (Session["CartSessionKey"] as string),
                    created_at = DateTime.Now
                };
                db.carts.Add(userCart);
                db.SaveChanges();
            }

            return userCart;
        }

        // Cập nhật Session["GioHang"] để badge trên LayoutPage hiển thị đúng số lượng
        private void SyncCartSessionBadge(int cartId)
        {
            Session["GioHang"] = db.cart_items.Where(ci => ci.cart_id == cartId).ToList();
        }

        // Dùng LINQ Join + GroupJoin dựng danh sách ViewModel GioHang trong 1 lần truy vấn
        private List<GioHang> BuildCartViewModels(int cartId)
        {
            var query = from ci in db.cart_items
                        where ci.cart_id == cartId
                        join p in db.products on ci.product_id equals p.id into prodGroup
                        from prod in prodGroup.DefaultIfEmpty()
                        join s in db.product_sizes on ci.product_size_id equals s.id into sizeGroup
                        from size in sizeGroup.DefaultIfEmpty()
                        select new
                        {
                            Item = ci,
                            Product = prod,
                            Size = size,
                            Toppings = (from cit in db.cart_item_toppings
                                        where cit.cart_item_id == ci.id
                                        join t in db.toppings on cit.topping_id equals t.id
                                        select new
                                        {
                                            t.name,
                                            t.price,
                                            cit.quantity
                                        }).ToList()
                        };

            return query.AsEnumerable().Select(x => new GioHang
            {
                CartItemId = x.Item.id,
                ProductId = x.Item.product_id,
                ProductName = x.Product?.name ?? "Sản phẩm",
                ProductImage = x.Product?.image,
                ProductSizeId = x.Item.product_size_id,
                SizeName = x.Size?.size_name ?? "M",
                BasePrice = x.Product?.price ?? 0,
                ExtraPrice = x.Size?.extra_price ?? 0,
                Quantity = x.Item.quantity,
                SugarLevel = x.Item.sugar_level,
                IceLevel = x.Item.ice_level,
                Note = x.Item.note,
                ToppingNames = x.Toppings
                    .Select(t => t.quantity > 1 ? $"{t.name} (x{t.quantity})" : t.name)
                    .ToList(),
                ToppingsTotalPerCup = x.Toppings.Sum(t => t.price * t.quantity)
            }).ToList();
        }

        // Kiểm tra và tính toán Voucher (Giảm món trực tiếp vs Giảm ship khi thanh toán)
        private object EvaluateVoucher(string code, decimal subtotal)
        {
            code = (code ?? "").Trim().ToUpper();
            if (string.IsNullOrEmpty(code))
            {
                Session.Remove("CartVoucher");
                Session.Remove("CartVoucherType");
                Session.Remove("CartVoucherDiscount");
                return new
                {
                    success = true,
                    code = "",
                    voucherType = "",
                    discount = 0m,
                    finalTotal = subtotal,
                    message = ""
                };
            }

            decimal discount = 0;
            string voucherType = "ITEM"; // "ITEM": giảm trực tiếp tiền món | "SHIP": giảm ship khi thanh toán
            string message = "";

            // Tìm mã trong bảng promotions bằng LINQ
            var promo = db.promotions.FirstOrDefault(p => p.code.ToUpper() == code && p.status);
            if (promo != null)
            {
                DateTime now = DateTime.Now;
                if (promo.start_date > now || promo.end_date < now)
                    return new { success = false, message = $"Mã {code} đã hết hạn hoặc chưa đến thời gian áp dụng." };

                if (promo.usage_limit.HasValue && promo.used_count >= promo.usage_limit.Value)
                    return new { success = false, message = $"Mã {code} đã hết lượt sử dụng." };

                if (subtotal < promo.min_order_amount)
                    return new { success = false, message = $"Mã {code} chỉ áp dụng cho đơn từ {promo.min_order_amount:N0}đ." };

                string pType = (promo.discount_type ?? "").ToUpper();
                if (pType == "SHIP" || pType == "FREESHIP")
                {
                    voucherType = "SHIP";
                    discount = 0;
                    message = $"Miễn phí vận chuyển (tối đa {(promo.max_discount_amount ?? promo.discount_value):N0}đ) khi thanh toán";
                }
                else if (pType == "PERCENT")
                {
                    voucherType = "ITEM";
                    decimal rawDiscount = subtotal * (promo.discount_value / 100m);
                    discount = promo.max_discount_amount.HasValue
                        ? Math.Min(rawDiscount, promo.max_discount_amount.Value)
                        : rawDiscount;
                    message = $"Đã giảm {discount:N0}đ trực tiếp vào tiền món";
                }
                else
                {
                    voucherType = "ITEM";
                    discount = promo.discount_value;
                    message = $"Đã giảm {discount:N0}đ trực tiếp vào tiền món";
                }
            }
            else
            {
                switch (code)
                {
                    case "GIAM20K":
                        if (subtotal < 100000)
                            return new { success = false, message = "Mã GIAM20K chỉ áp dụng cho đơn từ 100.000đ." };
                        discount = 20000;
                        message = "Đã giảm 20.000đ trực tiếp vào tiền món";
                        break;

                    case "MILKTEA15":
                        discount = Math.Min(subtotal * 0.15m, 30000);
                        message = $"Đã giảm {discount:N0}đ (15%) trực tiếp vào tiền món";
                        break;

                    case "GIAM5K":
                        if (subtotal < 30000)
                            return new { success = false, message = "Mã GIAM5K chỉ áp dụng cho đơn từ 30.000đ." };
                        discount = 5000;
                        message = "Đã giảm 5.000đ trực tiếp vào tiền món";
                        break;

                    case "FREESHIP":
                        voucherType = "SHIP";
                        discount = 0;
                        message = "Miễn phí vận chuyển (giảm ship khi thanh toán)";
                        break;

                    default:
                        return new { success = false, message = $"Mã khuyến mãi \"{code}\" không hợp lệ." };
                }
            }

            discount = Math.Min(discount, subtotal);

            Session["CartVoucher"] = code;
            Session["CartVoucherType"] = voucherType;
            Session["CartVoucherDiscount"] = discount;

            return new
            {
                success = true,
                code,
                voucherType,
                discount,
                finalTotal = subtotal - discount,
                message
            };
        }

        // GET: /GioHang/Index
        public ActionResult Index()
        {
            var userCart = GetOrCreateCart();
            var viewModels = BuildCartViewModels(userCart.id);
            SyncCartSessionBadge(userCart.id);

            ViewBag.AppliedVoucher = Session["CartVoucher"] as string ?? "";
            return View(viewModels);
        }

        // GET/POST: /GioHang/AddToCart
        public ActionResult AddToCart(int product_id, int? product_size_id = null, int quantity = 1, int sugar_level = 100, int ice_level = 100, string note = null, int[] topping_ids = null)
        {
            var prod = db.products.FirstOrDefault(p => p.id == product_id && p.status);
            if (prod == null) return HttpNotFound();

            var size = product_size_id.HasValue
                ? db.product_sizes.FirstOrDefault(s => s.id == product_size_id.Value && s.status)
                : db.product_sizes.Where(s => s.product_id == product_id && s.status).OrderBy(s => s.id).FirstOrDefault();

            if (size == null) return HttpNotFound();

            var userCart = GetOrCreateCart();
            var normalizedToppings = (topping_ids ?? new int[0]).OrderBy(x => x).ToList();
            string cleanNote = note ?? "";

            // Tìm món trùng cấu hình (sản phẩm, size, đường, đá, ghi chú và danh sách topping) bằng LINQ
            var matchedItem = db.cart_items
                .Where(ci => ci.cart_id == userCart.id
                          && ci.product_id == product_id
                          && ci.product_size_id == size.id
                          && ci.sugar_level == sugar_level
                          && ci.ice_level == ice_level
                          && (ci.note ?? "") == cleanNote)
                .AsEnumerable()
                .FirstOrDefault(ci => db.cart_item_toppings
                    .Where(t => t.cart_item_id == ci.id)
                    .Select(t => t.topping_id)
                    .OrderBy(id => id)
                    .SequenceEqual(normalizedToppings));

            if (matchedItem != null)
            {
                matchedItem.quantity += Math.Max(1, quantity);
            }
            else
            {
                var newItem = new cart_items
                {
                    cart_id = userCart.id,
                    product_id = product_id,
                    product_size_id = size.id,
                    quantity = Math.Max(1, quantity),
                    sugar_level = sugar_level,
                    ice_level = ice_level,
                    note = note
                };
                db.cart_items.Add(newItem);
                db.SaveChanges();

                if (normalizedToppings.Any())
                {
                    var toppingEntities = normalizedToppings.Select(tid => new cart_item_toppings
                    {
                        cart_item_id = newItem.id,
                        topping_id = tid,
                        quantity = 1
                    });
                    db.cart_item_toppings.AddRange(toppingEntities);
                }
            }

            db.SaveChanges();
            SyncCartSessionBadge(userCart.id);

            return Request.UrlReferrer != null
                ? (ActionResult)Redirect(Request.UrlReferrer.ToString())
                : RedirectToAction("Index");
        }

        // POST: AJAX Tăng / Giảm số lượng món
        [HttpPost]
        public JsonResult UpdateQuantityAjax(int cart_item_id, int quantity)
        {
            var userCart = GetOrCreateCart();
            var item = db.cart_items.FirstOrDefault(ci => ci.id == cart_item_id && ci.cart_id == userCart.id);

            bool removed = false;
            if (item != null)
            {
                if (quantity <= 0)
                {
                    db.cart_item_toppings.RemoveRange(db.cart_item_toppings.Where(t => t.cart_item_id == cart_item_id));
                    db.cart_items.Remove(item);
                    removed = true;
                }
                else
                {
                    item.quantity = quantity;
                }
                db.SaveChanges();
            }

            var updatedList = BuildCartViewModels(userCart.id);
            SyncCartSessionBadge(userCart.id);

            var updatedItem = updatedList.FirstOrDefault(x => x.CartItemId == cart_item_id);
            decimal subtotal = updatedList.Sum(x => x.LineTotal);
            int totalItems = updatedList.Sum(x => x.Quantity);
            var voucherResult = EvaluateVoucher(Session["CartVoucher"] as string ?? "", subtotal);

            return Json(new
            {
                success = true,
                removed,
                itemQuantity = updatedItem?.Quantity ?? 0,
                itemLineTotal = updatedItem?.LineTotal ?? 0,
                subtotal,
                totalItems,
                isEmpty = !updatedList.Any(),
                voucher = voucherResult
            });
        }

        // POST: AJAX Xóa từng món
        [HttpPost]
        public JsonResult RemoveItemAjax(int cart_item_id)
        {
            var userCart = GetOrCreateCart();
            var item = db.cart_items.FirstOrDefault(ci => ci.id == cart_item_id && ci.cart_id == userCart.id);

            if (item != null)
            {
                db.cart_item_toppings.RemoveRange(db.cart_item_toppings.Where(t => t.cart_item_id == cart_item_id));
                db.cart_items.Remove(item);
                db.SaveChanges();
            }

            var updatedList = BuildCartViewModels(userCart.id);
            SyncCartSessionBadge(userCart.id);

            decimal subtotal = updatedList.Sum(x => x.LineTotal);
            int totalItems = updatedList.Sum(x => x.Quantity);
            var voucherResult = EvaluateVoucher(Session["CartVoucher"] as string ?? "", subtotal);

            return Json(new
            {
                success = true,
                subtotal,
                totalItems,
                isEmpty = !updatedList.Any(),
                voucher = voucherResult
            });
        }

        // POST: AJAX Xóa tất cả món
        [HttpPost]
        public JsonResult ClearCartAjax()
        {
            var userCart = GetOrCreateCart();
            var itemsQuery = db.cart_items.Where(ci => ci.cart_id == userCart.id);
            var itemIds = itemsQuery.Select(i => i.id);

            db.cart_item_toppings.RemoveRange(db.cart_item_toppings.Where(t => itemIds.Contains(t.cart_item_id)));
            db.cart_items.RemoveRange(itemsQuery);
            db.SaveChanges();

            Session.Remove("GioHang");
            Session.Remove("CartVoucher");
            Session.Remove("CartVoucherType");
            Session.Remove("CartVoucherDiscount");

            return Json(new { success = true });
        }

        // POST: AJAX Áp dụng / Bỏ chọn Voucher
        [HttpPost]
        public JsonResult ApplyVoucherAjax(string code)
        {
            var userCart = GetOrCreateCart();
            decimal subtotal = BuildCartViewModels(userCart.id).Sum(x => x.LineTotal);
            return Json(EvaluateVoucher(code, subtotal));
        }

        // Các Action GET dự phòng
        public ActionResult UpdateQuantity(int cart_item_id, int quantity)
        {
            UpdateQuantityAjax(cart_item_id, quantity);
            return RedirectToAction("Index");
        }

        public ActionResult RemoveItem(int cart_item_id)
        {
            RemoveItemAjax(cart_item_id);
            return RedirectToAction("Index");
        }

        public ActionResult ClearCart()
        {
            ClearCartAjax();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}