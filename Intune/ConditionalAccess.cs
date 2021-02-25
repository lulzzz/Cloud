using Cloud.AzureAD.Authentication.AzureAdBackendToken;
using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Cloud.AzureAD.Intune
{
    public class ConditionalAccessServiceV2
    {
        private string POLICYREQUESTURL = "https://main.iam.ad.ext.azure.com/api/Policies/Policies?top=10&nextLink={0}&appId=&includeBaseline=true";
        private List<string> policyIds { get; set; }

        public ConditionalAccessServiceV2()
        {
            policyIds = new List<string>();
        }

        public async Task<IEnumerable<ConditionalAccessPolicyModel>> GetConditionalAccessPolicies(GraphServiceClient graphClient=null, HttpClient httpClient = null)
        {
            if (graphClient == null)
            {
                graphClient = GraphClient.GetInstance("d6e01331-be4e-4114-86f1-09f2a9252679", "46514c3a-1b90-426d-949f-92e8be67da29", "sxw~0_yLYS6l1w~_ny5qf1Nr7-p2D4XGEE");
            }
            if (httpClient == null)
            {
                httpClient = HttpClientService.GetInstance("admin@M365x677264.onmicrosoft.com", "5kep7353bC");
            }
            var conditionalAccessPolicies = new List<ConditionalAccessPolicyModel>();
            await GetConditionalAccessPolicyIds(httpClient, null);
            if (policyIds.Any())
            {
                var locations = await graphClient.Identity.ConditionalAccess.NamedLocations.Request().GetAsync();
                foreach (var policyId in policyIds.Distinct())
                {
                    var response = httpClient.GetAsync($"https://main.iam.ad.ext.azure.com/api/Policies/{policyId}").GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        var policy = JsonConvert.DeserializeObject<ConditionalAccessPolicyModel>(response.Content.ReadAsStringAsync().Result);
                        if (policy.conditions.namedNetworks.applyCondition)
                        {
                            if (policy.conditions.namedNetworks.includedNetworkIds.Any())
                            {
                                var locationNames = locations.Where(l => policy.conditions.namedNetworks.includedNetworkIds.Contains(l.Id)).Select(l => l.DisplayName).ToList();
                                policy.conditions.namedNetworks.includedNetworkIds.Clear();
                                policy.conditions.namedNetworks.includedNetworkIds.AddRange(locationNames);
                            }
                            if (policy.conditions.namedNetworks.excludedNetworkIds.Any())
                            {
                                var locationNames = locations.Where(l => policy.conditions.namedNetworks.excludedNetworkIds.Contains(l.Id)).Select(l => l.DisplayName).ToList();
                                policy.conditions.namedNetworks.excludedNetworkIds.Clear();
                                policy.conditions.namedNetworks.excludedNetworkIds.AddRange(locationNames);
                            }
                        }
                        conditionalAccessPolicies.Add(policy);
                    }
                    else
                    {
                        
                    }
                }
            }

            return conditionalAccessPolicies;
        }

        public  async Task CreateConditionalAccessPolicies(IEnumerable<ConditionalAccessPolicyModel> policies)
        {

            var graphClient = GraphClient.GetInstance("a7223375-8d73-437d-a391-1c30f50afd49", "dc293766-b44d-48e6-bc3d-a14569148567", "JQXW_GIhdb3_74-h8U7e_ABFuR5u9vK937");
            var httpClient = HttpClientService.GetInstance("admin@M365x623643.onmicrosoft.com", "OsiN20mhqA");

            //var graphClient = GraphClient.GetInstance("d6e01331-be4e-4114-86f1-09f2a9252679", "46514c3a-1b90-426d-949f-92e8be67da29", "sxw~0_yLYS6l1w~_ny5qf1Nr7-p2D4XGEE");
            //var httpClient = HttpClientService.GetInstance("admin@M365x677264.onmicrosoft.com", "5kep7353bC");
            var locations = await graphClient.Identity.ConditionalAccess.NamedLocations.Request().GetAsync();

            var targetPolicies = await GetConditionalAccessPolicies(graphClient, httpClient);
            foreach (var policy in policies)
            {
                try
                {
                    var targetPolicy = targetPolicies.FirstOrDefault(p => p.policyName.Equals(policy.policyName));
                    if (targetPolicy != null)
                    {
                        await UpdateConditionalAccessPolicy(httpClient, targetPolicy.policyId, policy, locations);
                    }
                    else
                    {
                        await CreateConditionalAccessPolicy(graphClient, httpClient, policy, locations);
                    }
                }
                catch (Exception ex)
                {
                  
                }
            }
        }

        public  async Task UpdateConditionalAccessPolicy(string policyId, ConditionalAccessPolicyModel policy)
        {
            var graphClient = GraphClient.GetInstance("a7223375-8d73-437d-a391-1c30f50afd49", "dc293766-b44d-48e6-bc3d-a14569148567", "JQXW_GIhdb3_74-h8U7e_ABFuR5u9vK937");
            var httpClient = HttpClientService.GetInstance("admin@M365x623643.onmicrosoft.com", "OsiN20mhqA");
            var locations = await graphClient.Identity.ConditionalAccess.NamedLocations.Request().GetAsync();
            await UpdateConditionalAccessPolicy(httpClient, policyId, policy, locations);
        }

        private  async Task CreateConditionalAccessPolicy(GraphServiceClient graphClient, HttpClient httpClient, ConditionalAccessPolicyModel policy, IEnumerable<NamedLocation> locations)
        {
            try
            {
                policy = BuildConditionlAccessPolicy(policy, locations);
                var content = JsonConvert.SerializeObject(policy, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("https://main.iam.ad.ext.azure.com/api/Policies", httpContent);
                if (response.IsSuccessStatusCode)
                {
                    
                }
                else
                {
                   
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        private  async Task UpdateConditionalAccessPolicy(HttpClient httpClient, string policyId, ConditionalAccessPolicyModel policy, IEnumerable<NamedLocation> locations)
        {
            policy = BuildConditionlAccessPolicy(policy, locations);

            var content = JsonConvert.SerializeObject(policy, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Include });
            var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
            var validResponse = await httpClient.PostAsync($"https://main.iam.ad.ext.azure.com/api/Policies/Validate", httpContent);
            var response = await httpClient.PutAsync($"https://main.iam.ad.ext.azure.com/api/Policies/{policyId}", httpContent);
            if (response.IsSuccessStatusCode)
            {
               
            }
            else
            {
               
            }
        }

        private  async Task GetConditionalAccessPolicyIds(HttpClient httpClient, string nextLink = null)
        {
            var response = await httpClient.GetAsync(string.IsNullOrEmpty(nextLink) ? string.Format(POLICYREQUESTURL, "null") : string.Format(POLICYREQUESTURL, nextLink));
            if (response.IsSuccessStatusCode)
            {
                var policy = JsonConvert.DeserializeObject<ConditionalAccessPolicyBaseModel>(await response.Content.ReadAsStringAsync());
                policyIds.AddRange(policy.items.Select(i => i.policyId));
                if (!string.IsNullOrEmpty(policy.nextLink))
                {
                    await GetConditionalAccessPolicyIds(httpClient, policy.nextLink);
                }
            }
            else
            {
               
            }
        }

        private  ConditionalAccessPolicyModel BuildConditionlAccessPolicy(ConditionalAccessPolicyModel policy, IEnumerable<NamedLocation> locations)
        {
            if (policy.users.allUsers > 1 || policy.usersV2.allUsers > 1)
            {
                policy.isUsersGroupsV2Enabled = true;
            }
            else
            {
                policy.users = null;
                policy.usersV2 = null;
            }

            //Must to set isAllProtocolsEnabled = true for create and update operation
            if (policy.conditions.clientApps.applyCondition || policy.conditions.clientAppsV2.applyCondition)
            {
                policy.isAllProtocolsEnabled = true;
            }            

            if (policy.conditions.namedNetworks.applyCondition)
            {
                if (policy.conditions.namedNetworks.includedNetworkIds.Any())
                {
                    var locationIds = locations.Where(l => policy.conditions.namedNetworks.includedNetworkIds.Contains(l.DisplayName)).Select(l => l.Id).ToList();
                    policy.conditions.namedNetworks.includedNetworkIds.Clear();
                    policy.conditions.namedNetworks.includedNetworkIds.AddRange(locationIds);
                }
                if (policy.conditions.namedNetworks.excludedNetworkIds.Any())
                {
                    var locationIds = locations.Where(l => policy.conditions.namedNetworks.excludedNetworkIds.Contains(l.DisplayName)).Select(l => l.Id).ToList();
                    policy.conditions.namedNetworks.excludedNetworkIds.Clear();
                    policy.conditions.namedNetworks.excludedNetworkIds.AddRange(locationIds);
                }
            }
            return policy;
        }
    }

    public class ConditionalAccessPolicyBaseModel
    {
        public List<ConditionalAccessPolicyItem> items { get; set; }
        public string nextLink { get; set; }
        public int totalCount { get; set; }
    }

    public class ConditionalAccessPolicyItem
    {
        public bool applyRule { get; set; }
        public int baselineType { get; set; }
        public string createdDateTime { get; set; }
        public string modifiedDateTime { get; set; }
        public string policyId { get; set; }
        public string policyName { get; set; }
        public int policyState { get; set; }
        public bool usePolicyState { get; set; }
    }

    public class ConditionalAccessPolicyModel
    {
        public string Id { get; set; }
        public Conditions conditions { get; set; }
        public Controls controls { get; set; }
        public string createdDateTime { get; set; }
        public bool isAllProtocolsEnabled { get; set; }
        public bool isCloudAppsV2Enabled { get; set; }
        public bool isUsersGroupsV2Enabled { get; set; }
        public string modifiedDateTime { get; set; }
        public string policyId { get; set; }
        public string policyName { get; set; }
        public int policyState { get; set; }
        public ServicePrincipals servicePrincipals { get; set; }
        public ServicePrincipalsV2 servicePrincipalsV2 { get; set; }
        public SessionControls sessionControls { get; set; }
        public bool usePolicyState { get; set; }
        public Users users { get; set; }
        public UsersV2 usersV2 { get; set; }
        public int version { get; set; }
    }
    public class Conditions
    {
        public ClientApps clientApps { get; set; }
        public ClientAppsV2 clientAppsV2 { get; set; }
        public DevicePlatforms devicePlatforms { get; set; }
        public DeviceState deviceState { get; set; }
        public Locations locations { get; set; }
        public MinSigninRisk minSigninRisk { get; set; }
        public MinUserRisk minUserRisk { get; set; }
        public NamedNetworks namedNetworks { get; set; }
        public Time time { get; set; }
    }
    public class ClientApps
    {
        public bool applyCondition { get; set; }
        public bool exchangeActiveSync { get; set; }
        public bool mobileDesktop { get; set; }
        public bool onlyAllowSupportedPlatforms { get; set; }
        public bool specificClientApps { get; set; }
        public bool webBrowsers { get; set; }
    }
    public class ClientAppsV2
    {
        public bool applyCondition { get; set; }
        public bool exchangeActiveSync { get; set; }
        public bool mobileDesktop { get; set; }
        public bool modernAuth { get; set; }
        public bool onlyAllowSupportedPlatforms { get; set; }
        public bool otherClients { get; set; }
        public bool webBrowsers { get; set; }
    }
    public class DevicePlatforms
    {
        public int all { get; set; }
        public bool applyCondition { get; set; }
        public ExcludedAndIncluded excluded { get; set; }
        public ExcludedAndIncluded included { get; set; }

    }
    public class ExcludedAndIncluded
    {
        public bool android { get; set; }
        public bool ios { get; set; }
        public bool macOs { get; set; }
        public bool windows { get; set; }
        public bool windowsPhone { get; set; }
    }
    public class DeviceState
    {
        public bool applyCondition { get; set; }
        public bool excludeCompliantDevice { get; set; }
        public bool excludeDomainJoionedDevice { get; set; }
        //type ???
        public string filter { get; set; }
        public int includeDeviceStateType { get; set; }
    }
    public class Locations
    {
        public bool applyCondition { get; set; }
        public bool excludeAllTrusted { get; set; }
        public int includeLocationType { get; set; }
    }
    public class MinSigninRisk
    {
        public bool applyCondition { get; set; }
        public bool highRisk { get; set; }
        public bool lowRisk { get; set; }
        public bool mediumRisk { get; set; }
        public bool noRisk { get; set; }
    }
    public class MinUserRisk
    {
        public bool applyCondition { get; set; }
        public bool highRisk { get; set; }
        public bool lowRisk { get; set; }
        public bool mediumRisk { get; set; }
    }
    public class NamedNetworks
    {
        public bool applyCondition { get; set; }
        public bool excludeCorpnet { get; set; }
        public int excludeLocationType { get; set; }
        public bool excludeTrustedIps { get; set; }
        public List<string> excludedNetworkIds { get; set; }
        public bool includeCorpnet { get; set; }
        public int includeLocationType { get; set; }
        public bool includeTrustedIps { get; set; }
        public List<string> includedNetworkIds { get; set; }
    }
    public class Time
    {
        public int all { get; set; }
        public bool applyCondition { get; set; }
        public TimeExcludedIncluded excluded { get; set; }
        public TimeExcludedIncluded included { get; set; }

    }
    public class TimeExcludedIncluded
    {
        public DateRange dateRange { get; set; }
        public DaysOfWeek daysOfWeek { get; set; }
        public bool isExcludeSet { get; set; }
        //type???
        public string timezoneId { get; set; }
        public int type { get; set; }
    }
    public class DateRange
    {
        public string endDateTime { get; set; }
        public string startDateTime { get; set; }
    }
    public class DaysOfWeek
    {
        public bool allDay { get; set; }
        public List<int> day { get; set; }
        public string endTime { get; set; }
        public string startTime { get; set; }
    }
    public class Controls
    {
        public bool approvedClientApp { get; set; }
        public bool blockAccess { get; set; }
        public bool challengeWithMfa { get; set; }
        public List<string> claimProviderControlIds { get; set; }
        public bool compliantDevice { get; set; }
        public bool controlsOr { get; set; }
        public bool domainJoinedDevice { get; set; }
        public bool requireCompliantApp { get; set; }
        public bool requirePasswordChange { get; set; }
        public int requiredFederatedAuthMethod { get; set; }
    }
    public class ServicePrincipals
    {
        public int allServicePrincipals { get; set; }
        public bool excludeAllMicrosoftApps { get; set; }
        public ServicePrincipalExcludedIncluded excluded { get; set; }
        public ServicePrincipalExcludedIncluded included { get; set; }
        public bool includeAllMicrosoftApps { get; set; }
        public List<int> userActions { get; set; }
    }
    public class ServicePrincipalExcludedIncluded
    {
        public List<string> ids { get; set; }
    }
    public class ServicePrincipalsV2
    {
        public int allServicePrincipals { get; set; }
        public ServicePrincipalExcludedIncluded excluded { get; set; }
        public ServicePrincipalExcludedIncluded included { get; set; }
        public string includedAppContext { get; set; }
        public bool shouldIncludeAppContext { get; set; }
    }
    public class SessionControls
    {
        public bool appEnforced { get; set; }
        public bool cas { get; set; }
        public int cloudAppSecuritySessionControlType { get; set; }
        public int persistentBrowserSessionMode { get; set; }
        public int signInFrequency { get; set; }
        public SignInFrequencyTimeSpan signInFrequencyTimeSpan { get; set; }
    }
    public class SignInFrequencyTimeSpan
    {
        public int type { get; set; }
        public int value { get; set; }
    }
    public class Users
    {
        public int allUsers { get; set; }
        public UsersExcludedIncluded excluded { get; set; }
        public UsersExcludedIncluded included { get; set; }

    }
    public class UsersExcludedIncluded
    {
        public List<string> groupIds { get; set; }
        public List<string> userIds { get; set; }
    }
    public class UsersV2
    {
        public int allUsers { get; set; }
        public UserV2ExcludedIncluded excluded { get; set; }
        public UserV2ExcludedIncluded included { get; set; }
    }
    public class UserV2ExcludedIncluded
    {
        public bool allGuestUsers { get; set; }

        public List<string> groupIds { get; set; }
        public List<string> roleIds { get; set; }
        public bool roles { get; set; }
        public List<string> userIds { get; set; }
        public bool usersGroups { get; set; }
    }
}
