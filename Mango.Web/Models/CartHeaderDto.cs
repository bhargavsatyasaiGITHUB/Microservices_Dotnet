

using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Models
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
        [Required]
        public string? FirstName { get; set; }
        [Required]
        public string? LastName { get; set; }
        [Required]
        public string? Phone { get; set; }
        [Required]
        public string? Email { get; set; }
    }

}
