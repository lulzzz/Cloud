using Cloud.AzureAD.Authentication.AzureAdBackendToken;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Cloud.AzureAD.Intune
{
    public class AutomaticEnrollment
    {
        public static async Task<AutomaticEnrollmentModel> GetAutomaticEnrollment()
        {
            var graphClient = GraphClient.GetInstance("d6e01331-be4e-4114-86f1-09f2a9252679", "46514c3a-1b90-426d-949f-92e8be67da29", "sxw~0_yLYS6l1w~_ny5qf1Nr7-p2D4XGEE");
            var httpClient = HttpClientService.GetInstance("admin@M365x677264.onmicrosoft.com", "5kep7353bC");

            var servicePrincipals = await graphClient.ServicePrincipals.Request().Filter("appId eq '0000000a-0000-0000-c000-000000000000'").GetAsync();
            if (servicePrincipals.Any())
            {
                var intuneServiceId = servicePrincipals.First().Id;
                var response = await httpClient.GetAsync($"https://main.iam.ad.ext.azure.com/api/MdmApplications/{intuneServiceId}");
                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<AutomaticEnrollmentModel>(content);
                }
                else
                {
                    
                }
            }
            return null;
        }

        public static async Task UpdateAutomaticEnrollment(AutomaticEnrollmentModel enrollment)
        {
            var graphClient = GraphClient.GetInstance("d6e01331-be4e-4114-86f1-09f2a9252679", "46514c3a-1b90-426d-949f-92e8be67da29", "sxw~0_yLYS6l1w~_ny5qf1Nr7-p2D4XGEE");
            var httpClient = HttpClientService.GetInstance("admin@M365x677264.onmicrosoft.com", "5kep7353bC");

            var servicePrincipals = await graphClient.ServicePrincipals.Request().Filter("appId eq '0000000a-0000-0000-c000-000000000000'").GetAsync();
            if (servicePrincipals.Any())
            {
                var intuneServiceId = servicePrincipals.First().Id;
                var requestUrl = $"https://main.iam.ad.ext.azure.com/api/MdmApplications/{intuneServiceId}?mdmAppliesToChanged=true&mamAppliesToChanged=true";
                //https://main.iam.ad.ext.azure.com/api/MdmApplications/26438a56-a35d-4f21-9178-4865b27c1d75?mdmAppliesToChanged=true&mamAppliesToChanged=false
                var settings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Include };
                var content = JsonConvert.SerializeObject(enrollment, settings);
                var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                var response = await httpClient.PutAsync(requestUrl, httpContent);
                if (!response.IsSuccessStatusCode)
                {
                   
                }
            }
        }
    }
    public class AutomaticEnrollmentModel
    {
        public int mamAppliesTo { get; set; }
        public List<GroupData> mamAppliesToGroups { get; set; }
        public int mdmAppliesTo { get; set; }
        public List<GroupData> mdmAppliesToGroups { get; set; }
        public AppData appData { get; set; }
        public string appCategory { get; set; }
        public string appDisplayName { get; set; }
        public bool isOnPrem { get; set; }
        public string logoUrl { get; set; }
        public string objectId { get; set; }
        public OriginalAppData originalAppData { get; set; }
    }

    public class GroupData
    {
        public string displayName { get; set; }
        public string objectId { get; set; }

        //public bool dirSyncEnabled { get; set; }
        //public string groupTypes { get; set; }
        //public bool mailEnabled { get; set; }
        //public bool securityEnabled { get; set; }
    }

    public class AppData
    {
        public string complianceUrl { get; set; }
        public string enrollmentUrl { get; set; }
        public string termsOfUseUrl { get; set; }
        public string mamComplianceUrl { get; set; }
        public string mamEnrollmentUrl { get; set; }
        public string mamTermsOfUseUrl { get; set; }
    }

    public class OriginalAppData
    {
        public string complianceUrl { get; set; }
        public string enrollmentUrl { get; set; }
        public string mamComplianceUrl { get; set; }
        public string mamEnrollmentUrl { get; set; }
        public string mamTermsOfUseUrl { get; set; }
        public string termsOfUseUrl { get; set; }
    }
}
