// <copyright file="MainViewModel.cs" company="Esri">
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
    using System.Collections.Generic;
    using System.Windows.Input;
    using Commands;
    using Esri.ArcGISRuntime.Mapping;

    /// <summary>
    /// Main view model handles logic for the main window
    /// </summary>
    internal class MainViewModel : BaseViewModel
    {
        private IReadOnlyList<Map> mapItems;
        private ICommand openMapCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel()
        {
            this.MmpkItem = AppViewModel.Instance.Mmpk.Item;
            this.MapItems = AppViewModel.Instance.Mmpk.Maps;
        }

        /// <summary>
        /// Gets or sets list of MapItems coming from inside the mmpk
        /// </summary>
        public IReadOnlyList<Map> MapItems
        {
            get
            {
                return this.mapItems;
            }

            set
            {
                this.mapItems = value;
                this.OnPropertyChanged(nameof(this.MapItems));
            }
        }

        /// <summary>
        /// Gets or sets the MmpkItem
        /// </summary>
        public Esri.ArcGISRuntime.Portal.Item MmpkItem { get; set; }

        /// <summary>
        /// Gets command called by UI button
        /// </summary>
        public ICommand OpenMapCommand
        {
            get
            {
                return this.openMapCommand ?? (this.openMapCommand = new ParameterCommand((x) => this.OpenMapAction((Map)x), true));
            }
        }

        /// <summary>
        /// Opens a new map screen by setting the DisplayViewModel
        /// </summary>
        /// <param name="map">Map to be opened</param>
        public void OpenMapAction(Map map)
        {
            AppViewModel.Instance.DisplayViewModel = new MapViewModel(map);
        }
    }
}
