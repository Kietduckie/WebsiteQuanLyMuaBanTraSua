using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication4.Models
{
    public class ThanhToan
    {
            [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
            [StringLength(100)]
            public string ReceiverName { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
            [StringLength(15)]
            public string Phone { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
            [StringLength(255)]
            public string DeliveryAddress { get; set; }

            public string OrderNote { get; set; }

            [Required]
            public string PaymentMethod { get; set; } = "COD";

            public string AppliedVoucherCode { get; set; }
            public string VoucherType { get; set; } // "ITEM" hoặc "SHIP"

            public decimal Subtotal { get; set; }
            public decimal ShippingFee { get; set; } = 15000m;
            public decimal ShippingDiscount { get; set; }
            public decimal ItemDiscount { get; set; }

            public decimal TotalDiscount => ItemDiscount + ShippingDiscount;
            public decimal FinalTotal => (Subtotal - ItemDiscount) + (ShippingFee - ShippingDiscount);

            public List<GioHang> CartItems { get; set; } = new List<GioHang>();
        }
    }

