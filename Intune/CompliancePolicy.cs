using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cloud.AzureAD.Intune
{
    public class CompliancePolicy
    {
        public async Task<IEnumerable<ManagementCondition>> GetManagementConditions()
        {
            var graphClient = GraphClient.GetInstance("d6e01331-be4e-4114-86f1-09f2a9252679", "46514c3a-1b90-426d-949f-92e8be67da29", "sxw~0_yLYS6l1w~_ny5qf1Nr7-p2D4XGEE");
            
            var conditions = await graphClient.DeviceManagement.ManagementConditions
              .Request()
              .GetAsync();
            return conditions;
        }

        public async Task<IEnumerable<ManagementConditionStatement>> GetManagementConditionStatements()
        {
            var graphClient = GraphClient.GetInstance("d6e01331-be4e-4114-86f1-09f2a9252679", "46514c3a-1b90-426d-949f-92e8be67da29", "sxw~0_yLYS6l1w~_ny5qf1Nr7-p2D4XGEE");

            var conditionStatements = await graphClient.DeviceManagement.ManagementConditionStatements
            .Request()
            .GetAsync();
            return conditionStatements;
        }

        public async void CreateManagementConditions(IEnumerable<ManagementCondition> conditions)
        {
            var graphClient = GraphClient.GetInstance("a7223375-8d73-437d-a391-1c30f50afd49", "dc293766-b44d-48e6-bc3d-a14569148567", "JQXW_GIhdb3_74-h8U7e_ABFuR5u9vK937");

            foreach (var condition in conditions)
            {
                try
                {
                    await graphClient.DeviceManagement.ManagementConditions
                        .Request()
                        .AddAsync(condition);
                }
                catch (Exception ex)
                {
                }
            }
        }

        public async void CreateManagementConditionStatements(IEnumerable<ManagementConditionStatement> conditionStatements)
        {
            var graphClient = GraphClient.GetInstance("a7223375-8d73-437d-a391-1c30f50afd49", "dc293766-b44d-48e6-bc3d-a14569148567", "JQXW_GIhdb3_74-h8U7e_ABFuR5u9vK937");

            foreach (var conditionStatement in conditionStatements)
            {
                await graphClient.DeviceManagement.ManagementConditionStatements
                    .Request()
                    .AddAsync(new ManagementConditionStatement()
                    {
                        DisplayName = conditionStatement.DisplayName,
                        Description = conditionStatement.Description,
                        ApplicablePlatforms = conditionStatement.ApplicablePlatforms,
                        ETag = conditionStatement.ETag,
                        ManagementConditions = conditionStatement.ManagementConditions,
                    });
            }
        }

        public static async Task<IEnumerable<DeviceCompliancePolicy>> GetDeviceCompliancePolicies()
        {
            var graphClient = GraphClient.GetInstance("d6e01331-be4e-4114-86f1-09f2a9252679", "46514c3a-1b90-426d-949f-92e8be67da29", "sxw~0_yLYS6l1w~_ny5qf1Nr7-p2D4XGEE");

            var policies = await graphClient.DeviceManagement.DeviceCompliancePolicies.Request().Expand(p => p.Assignments).GetAsync();
           
            return policies;
        }

        public static async Task CreateDeviceCompliancePolicy( DeviceCompliancePolicy compliancePolicy)
        {
            var graphClient = GraphClient.GetInstance("a7223375-8d73-437d-a391-1c30f50afd49", "dc293766-b44d-48e6-bc3d-a14569148567", "JQXW_GIhdb3_74-h8U7e_ABFuR5u9vK937");
            try
            {
                if (compliancePolicy.ODataType == "#microsoft.graph.androidCompliancePolicy")
                {
                    var androidCompliancePolicy = (AndroidCompliancePolicy)compliancePolicy;
                    androidCompliancePolicy.ConditionStatementId = null;
                    compliancePolicy = androidCompliancePolicy;
                }
            }
            catch (Exception ex)
            {
                
            }
        }

    }
}
