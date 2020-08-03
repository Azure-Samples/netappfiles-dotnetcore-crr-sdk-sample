// Copyright (c) Microsoft and contributors.  All rights reserved.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.

namespace anf_dotnetcore_crr_sdk_sample.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using anf_dotnetcore_crr_sdk_sample.Model;
    //using Microsoft.Azure.Management.NetApp.Models;
    //using Microsoft.Azure.Management.ANF.Samples.Model;
    using Microsoft.Identity.Client;
    using Microsoft.Rest;
    using Microsoft.Rest.Azure;
    using Microsoft.Rest.Azure.Authentication;
    using Newtonsoft.Json;

    /// <summary>
    /// Contains public methods to get configuration settigns, to initiate authentication, output error results, etc.
    /// </summary>
    
    public static class ServicePrincipalAuth
    {
        /// <summary>
        /// Gets service principal based credentials
        /// </summary>
        /// <param name="authEnvironmentVariable">Environment variable that points to the file system secured azure auth settings</param>
        /// <returns>ServiceClientCredentials</returns>
        public static async Task<ServiceClientCredentials> GetServicePrincipalCredential(string authEnvironmentVariable)
        {
            AzureAuthInfo authSettings = Deserialize<AzureAuthInfo>(Environment.GetEnvironmentVariable(authEnvironmentVariable));

            var aadSettings = new ActiveDirectoryServiceSettings
            {
                AuthenticationEndpoint = new Uri(authSettings.ActiveDirectoryEndpointUrl),
                TokenAudience = new Uri(authSettings.ManagementEndpointUrl),
                ValidateAuthority = true
            };

            return await ApplicationTokenProvider.LoginSilentAsync(
                authSettings.TenantId,
                authSettings.ClientId,
                authSettings.ClientSecret,
                aadSettings);
        }

        /// <summary>
        /// Deserialize json strings
        /// </summary>
        /// <typeparam name="T">Type that is used for the deserialization process</typeparam>
        /// <param name="filePath">Json file path</param>
        /// <returns>T</returns>
        public static T Deserialize<T>(string filePath)
        {
            var serializer = new JsonSerializer();
            using (var sr = new StreamReader(filePath))
            {
                using (var reader = new JsonTextReader(sr))
                {
                    return serializer.Deserialize<T>(reader);
                }
            }
        }

    }
}
