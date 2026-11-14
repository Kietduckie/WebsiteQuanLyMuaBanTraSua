using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class ThanhToanController : Controller
    {
        private QuanLyMuaBanTraSuaEntities db = new QuanLyMuaBanTraSuaEntities();
        private const decimal DEFAULT_SHIPPING_FEE = 15000m;

        private int? GetCurrentUserId()
        {
            var tk = Session["TaiKhoan"];
            if (tk == null) return null;

            if (tk is int intId) return intId;

            if (tk is string jsonStr)
            {
                if (int.TryParse(jsonStr, out int parsedId)) return parsedId;
                try
                {
                    var dict = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(jsonStr);
                    if (dict != null)
                    {
                        if (dict.ContainsKey("id") && dict["id"] != null) return Convert.ToInt32(dict["id"]);
                        if (dict.ContainsKey("Id") && dict["Id"] != null) return Convert.ToInt32(dict["Id"]);
                        if (dict.ContainsKey("user_id") && dict["user_id"] != null) return Convert.ToInt32(dict["user_id"]);
                    }
                }
                catch { }
                return null;
            }

            var prop = tk.GetType().GetProperty("id")
                    ?? tk.GetType().GetProperty("Id")
                    ?? tk.GetType().GetProperty("user_id");
            return prop != null ? (int?)Convert.ToInt32(prop.GetValue(tk, null)) : null;
        }

        private carts GetUserCart(int? userId)
        {
            if (userId.HasValue)
                return db.carts.FirstOrDefault(c => c.user_id == userId.Value);

            string sessionKey = Session["CartSessionKey"] as string;
            return !string.IsNullOrEmpty(sessionKey)
                ? db.carts.FirstOrDefault(c => c.session_key == sessionKey)
                : null;
        }

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
                                            t.id,
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

        // Tính toán khuyến mãi áp dụng cho đơn hàng
        private void ApplyPromotionCalculation(ThanhToan model, string voucherCode, out int? promotionId)
        {
            promotionId = null;
            model.AppliedVoucherCode = "";
            model.VoucherType = "";
            model.ItemDiscount = 0;
            model.ShippingDiscount = 0;

            string code = (voucherCode ?? "").Trim().ToUpper();
            if (string.IsNullOrEmpty(code)) return;

            var promo = db.promotions.FirstOrDefault(p => p.code.ToUpper() == code && p.status);
            if (promo != null)
            {
                DateTime now = DateTime.Now;
                bool validDate = promo.start_date <= now && promo.end_date >= now;
                bool validLimit = !promo.usage_limit.HasValue || promo.used_count < promo.usage_limit.Value;
                bool validMin = model.Subtotal >= promo.min_order_amount;

                if (validDate && validLimit && validMin)
                {
                    promotionId = promo.id;
                    model.AppliedVoucherCode = promo.code.ToUpper();
                    string pType = (promo.discount_type ?? "").ToUpper();

                    if (pType == "SHIP" || pType == "FREESHIP")
                    {
                        model.VoucherType = "SHIP";
                        decimal maxShip = promo.max_discount_amount ?? promo.discount_value;
                        model.ShippingDiscount = Math.Min(model.ShippingFee, maxShip > 0 ? maxShip : model.ShippingFee);
                    }
                    else if (pType == "PERCENT")
                    {
                        model.VoucherType = "ITEM";
                        decimal raw = model.Subtotal * (promo.discount_value / 100m);
                        decimal capped = promo.max_discount_amount.HasValue ? Math.Min(raw, promo.max_discount_amount.Value) : raw;
                        model.ItemDiscount = Math.Min(capped, model.Subtotal);
                    }
                    else
                    {
                        model.VoucherType = "ITEM";
                        model.ItemDiscount = Math.Min(promo.discount_value, model.Subtotal);
                    }
                    return;
                }
            }

            // Các mã demo đồng bộ với trang Giỏ hàng
            switch (code)
            {
                case "GIAM20K":
                    if (model.Subtotal >= 100000)
                    {
                        model.AppliedVoucherCode = code;
                        model.VoucherType = "ITEM";
                        model.ItemDiscount = 20000m;
                    }
                    break;
                case "MILKTEA15":
                    model.AppliedVoucherCode = code;
                    model.VoucherType = "ITEM";
                    model.ItemDiscount = Math.Min(model.Subtotal * 0.15m, 30000m);
                    break;
                case "GIAM5K":
                    if (model.Subtotal >= 30000)
                    {
                        model.AppliedVoucherCode = code;
                        model.VoucherType = "ITEM";
                        model.ItemDiscount = 5000m;
                    }
                    break;
                case "FREESHIP":
                    model.AppliedVoucherCode = code;
                    model.VoucherType = "SHIP";
                    model.ShippingDiscount = model.ShippingFee;
                    break;
            }
        }

        //[HttpGet]
        //public ActionResult Checkout()
        //{
        //    int? userId = GetCurrentUserId();
        //    var userCart = GetUserCart(userId);
        //    var cartItems = userCart != null ? BuildCartViewModels(userCart.id) : new List<GioHang>();

        //    // DỮ LIỆU ẢO TẠM THỜI ĐỂ TEST GIAO DIỆN THANH TOÁN (Xóa khi chạy thật)
        //    if (!cartItems.Any())
        //    {
        //        cartItems.Add(new GioHang
        //        {
        //            CartItemId = 1,
        //            ProductId = 1,
        //            ProductName = "Trà Sữa Trân Châu Đường Đen",
        //            ProductImage = "/images/tra-sua-truyen-thong.jpg",
        //            SizeName = "L",
        //            BasePrice = 30000,
        //            ExtraPrice = 5000,
        //            ToppingsTotalPerCup = 5000,
        //            SugarLevel = 70,
        //            IceLevel = 50,
        //            ToppingNames = new List<string> { "Trân châu đen" },
        //            Quantity = 2
        //        });
        //    }

        //    var model = new ThanhToan
        //    {
        //        CartItems = cartItems,
        //        Subtotal = cartItems.Sum(x => x.LineTotal),
        //        ShippingFee = DEFAULT_SHIPPING_FEE,
        //        PaymentMethod = "COD"
        //    };

        //    // Load thông tin đăng ký (name, phone) + địa chỉ giao hàng gần nhất (nếu đã từng điền)
        //    if (userId.HasValue)
        //    {
        //        var user = db.users.Find(userId.Value);
        //        if (user != null)
        //        {
        //            model.ReceiverName = user.name;
        //            model.Phone = user.phone;
        //        }

        //        // Lấy địa chỉ từ đơn hàng gần nhất của khách (nếu chưa có đơn nào sẽ là null/rỗng)
        //        var lastOrderWithAddress = db.orders
        //            .Where(o => o.user_id == userId.Value && !string.IsNullOrEmpty(o.delivery_address))
        //            .OrderByDescending(o => o.created_at)
        //            .FirstOrDefault();

        //        if (lastOrderWithAddress != null)
        //        {
        //            model.DeliveryAddress = lastOrderWithAddress.delivery_address;
        //        }
        //    }

        //    string savedVoucher = Session["CartVoucher"] as string ?? "";
        //    int? promoId;
        //    ApplyPromotionCalculation(model, savedVoucher, out promoId);

        //    return View(model);
        //}

        // GET: /Orders/Checkout
        [HttpGet]
        public ActionResult Checkout()
        {
            int? userId = GetCurrentUserId();
            var userCart = GetUserCart(userId);
            if (userCart == null)
                return RedirectToAction("Index", "GioHang");

            var cartItems = BuildCartViewModels(userCart.id);
            if (!cartItems.Any())
                return RedirectToAction("Index", "GioHang");

            var model = new ThanhToan
            {
                CartItems = cartItems,
                Subtotal = cartItems.Sum(x => x.LineTotal),
                ShippingFee = DEFAULT_SHIPPING_FEE,
                PaymentMethod = "COD"
            };

            // Tự động điền thông tin từ tài khoản đang đăng nhập (nếu có)
            if (userId.HasValue)
            {
                var user = db.users.Find(userId.Value);
                if (user != null)
                {
                    model.ReceiverName = user.name;
                    model.Phone = user.phone;
                }
            }

            string savedVoucher = Session["CartVoucher"] as string ?? "";
            int? promoId;
            ApplyPromotionCalculation(model, savedVoucher, out promoId);

            return View(model);
        }

        // POST: /Orders/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(ThanhToan form)
        {
            int? userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                TempData["Error"] = "Vui lòng đăng nhập tài khoản trước khi xác nhận đặt hàng.";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "Orders") });
            }

            var userCart = GetUserCart(userId);
            if (userCart == null)
                return RedirectToAction("Index", "GioHang");

            var cartItems = BuildCartViewModels(userCart.id);
            if (!cartItems.Any())
                return RedirectToAction("Index", "GioHang");

            form.CartItems = cartItems;
            form.Subtotal = cartItems.Sum(x => x.LineTotal);
            form.ShippingFee = DEFAULT_SHIPPING_FEE;

            string voucherCode = !string.IsNullOrWhiteSpace(form.AppliedVoucherCode)
                ? form.AppliedVoucherCode
                : (Session["CartVoucher"] as string ?? "");

            int? promotionId;
            ApplyPromotionCalculation(form, voucherCode, out promotionId);

            if (!ModelState.IsValid)
            {
                return View(form);
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 1. Lưu bảng orders
                    var order = new orders
                    {
                        user_id = userId.Value,
                        promotion_id = promotionId,
                        receiver_name = form.ReceiverName.Trim(),
                        phone = form.Phone.Trim(),
                        delivery_address = form.DeliveryAddress.Trim(),
                        subtotal = form.Subtotal,
                        discount_amount = form.TotalDiscount,
                        total = Math.Max(0, form.FinalTotal),
                        payment_method = string.IsNullOrWhiteSpace(form.PaymentMethod) ? "COD" : form.PaymentMethod,
                        payment_status = form.PaymentMethod == "COD" ? "unpaid" : "paid",
                        status = "new",
                        created_at = DateTime.Now
                    };
                    db.orders.Add(order);
                    db.SaveChanges();

                    // 2. Lưu bảng order_items & order_item_toppings (snapshot tên & giá tại thời điểm mua)
                    var dbCartItems = db.cart_items.Where(ci => ci.cart_id == userCart.id).ToList();
                    foreach (var ci in dbCartItems)
                    {
                        var prod = db.products.Find(ci.product_id);
                        var size = db.product_sizes.Find(ci.product_size_id);

                        decimal unitPrice = (prod?.price ?? 0) + (size?.extra_price ?? 0);

                        var ciToppings = (from cit in db.cart_item_toppings
                                          where cit.cart_item_id == ci.id
                                          join t in db.toppings on cit.topping_id equals t.id
                                          select new { cit, t }).ToList();

                        decimal toppingPerCup = ciToppings.Sum(x => x.t.price * x.cit.quantity);
                        decimal lineTotal = (unitPrice + toppingPerCup) * ci.quantity;

                        // Gộp ghi chú chung của đơn hàng nếu món chưa có ghi chú riêng
                        string itemNote = ci.note;
                        if (!string.IsNullOrWhiteSpace(form.OrderNote))
                        {
                            itemNote = string.IsNullOrWhiteSpace(itemNote)
                                ? $"[Ghi chú đơn: {form.OrderNote.Trim()}]"
                                : $"{itemNote} | [Ghi chú đơn: {form.OrderNote.Trim()}]";
                        }

                        var orderItem = new order_items
                        {
                            order_id = order.id,
                            product_id = ci.product_id,
                            product_size_id = ci.product_size_id,
                            product_name_snapshot = prod?.name ?? "Sản phẩm",
                            size_name_snapshot = size?.size_name ?? "M",
                            quantity = ci.quantity,
                            unit_price = unitPrice,
                            sugar_level = ci.sugar_level,
                            ice_level = ci.ice_level,
                            note = itemNote,
                            line_total = lineTotal
                        };
                        db.order_items.Add(orderItem);
                        db.SaveChanges();

                        foreach (var top in ciToppings)
                        {
                            db.order_item_toppings.Add(new order_item_toppings
                            {
                                order_item_id = orderItem.id,
                                topping_id = top.t.id,
                                topping_name_snapshot = top.t.name,
                                unit_price = top.t.price,
                                quantity = top.cit.quantity
                            });
                        }
                    }

                    // 3. Lưu lịch sử trạng thái đơn hàng (order_status_histories)
                    db.order_status_histories.Add(new order_status_histories
                    {
                        order_id = order.id,
                        old_status = null,
                        new_status = "new",
                        changed_by = userId.Value,
                        changed_at = DateTime.Now,
                        note = string.IsNullOrWhiteSpace(form.OrderNote)
                            ? "Khách tạo đơn hàng"
                            : $"Khách tạo đơn - Ghi chú: {form.OrderNote.Trim()}"
                    });

                    // 4. Cập nhật lượt dùng khuyến mãi (nếu dùng mã từ bảng promotions)
                    if (promotionId.HasValue)
                    {
                        var usedPromo = db.promotions.Find(promotionId.Value);
                        if (usedPromo != null)
                        {
                            usedPromo.used_count += 1;
                        }
                    }

                    // 5. Xóa giỏ hàng sau khi đặt thành công
                    var cartItemIds = dbCartItems.Select(x => x.id).ToList();
                    db.cart_item_toppings.RemoveRange(db.cart_item_toppings.Where(t => cartItemIds.Contains(t.cart_item_id)));
                    db.cart_items.RemoveRange(dbCartItems);

                    db.SaveChanges();
                    transaction.Commit();

                    Session.Remove("GioHang");
                    Session.Remove("CartVoucher");
                    Session.Remove("CartVoucherType");
                    Session.Remove("CartVoucherDiscount");

                    return RedirectToAction("Success", new { id = order.id });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "Không thể tạo đơn hàng: " + ex.Message);
                    return View(form);
                }
            }
        }

        // POST: AJAX đổi voucher ngay tại trang Thanh toán
        [HttpPost]
        public JsonResult ApplyCheckoutVoucherAjax(string code)
        {
            int? userId = GetCurrentUserId();
            var userCart = GetUserCart(userId);
            var cartItems = userCart != null ? BuildCartViewModels(userCart.id) : new List<GioHang>();
            decimal subtotal = cartItems.Sum(x => x.LineTotal);

            var tempModel = new ThanhToan
            {
                Subtotal = subtotal,
                ShippingFee = DEFAULT_SHIPPING_FEE
            };

            string cleanCode = (code ?? "").Trim().ToUpper();
            if (string.IsNullOrEmpty(cleanCode))
            {
                Session.Remove("CartVoucher");
                return Json(new
                {
                    success = true,
                    code = "",
                    voucherType = "",
                    discount = 0m,
                    shippingFee = tempModel.ShippingFee,
                    finalTotal = tempModel.FinalTotal
                });
            }

            int? promoId;
            ApplyPromotionCalculation(tempModel, cleanCode, out promoId);

            if (string.IsNullOrEmpty(tempModel.AppliedVoucherCode))
            {
                return Json(new { success = false, message = $"Mã \"{cleanCode}\" không hợp lệ hoặc chưa đạt giá trị đơn tối thiểu." });
            }

            Session["CartVoucher"] = tempModel.AppliedVoucherCode;
            return Json(new
            {
                success = true,
                code = tempModel.AppliedVoucherCode,
                voucherType = tempModel.VoucherType,
                discount = tempModel.TotalDiscount,
                shippingFee = tempModel.ShippingFee - tempModel.ShippingDiscount,
                finalTotal = tempModel.FinalTotal
            });
        }

        // GET: /Orders/Success/5
        public ActionResult Success(int id)
        {
            var order = db.orders.Find(id);
            if (order == null) return HttpNotFound();
            return View(order);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}