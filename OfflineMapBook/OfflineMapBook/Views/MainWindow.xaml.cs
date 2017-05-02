// <copyright file="MainWindow.cs" company="Esri">
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
namespace OfflineMapBook
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Forms;
    using Esri.ArcGISRuntime.Mapping;
    using Properties;
    using ViewModels;

    /// <summary>
    /// View logic for the main window
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            this.InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            // Test if download path has been specified by user, if not, prompt
            // If download path exists, check if valid
            if (string.IsNullOrEmpty(Settings.Default.DownloadPath) || !Directory.Exists(Settings.Default.DownloadPath))
            {
                this.PromptUserForDownloadDirectory();
            }

            // Test if AppViewModel singleton instance exists
            if (AppViewModel.Instance == null)
            {
                // Set data context for the main screen and load main screen
                AppViewModel.Instance = AppViewModel.Create();
                this.DataContext = AppViewModel.Instance;
            }

            // Make instance of the DownloadViewModel and set it as datacontext.
            // This will set the active view as the DownloadView
            try
            {
                var downloadViewModel = new DownloadViewModel();
                AppViewModel.Instance.DisplayViewModel = downloadViewModel;
                await downloadViewModel.ConnectToPortalAsync();
            }
            catch (Exception ex)
            {
                // If unexpected exception happens during download, ignore it and load existing map
                System.Windows.MessageBox.Show(
                    "An error has occured during the map download: " + ex.Message + " The previously downloaded map will now be loaded.",
                    "Unhandled Exception",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                await this.LoadMmpkAsync();
            }
        }

        private void PromptUserForDownloadDirectory()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                var result = dialog.ShowDialog();

                // TODO: Test that user has write permissions to the directory
                // Test that the user selected a valid directory
                if (Directory.Exists(dialog.SelectedPath))
                {
                    Settings.Default.DownloadPath = dialog.SelectedPath;
                    Settings.Default.Save();
                    Settings.Default.Reload();
                }
                else
                {
                    System.Windows.MessageBox.Show("Please select a valid folder for the data to be stored in. The application cannot continue until a valid folder is selected.");
                    this.PromptUserForDownloadDirectory();
                }
            }
        }

        /// <summary>
        /// Loads the Mobile Map Package and creates single instance of the AppViewModel
        /// </summary>
        /// <returns>Async task</returns>
        private async Task LoadMmpkAsync()
        {
            // Open mmpk if it exists
            // If no mmpk is found, alert the user and shut down the application
            var mmpkFullPath = Path.Combine(Settings.Default.DownloadPath, Settings.Default.MmpkFileName);

            if (File.Exists(mmpkFullPath))
            {
                try
                {
                    var mmpk = await MobileMapPackage.OpenAsync(mmpkFullPath);
                    AppViewModel.Instance.Mmpk = mmpk;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "Error opening map", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                System.Windows.MessageBox.Show(
                    "Map could not be downloaded and no locally stored map was found. Application will now exit. Please restart application to re-try the map download",
                    "No Map Found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Environment.Exit(0);
            }

            // Set data context for the main screen and load main screen
            this.DataContext = AppViewModel.Instance;
            AppViewModel.Instance.DisplayViewModel = new MainViewModel();
        }
    }
}
