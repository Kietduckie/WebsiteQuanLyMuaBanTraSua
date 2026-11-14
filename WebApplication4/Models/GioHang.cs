using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication4.Models
{
    public class GioHang
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int ProductSizeId { get; set; }
        public string SizeName { get; set; }
        public decimal BasePrice { get; set; }
        public decimal ExtraPrice { get; set; }
        public int Quantity { get; set; }
        public int SugarLevel { get; set; }
        public int IceLevel { get; set; }
        public string Note { get; set; }
        public List<string> ToppingNames { get; set; }
        public decimal ToppingsTotalPerCup { get; set; }

        public decimal UnitPriceWithToppings
        {
            get { return BasePrice + ExtraPrice + ToppingsTotalPerCup; }
        }

        public decimal LineTotal
        {
            get { return UnitPriceWithToppings * Quantity; }
        }
    }
}
