using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;

namespace Mango.Web.Service
{
    public class CartService : ICartService
    {
        private readonly IBaseService _baseService;
        public CartService(IBaseService baseService)
        {
            _baseService = baseService;

        }
        public async Task<ResponseDto?> ApplyCouponsAsync(CartDto cartDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Data = cartDto,
                Url = SD.ShoppingCartAPI + "/api/cart/ApplyCoupon"
            });
        }

        public async Task<ResponseDto?> GetCartByUserIdAsync(string userId)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.GET,
                Url = SD.ShoppingCartAPI + "/api/cart/GetCart"+userId
            });
        }

        public async Task<ResponseDto?> RemoveFromCartAsync(int cartDetailsId)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Data = cartDetailsId,
                Url = SD.ShoppingCartAPI + "/api/cart/RemoveCart"
            });
        }

        public async Task<ResponseDto?> UpsertCartAsync(CartDto cartDto)
        {
            var reqDto = new RequestDto();
            reqDto.ApiType = SD.ApiType.POST;
            reqDto.Data = cartDto;
            reqDto.Url = SD.ShoppingCartAPI + "/api/cart/CartUpsert";
            var res = await _baseService.SendAsync(reqDto);
            //var res= await _baseService.SendAsync(new RequestDto()
            //{
            //    ApiType = SD.ApiType.POST,
            //    Data = cartDto,
            //    Url = SD.ShoppingCartAPI + "/api/cart/CartUpsert"
            //});
            return res;
        }
    }
}
