using My9GAG.NineGagApiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NineGagFilter.Client.State
{
    public class AuthState
    {
        private readonly IApiClient _apiClient;
        public AuthState(IApiClient apiClient)
        {

            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        public void FireAuthStateChangedEvent()
        {
            AuthStateChanged?.Invoke(this, new EventArgs());
        }

        public bool IsAuthenticated => _apiClient.AuthenticationInfo.IsAuthenticated;
        public event EventHandler<EventArgs> AuthStateChanged;
    }
}
