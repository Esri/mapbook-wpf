// <copyright file="DownloadViewModel.cs" company="Esri">
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

namespace OfflineMapBook.ViewModels
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using System.Windows;
    using Esri.ArcGISRuntime.Portal;
    using Esri.ArcGISRuntime.Security;
    using Properties;

    /// <summary>
    /// View model to perform the mmpk download
    /// </summary>
    internal class DownloadViewModel : BaseViewModel
    {
        private string statusMessage;

        /// <summary>
        /// Gets the status message of the download operation
        /// </summary>
        public string StatusMessage
        {
            get
            {
                return this.statusMessage;
            }

            private set
            {
                if (this.statusMessage != value)
                {
                    this.statusMessage = value;
                    this.OnPropertyChanged(nameof(this.StatusMessage));
                }
            }
        }

        /// <summary>
        /// Connects to secure portal instance through Integrated Windows Authentication
        /// </summary>
        /// <returns> Authenticated Portal credential </returns>
        public async Task ConnectToPortalAsync()
        {
            this.StatusMessage = "Connecting to Portal";

            // Set uri for Portal with IWA authentication enabled
            // Note that this will not work if you do not have passthrough authentication enabled
            // If you open the URL in Internet Explorer, and it prompts for credentials, this will probably not work
            var serviceUri = new Uri(Settings.Default.PortalUri);

            // Get default credential for the authenticated Windows user
            var networkCredential = CredentialCache.DefaultCredentials.GetCredential(serviceUri, "Basic");

            // Create ArcGIS Network credential
            ArcGISNetworkCredential arcGisCredential = new ArcGISNetworkCredential
            {
                Credentials = networkCredential,
                ServiceUri = serviceUri,
            };

            // Add the credential to the AuthenticationManager
            Esri.ArcGISRuntime.Security.AuthenticationManager.Current.AddCredential(arcGisCredential);

            // Call GetData to download mobile map package
            await this.GetDataAsync();
        }

        /// <summary>
        /// Gets the data for the map. It downloads the mmpk if it doesn't exist or if there's a newer one available
        /// </summary>
        /// <returns>The map data.</returns>
        public async Task GetDataAsync()
        {
            // TODO: Test if device is online
            // If offline, test if mmpk exists and load it
            // If offline and no mmpk, show error
            // Show error message if unable to downoad the mmpk. This is usually when the device is online but signal isn't strong enough and connection to Portal times out
            try
            {
                // Get portal item
                var portal = await ArcGISPortal.CreateAsync(new Uri(Settings.Default.PortalUri)).ConfigureAwait(false);
                var item = await PortalItem.CreateAsync(portal, Settings.Default.PortalItemID).ConfigureAwait(false);

                var mmpkFullPath = Path.Combine(Settings.Default.DownloadPath, Settings.Default.MmpkFileName);

                // Test if mmpk is not already downloaded or is older than current portal version
                if (!File.Exists(mmpkFullPath) || item.Modified.LocalDateTime > Settings.Default.MmpkDownloadDate)
                {
                    this.StatusMessage = "Downloading map";

                    try
                    {
                        // Download new file and store in temp location
                        var tempFile = Path.GetTempFileName();
                        using (var stream = await item.GetDataAsync().ConfigureAwait(false))
                        {
                            using (var file = File.Create(tempFile))
                            {
                                await stream.CopyToAsync(file).ConfigureAwait(false);
                                Settings.Default.MmpkDownloadDate = DateTime.Now;
                                Settings.Default.Save();
                                Settings.Default.Reload();
                            }
                        }

                        // Once download was successful, delete mmpk file if it already exists
                        if (File.Exists(Settings.Default.MmpkFileName))
                        {
                            File.Delete(Settings.Default.MmpkFileName);
                        }

                        // Rename temp file to replace the mmpk file
                        File.Move(tempFile, mmpkFullPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Application was unable to download the map. " + ex.Message);

                        // TODO: Alert user app was unable to download new file and old file will be used, if available
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
