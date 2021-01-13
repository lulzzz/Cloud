using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Cloud.AzureAD.Authentication.Graph
{
    public class TokenService
    {
        public static async Task<string> GetToken(string tenantId, string clientId, string clientSecret)
        {
            var client = new HttpClient();
            var requestUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
            var scope = $"scope=https://graph.microsoft.com/.default&grant_type=client_credentials&client_id={clientId}&client_secret={clientSecret}";
            var content = new StringContent(scope, Encoding.UTF8, "application/x-www-form-urlencoded");
            var httpResponse = await client.PostAsync(requestUrl, content);
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception($"get token failed, tenant id: {tenantId} client id: {clientId}, client secret:{clientSecret}");
            }
            string json = await httpResponse.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TokenModel>(json).access_token;
        }
    }
}
internal class TokenModel
{
    public string token_type { get; set; }
    public string scope { get; set; }
    public string expires_in { get; set; }
    public string ext_expires_in { get; set; }
    public string expires_on { get; set; }
    public string not_before { get; set; }
    public string resource { get; set; }
    public string access_token { get; set; }
    public string refresh_token { get; set; }
    public string foci { get; set; }
    public string id_token { get; set; }
}
