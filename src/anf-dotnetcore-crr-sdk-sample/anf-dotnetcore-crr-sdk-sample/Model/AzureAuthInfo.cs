// Copyright (c) Microsoft and contributors.  All rights reserved.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.

namespace Microsoft.Azure.Management.ANF.Samples.Model
{
    /// <summary>
    /// Creates an instance of AzureAuthInfo. This is used to read the contents of azureauth.json to perform SP authentication
    /// </summary>
    public class AzureAuthInfo
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string SubscriptionId { get; set; }
        public string TenantId { get; set; }
        public string ActiveDirectoryEndpointUrl { get; set; }
        public string ResourceManagerEndpointUrl { get; set; }
        public string ActiveDirectoryGraphResourceId { get; set; }
        public string SqlManagementEndpointUrl { get; set; }
        public string GalleryEndpointUrl { get; set; }
        public string ManagementEndpointUrl { get; set; }
    }
}
