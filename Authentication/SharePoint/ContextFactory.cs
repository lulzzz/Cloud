using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace Cloud.AzureAD.Authentication.SharePoint
{
    public class ContextFactory
    {
        public static ClientContext GetContext(string userName, string password, string clientId, string rootWebUrl)
        {
            using (var authenticationManager = new AuthenticationManager(clientId))
            {
                var context = authenticationManager.GetContext(new Uri(rootWebUrl), userName, password.ToSecureString());
                return context;
            }
        }

        public static ClientContext GetContext(string accessToken, string siteUrl)
        {
            var context = new ClientContext(new Uri(siteUrl));
            context.ExecutingWebRequest += (sender, e) =>
            {
                e.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + accessToken;
            };
            return context;
        }
    }

    public static class Extenstion
    {
        public static SecureString ToSecureString(this string value)
        {
            var secureValue = new SecureString();
            Array.ForEach(value.ToCharArray(), secureValue.AppendChar);
            return secureValue;
        }
    }
}
