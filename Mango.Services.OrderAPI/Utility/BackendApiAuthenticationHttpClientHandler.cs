﻿using Microsoft.AspNetCore.Authentication;

namespace Mango.Services.OrderAPI.Utility
{
    public class BackendApiAuthenticationHttpClientHandler:DelegatingHandler
    {
        private readonly HttpContextAccessor _contextAccessor;
        public BackendApiAuthenticationHttpClientHandler(HttpContextAccessor httpContextAccessor)
        {
            _contextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
        {
            var token = await _contextAccessor.HttpContext.GetTokenAsync("access_token");

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
