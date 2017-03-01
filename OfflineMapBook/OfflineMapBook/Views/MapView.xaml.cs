// <copyright file="BooleanToVisibilityInverseConverter.cs" company="Esri">
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
    using System.Windows.Controls;
    using ViewModels;

    /// <summary>
    /// Interaction logic for MapView.xaml
    /// </summary>
    public partial class MapView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapView"/> class.
        /// </summary>
        public MapView()
        {
            this.InitializeComponent();

            // Set data context for the view
            this.DataContextChanged += (s, e) =>
            {
                if (this.DataContext != null)
                {
                    this.ViewModel = this.DataContext as MapViewModel;
                    this.ViewModel.PropertyChanged += this.ViewModel_PropertyChanged;
                }
            };
        }

        private MapViewModel ViewModel { get; set; }

        /// <summary>
        /// Property changed event handler for the view
        /// </summary>
        /// <param name="sender">Sender control</param>
        /// <param name="e">event args</param>
        private async void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                // When viewpoint changes, call SetViewpointAsync on the MapView
                case "ViewPoint":
                    if (this.ViewModel.ViewPoint != null)
                    {
                        await this.MapBookMapView.SetViewpointAsync(this.ViewModel.ViewPoint);
                    }

                    break;
            }
        }
    }
}
