using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cloud.AzureAD.Authentication.Microsoft
{
    public class AzureBackendAPITokenService
    {
        public static async Task<string> GetAccesssToken(string userName, string password)
        {
            var httpClient = new HttpClient();
            var request = "https://login.microsoftonline.com/common/oauth2/token";
            var resource = $"resource=74658136-14ec-4630-ad9b-26e160ff0fc6&client_id=1950a258-227b-4e31-a9cf-717495945fc2&grant_type=password&username={userName}&password={password}";

            using (var stringContent = new StringContent(resource, Encoding.UTF8, "application/x-www-form-urlencoded"))
            {
                var response = await httpClient.PostAsync(request, stringContent).ContinueWith((response) =>
                {
                    return response.Result.Content.ReadAsStringAsync().Result;
                }).ConfigureAwait(false);

                var token = JsonSerializer.Deserialize<JsonElement>(response);
                var accessToken = $"{token.GetProperty("access_token")}";
                return accessToken;
            }
        }
    }
}
