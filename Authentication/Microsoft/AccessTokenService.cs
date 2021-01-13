using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Cloud.AzureAD.Authentication.MS
{
    public class AccessTokenService : IDisposable
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string tokenEndpoint = "https://login.microsoftonline.com/common/oauth2/token";
        private static readonly SemaphoreSlim semaphoreSlimTokens = new SemaphoreSlim(3);
        private AutoResetEvent tokenResetEvent = null;
        private readonly ConcurrentDictionary<string, string> tokenCache = new ConcurrentDictionary<string, string>();
        private bool disposedValue;

        public async Task<string> GetAccessTokenAsync(string userName, string password)
        {
            var accessToken = GetAccessToken(userName, tokenCache);
            if (accessToken == null)
            {
                await semaphoreSlimTokens.WaitAsync().ConfigureAwait(false);
                try
                {
                    accessToken = await AcquireTokenAsync(userName, password).ConfigureAwait(false);
                    CacheToken(userName, tokenCache, accessToken);

                    tokenResetEvent = new AutoResetEvent(false);
                    TokenWaitInfo wi = new TokenWaitInfo();
                    wi.Handle = ThreadPool.RegisterWaitForSingleObject(
                        tokenResetEvent,
                        async (state, timedOut) =>
                        {
                            if (!timedOut)
                            {
                                TokenWaitInfo wi = (TokenWaitInfo)state;
                                if (wi.Handle != null)
                                {
                                    wi.Handle.Unregister(null);
                                }
                            }
                            else
                            {
                                try
                                {
                                    // Take a lock to ensure no other threads are updating the Azure Access token at this time
                                    await semaphoreSlimTokens.WaitAsync().ConfigureAwait(false);
                                    RemoveCacheToken(userName, tokenCache);
                                }
                                catch (Exception)
                                {
                                    RemoveCacheToken(userName, tokenCache);
                                }
                                finally
                                {
                                    semaphoreSlimTokens.Release();
                                }
                            }
                        },
                        wi,
                        (uint)CalculateThreadSleep(accessToken).TotalMilliseconds,
                        true
                    );

                    return accessToken;

                }
                finally
                {
                    semaphoreSlimTokens.Release();
                }
            }
            else
            {
                return accessToken;
            }
        }

        private async Task<string> AcquireTokenAsync(string username, string password)
        {
            var requestUrl = $"resource=https://management.core.windows.net/&client_id=1950a258-227b-4e31-a9cf-717495945fc2&grant_type=password&username={username}&scope=openid&password={password}";
            using (var stringContent = new StringContent(requestUrl, Encoding.UTF8, "application/x-www-form-urlencoded"))
            {
                var response = await httpClient.PostAsync(tokenEndpoint, stringContent).ContinueWith((response) =>
                {
                    return response.Result.Content.ReadAsStringAsync().Result;
                }).ConfigureAwait(false);

                var tokenResult = JsonSerializer.Deserialize<JsonElement>(response);
                //var token = tokenResult.GetProperty("access_token").GetString();

                var refreshToken = tokenResult.GetProperty("refresh_token").GetString();
                string refreshTokenRequestUrl = $"grant_type=refresh_token&refresh_token={refreshToken}&resource=74658136-14ec-4630-ad9b-26e160ff0fc6";
                using (var content = new StringContent(refreshTokenRequestUrl, Encoding.UTF8, "application/x-www-form-urlencoded"))
                {
                    var refreshResponse = await httpClient.PostAsync(tokenEndpoint, content).ContinueWith((response) =>
                    {
                        return response.Result.Content.ReadAsStringAsync().Result;
                    }).ConfigureAwait(false);

                    var refreshTokenResult = JsonSerializer.Deserialize<JsonElement>(refreshResponse);
                    var token = refreshTokenResult.GetProperty("access_token").GetString();
                    return token;
                }
            }
        }

        private static string GetAccessToken(string userPrincipalName, ConcurrentDictionary<string, string> tokenCache)
        {
            if (tokenCache.TryGetValue(userPrincipalName, out string accessToken))
            {
                return accessToken;
            }

            return null;
        }

        private static void CacheToken(string userName, ConcurrentDictionary<string, string> tokenCache, string newAccessToken)
        {
            if (tokenCache.TryGetValue(userName, out string currentAccessToken))
            {
                tokenCache.TryUpdate(userName, newAccessToken, currentAccessToken);
            }
            else
            {
                tokenCache.TryAdd(userName, newAccessToken);
            }
        }

        private static void RemoveCacheToken(string userName, ConcurrentDictionary<string, string> tokenCache)
        {
            tokenCache.TryRemove(userName, out string currentAccessToken);
        }

        private static TimeSpan CalculateThreadSleep(string accessToken)
        {
            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(accessToken);
            var lease = GetAccessTokenLease(token.ValidTo);
            lease = TimeSpan.FromSeconds(lease.TotalSeconds - TimeSpan.FromMinutes(5).TotalSeconds > 0 ? lease.TotalSeconds - TimeSpan.FromMinutes(5).TotalSeconds : lease.TotalSeconds);
            return lease;
        }

        private static TimeSpan GetAccessTokenLease(DateTime expiresOn)
        {
            DateTime now = DateTime.UtcNow;
            DateTime expires = expiresOn.Kind == DateTimeKind.Utc ? expiresOn : TimeZoneInfo.ConvertTimeToUtc(expiresOn);
            TimeSpan lease = expires - now;
            return lease;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (tokenResetEvent != null)
                    {
                        tokenResetEvent.Set();
                        tokenResetEvent.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    internal class TokenWaitInfo
    {
        public RegisteredWaitHandle Handle = null;
    }
}
