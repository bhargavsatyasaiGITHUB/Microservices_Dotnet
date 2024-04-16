using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
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

        public async Task<IActionResult> Confirmation(int orderId)
        {
            ResponseDto? response = await _orderService.ValidateStripeSession(orderId);

            if (response != null && response.IsSuccess)
            {
                OrderHeaderDto orderHeader = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));
                if (orderHeader.Status==SD.Status_Approved)
                {
                    return View(orderId);
                }
            }
            // redirect to some error page based on status
            return View(orderId);
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

                var domain = Request.Scheme + "://" + Request.Host.Value + "/";
                StripeRequestDto stripeRequestDto = new()
                {
                    ApprovedUrl = domain + "cart/Confirmation?orderId=" + orderHeaderDto.OrderHeaderId,
                    CancelUrl = domain + "cart/checkout",
                    OrderHeader = orderHeaderDto

                };

                var stripeResponse=await _orderService.CreateStripeSession(stripeRequestDto);
                StripeRequestDto stripeResponseResult= JsonConvert.DeserializeObject<StripeRequestDto>(Convert.ToString(stripeResponse.Result));

                Response.Headers.Add("Location", stripeResponseResult.StripeSessionUrl);

                return new StatusCodeResult(303);



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
