using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using System.Net.Http.Headers;
using static System.Net.WebRequestMethods;

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

        public async Task<ResponseDto?> EmailCart(CartDto cartDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Data = cartDto,
                Url = SD.ShoppingCartAPI + "/api/cart/EmailCartRequest"
            });
        }

        public async Task<ResponseDto?> GetCartByUserIdAsync(string userId)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.GET,
                Url = SD.ShoppingCartAPI + "/api/cart/GetCart/"+userId
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
           



            //HttpClient client = new HttpClient();

            //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7003/api/cart/AddCart");

            //request.Headers.Add("accept", "text/plain");

            //request.Content = new StringContent("{\n  \"cartHeader\": {\n    \"title\": \"string\",\n    \"cartHeaderId\": 0,\n    \"userId\": \"de471604-3f47-47e0-acb5-7e5efbd84972\",\n    \"couponId\": \"string\",\n    \"discount\": 0,\n    \"cartTotal\": 0,\n    \"firstName\": \"string\",\n    \"lastName\": \"string\",\n    \"phone\": \"string\",\n    \"email\": \"string\"\n  },\n  \"cartDetails\": [\n    {\n      \"id\": 0,\n      \"cartDetailsId\": 0,\n      \"cartHeaderId\": 0,\n      \"cartHeader\": {\n        \"title\": \"string\",\n        \"cartHeaderId\": 0,\n        \"userId\": \"string\",\n        \"couponId\": \"string\",\n        \"discount\": 0,\n        \"cartTotal\": 0,\n        \"firstName\": \"string\",\n        \"lastName\": \"string\",\n        \"phone\": \"string\",\n        \"email\": \"string\"\n      },\n      \"productId\": 3,\n      \"product\": {\n        \"productId\": 0,\n        \"name\": \"string\",\n        \"price\": 0,\n        \"description\": \"string\",\n        \"categoryName\": \"string\",\n        \"imageUrl\": \"string\"\n      },\n      \"count\": 0\n    }\n  ]\n}");
            //request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            //HttpResponseMessage response = await client.SendAsync(request);
            //response.EnsureSuccessStatusCode();
            //string responseBody = await response.Content.ReadAsStringAsync();

            var reqDto = new RequestDto();
            reqDto.ApiType = SD.ApiType.POST;
            reqDto.Data = cartDto;
            reqDto.Url = "https://localhost:7003/api/cart/AddCart";
            //   reqDto.Url = SD.ShoppingCartAPI + "/api/cart/AddCart";
            var res = await _baseService.SendAsync(reqDto);
            return res;
        }
    }
}
