using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Cloud.AzureAD.Authentication.MS
{
    public class HttpClientService :IHttpClientService
    {
        private static ILog _logger = LogManager.GetLogger(typeof(HttpClientService));
        public HttpClientService()
        {
           
        }

        public System.Net.Http.HttpClient GetInstance(string userName, string password)
        {
            var accessToken = GetAccessToken(userName, password).GetAwaiter().GetResult();

            var httpClient = new System.Net.Http.HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            httpClient.DefaultRequestHeaders.Add("x-ms-client-request-id", $"{new Guid()}");
            httpClient.DefaultRequestHeaders.Add("x-ms-correlation-id", $"{new Guid()}");
            return httpClient;
        }

        private async static Task<string> GetAccessToken(string userName, string password)
        {
            using (var tokenService = new AccessTokenService())
            {
                try
                {
                    var accesstoken = await tokenService.GetAccessTokenAsync(userName, password);
                    return accesstoken;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Get access token error, account: {userName}, exception: {ex.Message}.");
                }
            }
        }
    }
}
