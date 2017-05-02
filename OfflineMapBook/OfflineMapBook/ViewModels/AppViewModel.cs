// <copyright file="AppViewModel.cs" company="Esri">
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
    using Esri.ArcGISRuntime.Mapping;

    /// <summary>
    /// Accessible View Model class sets Mobile Map Package and Display View Model
    /// </summary>
    internal sealed class AppViewModel : BaseViewModel
    {
        private BaseViewModel displayViewModel;

        /// <summary>
        /// Gets or sets the active Display View Model
        /// </summary>
        public BaseViewModel DisplayViewModel
        {
            get
            {
                return this.displayViewModel;
            }

            set
            {
                this.displayViewModel = value;
                this.OnPropertyChanged(nameof(this.DisplayViewModel));
            }
        }

        /// <summary>
        /// Gets or sets the static instance of the class
        /// </summary>
        internal static AppViewModel Instance { get; set; }

        /// <summary>
        /// Gets or sets the Mobile Map Package
        /// </summary>
        internal MobileMapPackage Mmpk { get; set; }

        /// <summary>
        /// Creates instance of AppViewModel
        /// </summary>
        /// <param name="mmpk">Mobile Map Package</param>
        /// <returns>Instance of AppViewModel</returns>
        internal static AppViewModel Create()
        {
            var appViewModel = new AppViewModel();
            //appViewModel.Mmpk = mmpk;
            return appViewModel;
        }
    }
}
