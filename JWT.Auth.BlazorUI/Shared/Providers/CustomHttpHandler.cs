using Blazored.LocalStorage;
using JWT.Auth.BlazorUI.ViewModels.Accounts;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace JWT.Auth.BlazorUI.Shared.Providers
{
    public class CustomHttpHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public CustomHttpHandler(ILocalStorageService localStorageService, IHttpClientFactory httpClientFactory, AuthenticationStateProvider authenticationStateProvider)
        {
            _localStorageService = localStorageService;
            _httpClientFactory = httpClientFactory;
            _authenticationStateProvider = authenticationStateProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri.AbsolutePath.ToLower().Contains("login") ||
                request.RequestUri.AbsolutePath.ToLower().Contains("register") ||
                request.RequestUri.AbsolutePath.ToLower().Contains("renew-tokens"))
            {
                return await base.SendAsync(request, cancellationToken);
            }

            var token = await _localStorageService.GetItemAsync<string>("jwt-access-token");

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            var originalResponse = await base.SendAsync(request, cancellationToken);

            if(originalResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                return await InvokeRefreshApiCall(originalResponse, request, token, cancellationToken);
            }
            return originalResponse;
        }

        private async Task<HttpResponseMessage> InvokeRefreshApiCall(
            HttpResponseMessage originalResponse,
            HttpRequestMessage originalRequest,
            string jwtToken,
            CancellationToken cancellationToken)
        {
            var refreshToken = await _localStorageService.GetItemAsync<string>("refresh-token");

            var userClaims = Utility.ParseClaimsFromJwt(jwtToken);

            var renewTokenRequest = new RenewTokenRequestVM()
            {
                RefreshToken = refreshToken,
                UserId = userClaims.ToList().Where(_ => _.Type == "Sub").Select(_ => Convert.ToInt32(_.Value)).FirstOrDefault()
            };

            var jsonPayload = JsonSerializer.Serialize(renewTokenRequest);
            var requestContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var httpClient = _httpClientFactory.CreateClient("Dot6Api");
            var refreshTokenResponse = await httpClient.PostAsync("api/User/renew-tokens", requestContent);

            if(refreshTokenResponse.StatusCode == HttpStatusCode.OK)
            {
                var regeneratedTokens = await refreshTokenResponse.Content.ReadFromJsonAsync<JwtTokenResponseVM>();
                await _localStorageService.SetItemAsync<string>("jwt-access-token", regeneratedTokens.AccessToken);
                await _localStorageService.SetItemAsync<string>("refresh-token", regeneratedTokens.RefreshToken);
                (_authenticationStateProvider as CustomAuthProvider).NotifyAuthState();

                originalRequest.Headers.Remove("Authorization");
                originalRequest.Headers.Add("Authorization", $"Bearer {regeneratedTokens.AccessToken}");

                return await base.SendAsync(originalRequest, cancellationToken);
            }

            return originalResponse;
        }


    }
}
