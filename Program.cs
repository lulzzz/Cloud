using AutoMapperDemo.Extension;
using Cloud.AzureAD.AutoMapper;
using Cloud.AzureAD.AzureAD;
using Cloud.AzureAD.Extension;
using Cloud.AzureAD.Intune;
using Microsoft.Graph;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cloud.AzureAD
{
    class Program
    {
        public static void Main()
        {
            //var policies = ConfigurationProfile.GetGroupPolicyConfigurations().Result;
            //ConfigurationProfile.CreateGroupPolicyConfigurations(policies).GetAwaiter().GetResult();

            //var policies = CompliancePolicy.GetDeviceCompliancePolicies().GetAwaiter().GetResult();
            //foreach (var policy in policies)
            //{
            //    CompliancePolicy.CreateDeviceCompliancePolicy(null, policy);
            //}

            //var users = AADUser.GetAADUsers().GetAwaiter().GetResult();
            //var targetUsers = users.MapListV2<User, UserYamlModel>();
            var service = new ConditionalAccessServiceV2();
           
            var policies  =service.GetConditionalAccessPolicies().GetAwaiter().GetResult();
            policies = policies.Where(p => p.policyName.Equals("devtest"));
            service.CreateConditionalAccessPolicies(policies).GetAwaiter().GetResult();
        }
    }
}
