using Cloud.AzureAD.Intune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud.AzureAD.AzureAD
{
    public class AADGroup
    {
        public static async Task GetAADGroups()
        {
            var graphClient = GraphClient.GetInstance("d6e01331-be4e-4114-86f1-09f2a9252679", "46514c3a-1b90-426d-949f-92e8be67da29", "sxw~0_yLYS6l1w~_ny5qf1Nr7-p2D4XGEE");

            var groups = await graphClient.Groups
               .Request()
               .Expand(g => g.Owners)
               .Select(g => new
               {
                   g.Id,
                   g.GroupTypes,
                   g.DisplayName,
                   g.Description,
                   g.MailEnabled,
                   g.Mail,
                   g.MailNickname,
                   g.SecurityEnabled,
                   g.Visibility,
                   g.AssignedLicenses,
                   g.IsAssignableToRole,
                    //g.MembershipRule,
                    //g.MembershipRuleProcessingState
                })
               .GetAsync();

            var fitlerGroups = groups.Where(g => !g.GroupTypes.Any(gt => gt.Equals("DynamicMembership", StringComparison.OrdinalIgnoreCase))).Where(g => !g.MailEnabled.Value);
            //var filterGroups = groups.Where(g => g.DisplayName.Equals("test Members") || g.DisplayName.Equals("Test_IncludeGroup"));
            foreach (var group in fitlerGroups)
            {
                Console.WriteLine($"display name: {group.DisplayName}, mail enable: {group.MailEnabled}, group type: {string.Join(",",group.GroupTypes)})");
            }

        }
    }
}
