// Copyright (c) Microsoft and contributors.  All rights reserved.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace Microsoft.Azure.Management.ANF.Samples.Common
{
    //using Azure.Core.Diagnostics;
    using global::Azure.Core.Diagnostics;
    using Microsoft.Azure.Management.NetApp;
    using Microsoft.Azure.Management.NetApp.Models;
    using Microsoft.Azure.Management.ResourceManager.Models;
    using System;
    using System.Diagnostics.Tracing;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    /// <summary>
    /// Contains public methods to get configuration settigns, to initiate authentication, output error results, etc.
    /// </summary>
    public static class ResourceUriUtils
    {
        /// <summary>
        /// Gets volume Id from resource uri
        /// </summary>
        /// <param name="resourceUri"></param>
        /// <returns></returns>
        public static string GetVolumeId(string resourceUri)
        {
            if (string.IsNullOrWhiteSpace(resourceUri))
            {
                return null;
            }

            return resourceUri.Substring(0, resourceUri.IndexOf("/snapshots") - 1);
        }

        /// <summary>
        /// Gets ANF Account name from resource uri
        /// </summary>
        /// <param name="resourceUri"></param>
        /// <returns></returns>
        public static string GetAnfAccount(string resourceUri)
        {
            if (string.IsNullOrWhiteSpace(resourceUri))
            {
                return null;
            }

            return GetResourceValue(resourceUri, "/netAppAccounts");
        }

        /// <summary>
        /// Gets ANF Capacity pool name from resource uri
        /// </summary>
        /// <param name="resourceUri"></param>
        /// <returns></returns>
        public static string GetAnfCapacityPool(string resourceUri)
        {
            if (string.IsNullOrWhiteSpace(resourceUri))
            {
                return null;
            }

            return GetResourceValue(resourceUri, "/capacityPools");
        }

        /// <summary>
        /// Gets ANF Volume name from resource uri
        /// </summary>
        /// <param name="resourceUri"></param>
        /// <returns></returns>
        public static string GetAnfVolume(string resourceUri)
        {
            if (string.IsNullOrWhiteSpace(resourceUri))
            {
                return null;
            }

            return GetResourceValue(resourceUri, "/volumes");
        }

        /// <summary>
        /// Gets ANF Snapshot name from resource uri
        /// </summary>
        /// <param name="resourceUri"></param>
        /// <returns></returns>
        public static string GetAnfSnapshot(string resourceUri)
        {
            if (string.IsNullOrWhiteSpace(resourceUri))
            {
                return null;
            }

            return GetResourceValue(resourceUri, "/snapshots");
        }

        /// <summary>
        /// Gets subsctipion guid from resource uri
        /// </summary>
        /// <param name="resourceUri"></param>
        /// <returns></returns>
        public static string GetSubscriptionFromUri(string resourceUri)
        {
            if (string.IsNullOrWhiteSpace(resourceUri))
            {
                return null;
            }

            return GetResourceValue(resourceUri, "/subscriptions");
        }

        /// <summary>
        /// Parse the resource value from a resourceUri
        /// </summary>
        /// <param name="resourceUri"></param>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public static string GetResourceValue(string resourceUri, string resourceName)
        {
            if (String.IsNullOrWhiteSpace(resourceUri))
            {
                return null;
            }

            if (!resourceName.StartsWith("/"))
            {
                resourceName = $"/{resourceName}";
            }

            if (!resourceUri.StartsWith("/"))
            {
                resourceUri = $"/{resourceUri}";
            }

            //Checks to see if the ResourceName and ResourceGroup is the same name and if so handles it specially.          
            string rgResourceName = $"/resourceGroups{resourceName}";
            int rgindex = resourceUri.IndexOf(rgResourceName, StringComparison.InvariantCultureIgnoreCase);
            if (rgindex != -1) //ResourceGroup Name and resourceName passed  is same. Example, resourceGroup Name is "Snapshots" and so is the ResourceName
            {
                string removedSameRgName = resourceUri.ToLowerInvariant().Split(new[] { rgResourceName.ToLowerInvariant() }, StringSplitOptions.None).Last();
                return removedSameRgName.Split('/')[1];
            }

            int index = resourceUri.IndexOf(resourceName, StringComparison.InvariantCultureIgnoreCase);
            if (index != -1)
            {
                var res = resourceUri.Substring(index + resourceName.Length).Split('/');

                //to handle the partial resource uri that doesn't have real resource name
                if (res.Length > 1)
                {
                    return res[1];
                }
            }

            return null;
        }

        /// <summary>
        /// Gets resource name based on a resource uri
        /// </summary>
        /// <param name="resourceUri"></param>
        /// <returns></returns>
        public static string GetResourcName(string resourceUri)
        {
            if (string.IsNullOrWhiteSpace(resourceUri))
            {
                return null;
            }

            var position = resourceUri.LastIndexOf(@"/", StringComparison.CurrentCultureIgnoreCase);
            return resourceUri.Substring(position + 1, resourceUri.Length - position - 1);
        }

        /// <summary>
        /// Gets the resource group name based on a resource uri
        /// </summary>
        /// <param name="resourceUri"></param>
        /// <returns></returns>
        public static string GetResourceGroup(string resourceUri)
        {
            if (string.IsNullOrWhiteSpace(resourceUri))
            {
                return null;
            }

            return GetResourceValue(resourceUri, "/resourceGroups");
        }

        /// <summary>
        /// Verifies if resource is snapshot resource
        /// </summary>
        /// <param name="resourceUri"></param>
        /// <returns></returns>
        public static bool IsAnfSnapshotResource(string resourceUri)
        {
            if (string.IsNullOrWhiteSpace(resourceUri))
            {
                return false;
            }

            var isAnfRp = resourceUri.IndexOf(@"/providers/Microsoft.NetApp/netAppAccounts/", StringComparison.CurrentCultureIgnoreCase) != -1;
            var isAnfSnapshot = resourceUri.IndexOf(@"/snapshots", StringComparison.CurrentCultureIgnoreCase) != -1;
            return isAnfRp && isAnfSnapshot;
        }

        /// <summary>
        /// Build the resource uri
        /// </summary>
        /// <param name="subid">the sub id</param>
        /// <param name="rg">the resource group</param>
        /// <param name="provider">the provider type</param>
        /// <param name="item">name of the resource</param>
        /// <returns></returns>
        public static string BuildResourceUri(string subid, string rg, string provider, string item)
        {
            string ret = string.Empty;

            if (string.IsNullOrEmpty(subid) || string.IsNullOrEmpty(rg) || string.IsNullOrEmpty(provider) || string.IsNullOrEmpty(item))
            {
                return ret;
            }

            ret = $"/subscriptions/{subid}/resourceGroups/{rg}/providers/{provider}/{item}";

            return ret;
        }

        /// <summary>
        /// Function used to wait for a specific ANF resource complete its deletion and ARM caching gets cleared
        /// </summary>
        /// <typeparam name="T">Resource Types as Snapshot, Volume, CapacityPool, and NetAppAccount</typeparam>
        /// <param name="client">ANF Client</param>
        /// <param name="resourceId">Resource Id of the resource being waited for being deleted</param>
        /// <param name="intervalInSec">Time in seconds that the sample will poll to check if the resource got deleted or not. Defaults to 10 seconds.</param>
        /// <param name="retries">How many retries before exting the wait for no resource function. Defaults to 60 retries.</param>
        /// <returns></returns>
        static public async Task WaitForAnfResource<T>(AzureNetAppFilesManagementClient client, string resourceId, int intervalInSec = 10, int retries = 60)
        {
            bool isFound = false;

            for (int i = 0; i < retries; i++)
            {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(intervalInSec));

                try
                {
                    if (typeof(T) == typeof(NetAppAccount))
                    {
                        await client.Accounts.GetAsync(ResourceUriUtils.GetResourceGroup(resourceId),
                            ResourceUriUtils.GetAnfAccount(resourceId));
                    }
                    else if (typeof(T) == typeof(CapacityPool))
                    {
                        await client.Pools.GetAsync(ResourceUriUtils.GetResourceGroup(resourceId),
                            ResourceUriUtils.GetAnfAccount(resourceId),
                            ResourceUriUtils.GetAnfCapacityPool(resourceId));

                    }
                    else if (typeof(T) == typeof(Volume))
                    {
                        await client.Volumes.GetAsync(ResourceUriUtils.GetResourceGroup(resourceId),
                            ResourceUriUtils.GetAnfAccount(resourceId),
                            ResourceUriUtils.GetAnfCapacityPool(resourceId),
                            ResourceUriUtils.GetAnfVolume(resourceId));

                    }
                    else if (typeof(T) == typeof(Snapshot))
                    {
                        await client.Snapshots.GetAsync(ResourceUriUtils.GetResourceGroup(resourceId),
                            ResourceUriUtils.GetAnfAccount(resourceId),
                            ResourceUriUtils.GetAnfCapacityPool(resourceId),
                            ResourceUriUtils.GetAnfVolume(resourceId),
                            ResourceUriUtils.GetAnfSnapshot(resourceId));
                    }
                    isFound = true;
                    break;
                }
                catch
                {
                    continue;
                }
            }
            if (!isFound)
                throw new Exception($"Resource: {resourceId} is not found");

        }

        /// <summary>
        /// Function used to wait for a specific ANF resource complete its deletion and ARM caching gets cleared
        /// </summary>
        /// <typeparam name="T">Resource Types as Snapshot, Volume, CapacityPool, and NetAppAccount</typeparam>
        /// <param name="client">ANF Client</param>
        /// <param name="resourceId">Resource Id of the resource being waited for being deleted</param>
        /// <param name="intervalInSec">Time in seconds that the sample will poll to check if the resource got deleted or not. Defaults to 10 seconds.</param>
        /// <param name="retries">How many retries before exting the wait for no resource function. Defaults to 60 retries.</param>
        /// <returns></returns>
        static public async Task WaitForNoAnfResource<T>(AzureNetAppFilesManagementClient client, string resourceId, int intervalInSec = 10, int retries = 60)
        {
            for (int i = 0; i < retries; i++)
            {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(intervalInSec));
                try
                {
                    if (typeof(T) == typeof(Snapshot))
                    {
                        var resource = await client.Snapshots.GetAsync(ResourceUriUtils.GetResourceGroup(resourceId),
                            ResourceUriUtils.GetAnfAccount(resourceId),
                            ResourceUriUtils.GetAnfCapacityPool(resourceId),
                            ResourceUriUtils.GetAnfVolume(resourceId),
                            ResourceUriUtils.GetAnfSnapshot(resourceId));
                    }
                    else if (typeof(T) == typeof(Volume))
                    {
                        var resource = await client.Volumes.GetAsync(ResourceUriUtils.GetResourceGroup(resourceId),
                            ResourceUriUtils.GetAnfAccount(resourceId),
                            ResourceUriUtils.GetAnfCapacityPool(resourceId),
                            ResourceUriUtils.GetAnfVolume(resourceId));
                    }
                    else if (typeof(T) == typeof(CapacityPool))
                    {
                        var resource = await client.Pools.GetAsync(ResourceUriUtils.GetResourceGroup(resourceId),
                            ResourceUriUtils.GetAnfAccount(resourceId),
                            ResourceUriUtils.GetAnfCapacityPool(resourceId));
                    }
                    else if (typeof(T) == typeof(NetAppAccount))
                    {
                        var resource = await client.Accounts.GetAsync(ResourceUriUtils.GetResourceGroup(resourceId),
                            ResourceUriUtils.GetAnfAccount(resourceId));
                    }
                }
                catch (Exception ex)
                {
                    // The following HResult is thrown if no resource is found
                    if (ex.HResult == -2146233088)
                    {
                        break;
                    }
                    throw;
                }
            }
        }

        /// <summary>
        /// Waits for replication to become in status "Mirrored"
        /// </summary>
        /// <param name="client">ANF Client</param>
        /// <param name="resourceId">Resource Id of the resource being waited for being deleted</param>
        /// <param name="intervalInSec">Time in seconds that the sample will poll to check if the resource got deleted or not. Defaults to 10 seconds.</param>
        /// <param name="retries">How many retries before exting the wait for no resource function. Defaults to 60 retries.</param>
        /// <returns></returns>
        static public async Task WaitForCompleteReplicationStatus(AzureNetAppFilesManagementClient client, string resourceId, int intervalInSec = 10, int retries = 60)
        {
            using AzureEventSourceListener listener = AzureEventSourceListener.CreateTraceLogger(EventLevel.Verbose);
            for (int i = 0; i < retries; i++)
            {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(intervalInSec));
                try
                {
                    var status = await client.Volumes.ReplicationStatusMethodAsync(ResourceUriUtils.GetResourceGroup(resourceId),
                        ResourceUriUtils.GetAnfAccount(resourceId),
                        ResourceUriUtils.GetAnfCapacityPool(resourceId),
                        ResourceUriUtils.GetAnfVolume(resourceId));
                    if (status.MirrorState.ToLower().Equals("mirrored"))
                        break;
                }
                catch (Exception ex)
                {
                    if (!(ex.Message.ToLower().Contains("creating") && ex.Message.ToLower().Contains("replication")))
                        throw;
                }
            }
        }

        /// <summary>
        /// Waits for replication to become in "Broken" status
        /// </summary>
        /// <param name="client">ANF Client</param>
        /// <param name="resourceId">Resource Id of the resource being waited for being deleted</param>
        /// <param name="intervalInSec">Time in seconds that the sample will poll to check if the resource got deleted or not. Defaults to 10 seconds.</param>
        /// <param name="retries">How many retries before exting the wait for no resource function. Defaults to 60 retries.</param>
        /// <returns></returns>
        static public async Task WaitForBrokenReplicationStatus(AzureNetAppFilesManagementClient client, string resourceId, int intervalInSec = 10, int retries = 60)
        {
            for (int i = 0; i < retries; i++)
            {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(intervalInSec));
                try
                {
                    var status = await client.Volumes.ReplicationStatusMethodAsync(ResourceUriUtils.GetResourceGroup(resourceId),
                        ResourceUriUtils.GetAnfAccount(resourceId),
                        ResourceUriUtils.GetAnfCapacityPool(resourceId),
                        ResourceUriUtils.GetAnfVolume(resourceId));
                    if (status.MirrorState.ToLower().Equals("broken"))
                        break;
                }
                catch
                {
                    throw;
                }

            }
        }
    }
}

