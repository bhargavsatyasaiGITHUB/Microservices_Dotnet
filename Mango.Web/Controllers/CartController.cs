using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        
        public CartController(ICartService cartService,IOrderService orderService)
        {
            _cartService = cartService;
            _orderService = orderService;
                
        }

        public async Task<IActionResult> Remove(int cartDetailId)
        {
            var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
            ResponseDto? response = await _cartService.RemoveFromCartAsync(cartDetailId);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Cart Updated successfully";
                return RedirectToAction(nameof(CartIndex));
            }
           
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(CartDto cartDto)
        {
            ResponseDto? response = await _cartService.ApplyCouponsAsync(cartDto);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Cart Updated successfully";
                return RedirectToAction(nameof(CartIndex));
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EmailCart(CartDto cartDto)
        {
            CartDto cart = await LoadCartDtoBasedOnLoggedInUser();
            cart.CartHeader.Email= User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Email)?.FirstOrDefault()?.Value;
            ResponseDto? response = await _cartService.EmailCart(cart);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Email will be processed and sent shortly.";
                return RedirectToAction(nameof(CartIndex));
            }

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> RemoveCoupon(CartDto cartDto)
        {
            cartDto.CartHeader.CouponId = "";
            ResponseDto? response = await _cartService.ApplyCouponsAsync(cartDto);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Cart Updated successfully";
                return RedirectToAction(nameof(CartIndex));
            }

            return View();
        }


       // [Authorize]
        public async Task<IActionResult> CartIndex()
        {
            return View(await LoadCartDtoBasedOnLoggedInUser());
        }

        public async Task<IActionResult> Checkout()
        {
            return View(await LoadCartDtoBasedOnLoggedInUser());
        }

        [HttpPost]
        [ActionName("Checkout")]
        public async Task<IActionResult> Checkout(CartDto cartDto)
        {
         CartDto cart=  await LoadCartDtoBasedOnLoggedInUser();
            cart.CartHeader.Phone=cartDto.CartHeader.Phone;
            cart.CartHeader.Email=cartDto.CartHeader.Email;
            cart.CartHeader.FirstName=cartDto.CartHeader.FirstName;
            cart.CartHeader.LastName=cartDto.CartHeader.LastName;


            var response=await _orderService.CreateOrder(cart);

            OrderHeaderDto orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));

            if (response!=null &&response.IsSuccess)
            {
                // get stripe session and redirect to stripe to place order
            }
            return View();

        }


        private async Task<CartDto> LoadCartDtoBasedOnLoggedInUser()
        {
            var userId=User.Claims.Where(u=>u.Type==JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
            ResponseDto? response=await _cartService.GetCartByUserIdAsync(userId);

            if (response!=null && response.IsSuccess)
            {
                CartDto cart=JsonConvert.DeserializeObject<CartDto>(Convert.ToString(response.Result));
                return cart;
            }
            else
            {
                return new CartDto();
            }
        }
    }
}
