using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud.AzureAD.Intune
{
    public class AppProtection
    {
        public async static Task<IEnumerable<IosManagedAppProtection>> GetIosManagedAppProtections()
        {
            var graphClient = GraphClient.GetInstance("d6e01331-be4e-4114-86f1-09f2a9252679", "46514c3a-1b90-426d-949f-92e8be67da29", "sxw~0_yLYS6l1w~_ny5qf1Nr7-p2D4XGEE");
            var iosProtections = await graphClient.DeviceAppManagement.IosManagedAppProtections
                  .Request()
                  .GetAsync();
            return iosProtections;
        }

        public async static Task UpdateIosManagedAppProtection(string protectionId, IosManagedAppProtection protection)
        {
            var graphClient = GraphClient.GetInstance("d6e01331-be4e-4114-86f1-09f2a9252679", "46514c3a-1b90-426d-949f-92e8be67da29", "sxw~0_yLYS6l1w~_ny5qf1Nr7-p2D4XGEE");
            var iosProtections = await graphClient.DeviceAppManagement.IosManagedAppProtections[protectionId]
                .Request()
                .UpdateAsync(protection);                    
        }
    }
}
