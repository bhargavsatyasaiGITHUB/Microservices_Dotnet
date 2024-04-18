﻿

using System.ComponentModel.DataAnnotations;

namespace Mango.Services.ShoppingCartAPI.Models.Dto
{
    public class CartHeaderDto
    {
        
        public string Title { get; set; }
        [Key]
        public int CartHeaderId { get; set; } = 1;
        public string? UserId { get; set; }
        public string? CouponId { get; set; }
        public double Discount { get; set; }
        public double CartTotal { get; set; }
        public string? FirstName {  get; set; }
        public string? LastName { get; set;}
        public string? Phone { get; set;}
        public string? Email { get; set; }
    }
}
