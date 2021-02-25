using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cloud.AzureAD.Intune
{
    public class GraphClient
    {
        private static object obj = new object();
        private static GraphServiceClient graphClient;

        private GraphClient()
        {
            
        }

        public static GraphServiceClient GetInstance(string clientId, string tennatId, string clientSecret)
        {
            //if (graphClient == null)
            //{
            //    lock (obj)
            //    {
            //        if (graphClient == null)
            //        {
                        var confidentialClientApplication = ConfidentialClientApplicationBuilder
                            .Create(clientId)
                            .WithTenantId(tennatId)
                            .WithClientSecret(clientSecret)
                            .Build();

                        var authProvider = new ClientCredentialProvider(confidentialClientApplication);

                        graphClient = new GraphServiceClient(authProvider);
            //        }
            //    }
            //}
            return graphClient;
        }
    }
}
