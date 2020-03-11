using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;

namespace WineLottery
{
    public static class WinebotContainer
    {
        private static readonly IAzure azure = Azure.Authenticate(new AzureCredentials(new MSILoginInformation(MSIResourceType.AppService), AzureEnvironment.AzureGlobalCloud, "99fe1041-ba57-4f49-866b-06c297c116cc")).WithSubscription("5659753a-501e-4e46-9aff-6120ed5694cf");

        public static void Start()
        {
            var container = azure.ContainerGroups.GetByResourceGroup("WineLottery", "ci-we-winebot");

            container?.RestartAsync();
        }

        public static void Stop()
        {
            var container = azure.ContainerGroups.GetByResourceGroup("WineLottery", "ci-we-winebot");

            container?.StopAsync();
        }
    }
}
