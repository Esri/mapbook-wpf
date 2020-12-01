// <copyright file="DownloadViewModel.cs" company="Esri">
//      Copyright (c) 2017 Esri. All rights reserved.
//
//      Licensed under the Apache License, Version 2.0 (the "License");
//      you may not use this file except in compliance with the License.
//      You may obtain a copy of the License at
//
//      https://www.apache.org/licenses/LICENSE-2.0
//
//      Unless required by applicable law or agreed to in writing, software
//      distributed under the License is distributed on an "AS IS" BASIS,
//      WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//      See the License for the specific language governing permissions and
//      limitations under the License.
// </copyright>
// <author>Mara Stoica</author>

namespace Esri.ArcGISRuntime.OpenSourceApps.OfflineMapBook.ViewModels
{
    using System;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography;
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
        private static byte[] entropy = System.Text.Encoding.Unicode.GetBytes("U7Q4RIgIRNZ5hXm27JSjrKqqjYKSRKKx3EVKl61M");

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
            this.StatusMessage = "Connecting to Portal ...";

            // Set uri for Portal
            var serviceUri = new Uri(Settings.Default.PortalUri);

            switch (Settings.Default.AuthenticationType)
            {
                // For Portal with IWA authentication enabled
                // Note that this will only work if you have passthrough authentication enabled
                // If you open the URL in Internet Explorer, and it prompts for credentials, this will probably not work
                case "IWA":
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
                    break;

                // For Portal with OAuth authentication enabled
                case "OAuth":
                    this.UpdateAuthenticationManager(
                        serviceUri,
                        Settings.Default.OAuthClientID,
                        Settings.Default.OAuthRedirectUri,
                        Settings.Default.OAuthClientSecret);
                    break;

                    // For Portal with no authentication enabled.
                    // Use this for testing the app or if your data does not require to be secured
                case "None":
                default:
                    break;
            }

            // Call GetData to download mobile map package
            await this.GetDataAsync();
        }

        private void UpdateAuthenticationManager(Uri serviceUri, string clientID, string redirectUri, string clientSecret)
        {
            // Register the server information with the AuthenticationManager
            var serverInfo = new ServerInfo(serviceUri)
            {
                OAuthClientInfo = new OAuthClientInfo(clientID, new Uri(redirectUri))
            };

            // Use OAuthAuthorizationCode if you need a refresh token (and have specified a valid client secret)
            serverInfo.TokenAuthenticationType = TokenAuthenticationType.OAuthAuthorizationCode;
            serverInfo.OAuthClientInfo.ClientSecret = clientSecret;

            // Register this server with AuthenticationManager
            Esri.ArcGISRuntime.Security.AuthenticationManager.Current.RegisterServer(serverInfo);

            // Use the OAuthAuthorize class in this project to handle OAuth communication
            Esri.ArcGISRuntime.Security.AuthenticationManager.Current.OAuthAuthorizeHandler = new Views.OAuthAuthorize();

            // Use a function in this class to challenge for credentials
            Esri.ArcGISRuntime.Security.AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(this.CreateCredentialAsync);
        }

        /// <summary>
        /// Gets the credentials 
        /// </summary>
        /// <param name="info">Credentials info</param>
        /// <returns>Credential</returns>
        private async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            // ChallengeHandler function for AuthenticationManager that will be called whenever access to a secured
            // resource is attempted
            OAuthTokenCredential credential = null;

            try
            {
                // Create generate token options if necessary
                if (info.GenerateTokenOptions == null)
                {
                    info.GenerateTokenOptions = new GenerateTokenOptions();
                }

                // Use encrypted refresh token if it exists
                if (string.IsNullOrEmpty(Settings.Default.OAuthRefreshToken))
                {
                    // AuthenticationManager will handle challenging the user for credentials
                    credential = await Esri.ArcGISRuntime.Security.AuthenticationManager.Current.GenerateCredentialAsync(
                        info.ServiceUri,
                        info.GenerateTokenOptions) as OAuthTokenCredential;
                }
                else
                {
                    var token = ProtectedData.Unprotect(
                        Convert.FromBase64String(Settings.Default.OAuthRefreshToken),
                        entropy,
                        DataProtectionScope.CurrentUser);
                    credential = new OAuthTokenCredential();
                    credential.ServiceUri = info.ServiceUri;
                    credential.OAuthRefreshToken = System.Text.Encoding.Unicode.GetString(token);
                    credential.GenerateTokenOptions = info.GenerateTokenOptions;
                    await credential.RefreshTokenAsync();
                }
            }
            catch (Exception ex)
            {
                // Exception will be reported in calling function
                throw ex;
            }

            // Encrypt and save refresh token
            if (!string.IsNullOrEmpty(credential.OAuthRefreshToken))
            {
                var token = ProtectedData.Protect(
                        System.Text.Encoding.Unicode.GetBytes(credential.OAuthRefreshToken),
                        entropy,
                        DataProtectionScope.CurrentUser);
                Settings.Default.OAuthRefreshToken = Convert.ToBase64String(token);
                Settings.Default.Save();
                Settings.Default.Reload();
            }

            return credential;
        }

        /// <summary>
        /// Gets the data for the map. It downloads the mmpk if it doesn't exist or if there's a newer one available
        /// </summary>
        /// <returns>The map data.</returns>
        private async Task GetDataAsync()
        {
            if (Resources.TestConnection.IsConnectedToInternet())
            {
                try
                {
                    // Get portal item
                    var portal = await ArcGISPortal.CreateAsync(new Uri(Settings.Default.PortalUri)).ConfigureAwait(false);
                    var item = await PortalItem.CreateAsync(portal, Settings.Default.PortalItemID).ConfigureAwait(false);

                    var mmpkFullPath = Path.Combine(Settings.Default.DownloadPath, Settings.Default.MmpkFileName);

                    // Test if mmpk is not already downloaded or is older than current portal version
                    if (!File.Exists(mmpkFullPath) || item.Modified.LocalDateTime > Settings.Default.MmpkDownloadDate)
                    {
                        this.StatusMessage = "Downloading map ...";

                        try
                        {
                            // Download new file and store in temp location
                            var tempFile = Path.GetTempFileName();
                            using (var stream = await item.GetDataAsync().ConfigureAwait(false))
                            {
                                using (var file = File.Create(tempFile))
                                {
                                    await stream.CopyToAsync(file).ConfigureAwait(false);
                                }
                            }

                            this.StatusMessage = "Finalizing download ...";

                            // Once download was successful, delete mmpk file if it already exists
                            if (File.Exists(mmpkFullPath))
                            {
                                File.Delete(mmpkFullPath);
                            }

                            // Rename temp file to replace the mmpk file
                            File.Move(tempFile, mmpkFullPath);

                            // Set and save the download date
                            Settings.Default.MmpkDownloadDate = DateTime.Now;
                            Settings.Default.Save();
                            Settings.Default.Reload();
                        }
                        catch (Exception ex)
                        {
                            this.StatusMessage = "Download failed";
                            MessageBox.Show(
                                "Connection to Portal was successful. However, the application was unable to download the map. " + ex.Message,
                                "Error downloading map",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.StatusMessage = "Download failed";
                    MessageBox.Show(
                        ex.Message + " Application will attempt to use previously downloaded version of the map, if available.",
                        "Error downloading map",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show(
                    "Device does not seem to be connected to the internet. Application will attempt to use previously downloaded version of the map, if available.",
                    "No Internet Connection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
        }
    }
}
