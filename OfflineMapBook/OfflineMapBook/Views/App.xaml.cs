// <copyright file="App.cs" company="Esri">
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
// <author>Mara Stoica</author>using Esri.ArcGISRuntime;
namespace OfflineMapBook
{
    using System;
    using System.Windows;
    using Esri.ArcGISRuntime;

    /// <summary>
    /// The main application class
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Code that runs when the app starts
        /// </summary>
        /// <param name="sender">sender control</param>
        /// <param name="e">event args</param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                // Deployed applications must be licensed at the Basic level or greater (https://developers.arcgis.com/licensing).
                // To enable Basic level functionality set the Client ID property before initializing the ArcGIS Runtime.
                 ArcGISRuntimeEnvironment.SetLicense("runtimelite,1000,rud2672252234,none,D7MFA0PL4P2SPF002031");

                // Initialize the ArcGIS Runtime before any components are created.
                ArcGISRuntimeEnvironment.Initialize();

                // Standard level functionality can be enabled once the ArcGIS Runtime is initialized.
                // To enable Standard level functionality you must either:
                // 1. Allow the app user to authenticate with ArcGIS Online or Portal for ArcGIS then call the set license method with their license info.
                // ArcGISRuntimeEnvironment.License.SetLicense(LicenseInfo object returned from an ArcGIS Portal instance)
                // 2. Call the set license method with a license string obtained from Esri Customer Service or your local Esri distributor.
                // ArcGISRuntimeEnvironment.License.SetLicense("<Your LiC:\Projects\mapbook-wpf\OfflineMapBook\OfflineMapBook\ViewModels\MapViewModel.cscense String or Strings (extensions) Here>");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "ArcGIS Runtime initialization failed.");

                // Exit application
                this.Shutdown();
            }
        }
    }
}
