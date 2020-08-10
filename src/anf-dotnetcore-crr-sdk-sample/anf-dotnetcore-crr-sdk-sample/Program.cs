
namespace Microsoft.Azure.Management.ANF.Samples
{
    using Microsoft.Azure.Management.ANF.Samples.Common;
    using Microsoft.Azure.Management.NetApp;
    using Microsoft.Azure.Management.NetApp.Models;
    using Microsoft.Azure.Management.ResourceManager;
    using Microsoft.Rest;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using static Microsoft.Azure.Management.ANF.Samples.Common.Utils;

    class Program
    {
        //------------------------------------------IMPORTANT------------------------------------------------------------------
        // Setting variables necessary for resources creation - change these to appropriated values related to your environment
        // Please NOTE: Resource Group and VNETs need to be created prior to run this code
        //----------------------------------------------------------------------------------------------------------------------

        // Subscription - Change SubId below
        //const string subscriptionId = "[Subscription ID here]";

        //// Primary ANF
        //const string primaryResourceGroupName = "[Primary Resource Group Name]";
        //const string primaryLocation = "[Primary Resources Location]";
        //const string primaryVNETName = "[Primary VNET Name]";
        //const string primarySubnetName = "[Primary SubNet Name]";
        //const string primaryAnfAccountName = "[Primary ANF Account name]";
        //const string primarycapacityPoolName = "[Primary ANF Capacity Pool name]";

        //// Secondary ANF
        //const string secondaryResourceGroupName = "[Secondary Resource Group Name]";
        //const string secondaryLocation = "[Secondary Resources Location]";
        //const string secondaryVNETName = "[Secondary VNET Name]";
        //const string secondarySubnetName = "[Secondary SubNet Name]";
        //const string secondaryAnfAccountName = "[Secondary ANF Account name]";
        //const string secondarycapacityPoolName = "[Secondary ANF Capacity Pool name]";

        ////ANF Properties
        //const string capacityPoolServiceLevel = "Standard";
        //const long capacitypoolSize = 4398046511104;  // 4TiB which is minimum size
        //const long volumeSize = 107374182400;  // 100GiB - volume minimum size

        const string subscriptionId = "0661b131-4a11-479b-96bf-2f95acca2f73";

        // Primary ANF
        const string primaryResourceGroupName = "adghabbotest-RG";
        const string primaryLocation = "westus";
        const string primaryVNETName = "adghabbotest-RG-vnet";
        const string primarySubnetName = "anf01-sn";
        const string primaryAnfAccountName = "primaryanfacc01";
        const string primarycapacityPoolName = "primarypool01";

        // Secondary ANF
        const string secondaryResourceGroupName = "adghabbotest2-rg";
        const string secondaryLocation = "eastus";
        const string secondaryVNETName = "adghabbotest2-rg-vnet";
        const string secondarySubnetName = "anf02-sn";
        const string secondaryAnfAccountName = "secondaryanfacc02";
        const string secondarycapacityPoolName = "secondarypool02";

        //ANF Properties
        const string capacityPoolServiceLevel = "Standard";
        const long capacitypoolSize = 4398046511104;  // 4TiB which is minimum size
        const long volumeSize = 107374182400;  // 100GiB - volume minimum size


        private static ServiceClientCredentials Credentials { get; set; }

        /// <summary>
        /// Sample console application that creates an ANF Account, Capacity Pool and a Volume enable with NFS 4.1 protocol
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            DisplayConsoleAppHeader();
            
            try
            {
                //Utils.WriteConsoleMessage("Start creating Primary ANF...");
                CreateANFCRRAsync().GetAwaiter().GetResult();
                Utils.WriteConsoleMessage("Sample application successfuly completed execution.");
            }
            catch (Exception ex)
            {
                WriteErrorMessage(ex.Message);
            }
        }

        static private async Task CreateANFCRRAsync()
        {
            //----------------------------------------------------------------------------------------
            // Authenticating using service principal, refer to README.md file for requirement details
            //----------------------------------------------------------------------------------------
            WriteConsoleMessage("Authenticating...");
            Credentials = await ServicePrincipalAuth.GetServicePrincipalCredential("AZURE_AUTH_LOCATION");

            //------------------------------------------
            // Instantiating a new ANF management client
            //------------------------------------------
            WriteConsoleMessage("Instantiating a new Azure NetApp Files management client...");
            AzureNetAppFilesManagementClient anfClient = new AzureNetAppFilesManagementClient(Credentials)
            {
                SubscriptionId = subscriptionId
            };
            WriteConsoleMessage($"\tApi Version: {anfClient.ApiVersion}");

            //----------------------
            // Creating ANF Primary Account
            //----------------------

            // Setting up Primary NetApp Files account body  object
            NetAppAccount anfPrimaryAccountBody = new NetAppAccount(primaryLocation, null, primaryAnfAccountName);
            WriteConsoleMessage($"Requesting Primary account to be created in {primaryLocation}");
            var anfPrimaryAccount = await anfClient.Accounts.CreateOrUpdateAsync(anfPrimaryAccountBody, primaryResourceGroupName, primaryAnfAccountName);
            WriteConsoleMessage($"\tAccount Resource Id: {anfPrimaryAccount.Id}");


            // Setting up capacity pool body object for Primary Account
            CapacityPool primaryCapacityPoolBody = new CapacityPool()
            {
                Location = primaryLocation.ToLower(), // Important: location needs to be lower case
                ServiceLevel = capacityPoolServiceLevel,
                Size = capacitypoolSize
            };
            WriteConsoleMessage("Requesting capacity pool to be created for Primary Account");
            var primaryCapacityPool = await anfClient.Pools.CreateOrUpdateAsync(primaryCapacityPoolBody, primaryResourceGroupName, anfPrimaryAccount.Name, primarycapacityPoolName);
            WriteConsoleMessage($"\tCapacity Pool Resource Id: {primaryCapacityPool.Id}");


            // Creating export policy object
            VolumePropertiesExportPolicy exportPolicies = new VolumePropertiesExportPolicy()
            {
                Rules = new List<ExportPolicyRule>
                {
                    new ExportPolicyRule() {
                        AllowedClients = "0.0.0.0",
                        Cifs = false,
                        Nfsv3 = false,
                        Nfsv41 = true,
                        RuleIndex = 1,
                        UnixReadOnly = false,
                        UnixReadWrite = true
                    }
                }
            };

            // Creating primary volume body object
            string primarySubnetId = $"/subscriptions/{subscriptionId}/resourceGroups/{primaryResourceGroupName}/providers/Microsoft.Network/virtualNetworks/{primaryVNETName}/subnets/{primarySubnetName}";
            string primaryVolumeName = $"PrimaryVol01";
            //string primaryVolumeName = $"PrimaryVol01-{primaryAnfAccountName}-{primarycapacityPoolName}";

            Volume primaryVolumeBody = new Volume()
            {
                ExportPolicy = exportPolicies,
                Location = primaryLocation.ToLower(),
                ServiceLevel = capacityPoolServiceLevel,
                CreationToken = primaryVolumeName,
                SubnetId = primarySubnetId,
                UsageThreshold = volumeSize,
                ProtocolTypes = new List<string>() { "NFSv4.1" }
            };

            // Creating NFS 4.1 volume
            WriteConsoleMessage($"Requesting volume to be created in {primarycapacityPoolName}");
            var primaryVolume = await anfClient.Volumes.CreateOrUpdateAsync(primaryVolumeBody, primaryResourceGroupName, primaryAnfAccountName, ResourceUriUtils.GetAnfCapacityPool(primaryCapacityPool.Id), primaryVolumeName);
            WriteConsoleMessage($"\tVolume Resource Id: {primaryVolume.Id}");

            WriteConsoleMessage($"Waiting for {primaryVolume.Id} to be available...");
            await ResourceUriUtils.WaitForAnfResource<Volume>(anfClient, primaryVolume.Id);


            //----------------------
            // Creating ANF Secondary Account
            //----------------------

            // Setting up Secondary NetApp Files account body  object
            NetAppAccount anfSecondaryAccountBody = new NetAppAccount(secondaryLocation, null, secondaryAnfAccountName);
            WriteConsoleMessage($"Requesting Secondary account to be created in {secondaryLocation}");
            var anfSecondaryAccount = await anfClient.Accounts.CreateOrUpdateAsync(anfSecondaryAccountBody, secondaryResourceGroupName, secondaryAnfAccountName);
            WriteConsoleMessage($"\tAccount Resource Id: {anfSecondaryAccount.Id}");

            // Setting up capacity pool body object for Secondary Account
            CapacityPool secondaryCapacityPoolBody = new CapacityPool()
            {
                Location = secondaryLocation.ToLower(), // Important: location needs to be lower case
                ServiceLevel = capacityPoolServiceLevel,
                Size = capacitypoolSize
            };
            WriteConsoleMessage("Requesting capacity pool to be created for Secondary Account");
            var secondaryCapacityPool = await anfClient.Pools.CreateOrUpdateAsync(secondaryCapacityPoolBody, secondaryResourceGroupName, anfSecondaryAccount.Name, secondarycapacityPoolName);
            WriteConsoleMessage($"\tCapacity Pool Resource Id: {secondaryCapacityPool.Id}");

            // Creating secondary volume body object
            string secondarySubnetId = $"/subscriptions/{subscriptionId}/resourceGroups/{secondaryResourceGroupName}/providers/Microsoft.Network/virtualNetworks/{secondaryVNETName}/subnets/{secondarySubnetName}";
            //string secondaryVolumeName = $"Vol-{secondaryAnfAccountName}-{secondarycapacityPoolName}";
            string secondaryVolumeName = $"SecondaryVol02";

            Volume secondaryVolumeBody = new Volume()
            {
                ExportPolicy = exportPolicies,
                Location = secondaryLocation.ToLower(),
                ServiceLevel = capacityPoolServiceLevel,
                CreationToken = secondaryVolumeName,
                SubnetId = secondarySubnetId,
                UsageThreshold = volumeSize,
                ProtocolTypes = new List<string>() { "NFSv4.1" },
                DataProtection = new VolumePropertiesDataProtection()
                {
                    Replication = new ReplicationObject()
                    {
                        EndpointType = "dst",
                        RemoteVolumeRegion = primaryLocation,
                        RemoteVolumeResourceId = primaryVolume.Id,
                        ReplicationSchedule = "hourly"
                    }
                }
            };


            //-------------------------------------------------------------
            // Creating Data Replication Volume on the Destination Account
            //-------------------------------------------------------------

            // Creating NFS 4.1 Data Replication Volume
            WriteConsoleMessage("Adding Data Replication in Destination region...");
            var dataReplicationVolume = await anfClient.Volumes.CreateOrUpdateAsync(secondaryVolumeBody, secondaryResourceGroupName, anfSecondaryAccount.Name, ResourceUriUtils.GetAnfCapacityPool(secondaryCapacityPool.Id), secondaryVolumeName);
            await ResourceUriUtils.WaitForAnfResource<Volume>(anfClient, dataReplicationVolume.Id);

            WriteConsoleMessage($"Waiting for {dataReplicationVolume.Id} to be available...");
            await ResourceUriUtils.WaitForAnfResource<Volume>(anfClient, dataReplicationVolume.Id);

            //--------------------------
            // Authorizing Source volume
            //--------------------------
            AuthorizeRequest authRequest = new AuthorizeRequest()
            {
                RemoteVolumeResourceId = dataReplicationVolume.Id
            };
            WriteConsoleMessage("Authorizing replication in Source region...");
            await anfClient.Volumes.AuthorizeReplicationAsync(primaryResourceGroupName, primaryAnfAccountName, ResourceUriUtils.GetAnfCapacityPool(primaryCapacityPool.Id), primaryVolumeName, authRequest);

            WriteConsoleMessage("ANF Cross-Region Replication has completed successfully");

        }
    }
}
