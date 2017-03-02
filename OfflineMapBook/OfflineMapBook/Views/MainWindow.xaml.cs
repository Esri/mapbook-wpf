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
    using System.Threading.Tasks;
    using System.Windows;
    using Esri.ArcGISRuntime.Mapping;
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

            // Test if singleton instance exists
            if (AppViewModel.Instance == null)
            {
                this.LoadMmpkAsync();
            }
        }

        /// <summary>
        /// Loads the Mobile Map Package and creates single instance of the AppViewModel
        /// </summary>
        /// <returns>Async task</returns>
        internal async Task LoadMmpkAsync()
        {
            // TODO: Remove hard coded mmpk path when DownloadViewModel is implemented
            var mmpk = await MobileMapPackage.OpenAsync(@"C:\Users\mara8799\Downloads\OfflineMapbook_v7.mmpk");
            AppViewModel.Instance = AppViewModel.Create(mmpk);

            // Set data context for the main screen and load main screen
            this.DataContext = AppViewModel.Instance;
            AppViewModel.Instance.DisplayViewModel = new MainViewModel();
        }
    }
}
