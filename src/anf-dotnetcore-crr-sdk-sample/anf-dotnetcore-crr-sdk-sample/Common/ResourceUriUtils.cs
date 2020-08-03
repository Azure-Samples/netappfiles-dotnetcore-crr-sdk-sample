// Copyright (c) Microsoft and contributors.  All rights reserved.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace anf_dotnetcore_crr_sdk_sample.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Identity.Client;
    using Microsoft.Rest;
    using Microsoft.Rest.Azure;

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
    }
}
