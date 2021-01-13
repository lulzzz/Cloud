using System;
using System.Collections.Generic;
using System.Text;

namespace Cloud.AzureAD.Authentication.MS
{
    public interface IHttpClientService
    {
        System.Net.Http.HttpClient GetInstance(string customerId, string tenantId);
    }
}
