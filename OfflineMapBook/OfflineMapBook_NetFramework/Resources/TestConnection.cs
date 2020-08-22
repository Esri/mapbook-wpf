// <copyright file="TestConnection.cs" company="Esri">
//      Copyright (c) 2017 Esri. All rights reserved.
//
//      Licensed under the Apache License, Version 2.0 (the "License");
//      you may not use this file except in compliance with the License.
//      You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//      Unless required by applicable law or agreed to in writing, software
//      distributed under the License is distributed on an "AS IS" BASIS,
//      WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//      See the License for the specific language governing permissions and
//      limitations under the License.
// </copyright>
// <author>Mara Stoica</author>

namespace Esri.ArcGISRuntime.OpenSourceApps.OfflineMapBook.Resources
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// Helper class to test internext connection
    /// </summary>
    internal static class TestConnection
    {
        /// <summary>
        /// Tests internet connection
        /// </summary>
        /// <returns>Returns true if device is connected to the internet</returns>
        internal static bool IsConnectedToInternet()
        {
            bool returnValue = false;
            try
            {
                int desc;
                returnValue = InternetGetConnectedState(out desc, 0);
            }
            catch
            {
                returnValue = false;
            }

            return returnValue;
        }

        [DllImport("wininet.dll")]
        private static extern bool InternetGetConnectedState(out int description, int reservedValue);
    }
}
