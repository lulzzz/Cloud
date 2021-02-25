using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Cloud.AzureAD.Authentication.AzureAdBackendToken;
using Newtonsoft.Json;
using System.Net.Http;

namespace Cloud.AzureAD.Intune
{
    public class ConfigurationProfile
    {
        public static async Task<IEnumerable<GroupPolicyConfiguration>> GetGroupPolicyConfigurations()
        {
            var graphClient = GraphClient.GetInstance("d6e01331-be4e-4114-86f1-09f2a9252679", "46514c3a-1b90-426d-949f-92e8be67da29", "sxw~0_yLYS6l1w~_ny5qf1Nr7-p2D4XGEE");

            var groupPolicyConfigurations = await graphClient.DeviceManagement.GroupPolicyConfigurations
                  .Request()
                  .Expand(p => p.Assignments)
                  .GetAsync();

            foreach (var policy in groupPolicyConfigurations)
            {
                var definitionValues = await graphClient.DeviceManagement.GroupPolicyConfigurations[policy.Id].DefinitionValues
                    .Request()
                    .Expand(d => d.Definition)
                    .GetAsync();
                policy.DefinitionValues = definitionValues;

                foreach (var definitionValue in definitionValues)
                {
                    var presentationValues = await graphClient.DeviceManagement.GroupPolicyConfigurations[policy.Id].DefinitionValues[definitionValue.Id]
                        .PresentationValues
                        .Request()
                        .GetAsync();
                    definitionValue.PresentationValues = presentationValues;

                    var presentations = await graphClient.DeviceManagement.GroupPolicyDefinitions[definitionValue.Definition.Id]
                        .Presentations
                        .Request()
                        .GetAsync();
                    definitionValue.Definition.Presentations = presentations;
                }
            }
            return groupPolicyConfigurations;
        }

        public static async Task CreateGroupPolicyConfigurations(IEnumerable<GroupPolicyConfiguration> policies)
        {
            var graphClient = GraphClient.GetInstance("a7223375-8d73-437d-a391-1c30f50afd49", "dc293766-b44d-48e6-bc3d-a14569148567", "JQXW_GIhdb3_74-h8U7e_ABFuR5u9vK937");
            //Delete destination group policy configuration
            //var groupPolicyConfigurations = await graphClient.DeviceManagement.GroupPolicyConfigurations
            //       .Request()
            //       .GetAsync();
            //foreach (var groupPolicyConfiguration in groupPolicyConfigurations)
            //{
            //    await graphClient.DeviceManagement.GroupPolicyConfigurations[groupPolicyConfiguration.Id]
            //        .Request()
            //        .DeleteAsync();
            //}

            foreach (var policy in policies)
            {
                await CreateGroupPolicyConfiguraion(graphClient, policy);
            }
        }

        private static async Task CreateGroupPolicyConfiguraion(GraphServiceClient graphClient, GroupPolicyConfiguration configuration)
        {
            try
            {
                var newConfiguration = await graphClient.DeviceManagement.GroupPolicyConfigurations
                    .Request()
                    .AddAsync(configuration);

                await graphClient.DeviceManagement.GroupPolicyConfigurations[newConfiguration.Id]
                    .Assign(configuration.Assignments)
                    .Request()
                    .PostAsync();
                foreach (var definitionValue in configuration.DefinitionValues)
                {
                    await graphClient.DeviceManagement.GroupPolicyConfigurations[newConfiguration.Id]
                      .UpdateDefinitionValues(new List<GroupPolicyDefinitionValue>()
                      {
                          new GroupPolicyDefinitionValue(){
                              ODataType = definitionValue.ODataType,
                          Enabled = definitionValue.Enabled,
                          ConfigurationType = definitionValue.ConfigurationType,
                           Definition = definitionValue.Definition,
                            PresentationValues = definitionValue.PresentationValues
                          }
                      }, new List<GroupPolicyDefinitionValue>(), new List<string>())
                      .Request()
                      .PostAsync();
                }
              
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        private async static Task UpdateByHttpClient(GroupPolicyConfiguration configuration, string newPolicId)
        {
            var token = await Authentication.Graph.TokenService.GetToken("dc293766-b44d-48e6-bc3d-a14569148567", "a7223375-8d73-437d-a391-1c30f50afd49", "JQXW_GIhdb3_74-h8U7e_ABFuR5u9vK937");
            var httpClient = HttpClientService.GetInstance(token);
            
            var definitionValues = new GroupDefinitionValue()
            {
                added = BuildDefinitionValues(configuration.DefinitionValues),
                deletedIds = new List<string>(),
                updated =new List<UpdatedDefinitionValue>()
            };

            //var requestUrl = $"https://graph.microsoft.com/beta/deviceManagement/groupPolicyConfigurations('{newPolicId}')/updateDefinitionValues";
            var requestUrl = "https://graph.microsoft.com/beta/deviceManagement/groupPolicyConfigurations('4f40a132-334c-4632-8af8-01df0eb35dc3')/updateDefinitionValues";
            var content = JsonConvert.SerializeObject(definitionValues, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            content = content.Replace("definitionodatabind", "definition@odata.bind", StringComparison.OrdinalIgnoreCase);
            content = System.IO.File.ReadAllText("C:\\configurationprofile.txt");
            var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(requestUrl, httpContent);
        }

        private static List<AddedDefinitionValue> BuildDefinitionValues(IEnumerable<GroupPolicyDefinitionValue> groupPolicyDefinitionValues)
        {
            var updateValues = new List<AddedDefinitionValue>();
            foreach (var groupPolicyDefinitionValue in groupPolicyDefinitionValues)
            {
                updateValues.Add(new AddedDefinitionValue()
                {
                    definitionodatabind = $"https://graph.microsoft.com/beta/deviceManagement/groupPolicyDefinitions('{groupPolicyDefinitionValue.Definition.Id}')",
                    enabled = groupPolicyDefinitionValue.Enabled,
                    presentationValues  = new List<PresentationValue>()
                });
            }
            return updateValues;

        }

        private static async Task UpdateGroupPolicyConfiguration(GraphServiceClient graphClient, string configurationId, DateTimeOffset? lastModifiedDateTime, GroupPolicyConfiguration configuration)
        {
            try
            {
                if (!lastModifiedDateTime.HasValue || lastModifiedDateTime < configuration.LastModifiedDateTime)
                {
                    await graphClient.DeviceManagement.GroupPolicyConfigurations[configurationId]
                        .Request()
                        .UpdateAsync(new GroupPolicyConfiguration()
                        {
                            DisplayName = configuration.DisplayName,
                            Description = configuration.Description,
                            RoleScopeTagIds = configuration.RoleScopeTagIds
                        });
                    if (configuration.Assignments != null)
                    {
                        await graphClient.DeviceManagement.GroupPolicyConfigurations[configurationId]
                           .Assign(configuration.Assignments)
                           .Request()
                           .PostAsync();
                    }
                    if (configuration.DefinitionValues != null)
                    {
                        await graphClient.DeviceManagement.GroupPolicyConfigurations[configurationId]
                            .UpdateDefinitionValues(null, configuration.DefinitionValues, null)
                            .Request()
                            .PostAsync();
                    }
                }
                else
                {
                }
            }
            catch (Exception ex)
            {

            }
        }

        public class GroupDefinitionValue
        {
            public List<AddedDefinitionValue> added { get; set; }
            public List<string> deletedIds { get; set; }
            public List<UpdatedDefinitionValue> updated { get; set; }
        }

            public class AddedDefinitionValue
        {
            public string definitionodatabind { get; set; }
            public bool? enabled { get; set; }
            public List<PresentationValue> presentationValues { get; set; }
        }
        public class UpdatedDefinitionValue : AddedDefinitionValue
        { 
           public string Id { get; set; }
        }

        public class PresentationValue
        {
            public string odatatype { get; set; }
            public string presentationodatabind { get; set; }
            public string value { get; set; }
        }
    }

}
