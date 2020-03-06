using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using My9GAG.NineGagApiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NineGagFilter.Client.Authentication
{
    public class NineGagAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IApiClient _apiClient;
        private readonly ILocalStorageService _localStorageService;

        public NineGagAuthenticationStateProvider(IApiClient apiClient, ILocalStorageService localStorageService)
        {
            this._apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            this._localStorageService = localStorageService ?? throw new ArgumentNullException(nameof(localStorageService));
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (!_apiClient.AuthenticationInfo.IsAuthenticated && await _localStorageService.ContainKeyAsync("9gag-auth-token"))
            {
                _apiClient.AuthenticationInfo.Token = await _localStorageService.GetItemAsync<string>("9gag-auth-token");
                _apiClient.AuthenticationInfo.TokenWillExpireAt = await _localStorageService.GetItemAsync<DateTime>("9gag-auth-token-expiration-date");
            }

            if (_apiClient.AuthenticationInfo.IsAuthenticated)
            {
                var identity = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, "Jane Doe"),
                }, "9gag");

                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }

            return new AuthenticationState(new ClaimsPrincipal());
        }

        public async Task LoginAsync(string username, string password)
        {
            await _apiClient.LoginWithCredentialsAsync(username, password);
            await _apiClient.LoginWithCredentialsAsync(username, password);
            await _localStorageService.SetItemAsync("9gag-auth-token", _apiClient.AuthenticationInfo.Token);
            await _localStorageService.SetItemAsync("9gag-auth-token-expiration-date", _apiClient.AuthenticationInfo.TokenWillExpireAt);
            NotifyAuthenticationStateChanged();

        }

        public async Task LogoutAsync()
        {
            await _localStorageService.RemoveItemAsync("9gag-auth-token");
            await _localStorageService.RemoveItemAsync("9gag-auth-token-expiration-date");
            _apiClient.AuthenticationInfo.ClearToken();
            NotifyAuthenticationStateChanged();
        }

        public void NotifyAuthenticationStateChanged()
        {
            this.NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
