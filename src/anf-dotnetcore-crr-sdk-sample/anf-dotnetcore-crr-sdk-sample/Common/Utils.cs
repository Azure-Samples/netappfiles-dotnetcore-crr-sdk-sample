// Copyright (c) Microsoft and contributors.  All rights reserved.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.

namespace Microsoft.Azure.Management.ANF.Samples.Common
{

    using System;

    /// <summary>
    /// Contains public methods to get configuration settings, to initiate authentication, output error results, etc.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Simple function to display this console app basic information
        /// </summary>
        public static void DisplayConsoleAppHeader()
        {
            Console.WriteLine("Azure NetAppFiles .netcore NFS 4.1 SDK Sample - Sample project that creates an Azure NetApp Files Volume that uses NFS 4.1 protocol");
            Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("");
        }

        /// <summary>
        /// Displays errors messages in red
        /// </summary>
        /// <param name="message">Message to be written in console</param>
        public static void WriteErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Utils.WriteConsoleMessage(message);
            Console.ResetColor();
        }

        /// <summary>
        /// Displays errors messages in red
        /// </summary>
        /// <param name="message">Message to be written in console</param>
        public static void WriteConsoleMessage(string message)
        {
            Console.WriteLine($"{DateTime.Now}: {message}");
        }
    }
}
