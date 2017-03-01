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
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using Esri.ArcGISRuntime.Data;
    using ViewModels;

    /// <summary>
    /// Interaction logic for MapView.xaml
    /// </summary>
    public partial class MapView : UserControl
    {
        private bool isViewDoubleTapped;

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

            this.MapBookMapView.GeoViewTapped += this.MapBookMapView_GeoViewTapped;
            this.MapBookMapView.GeoViewDoubleTapped += (s, e) =>
            {
                this.isViewDoubleTapped = true;
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

        /// <summary>
        /// Gets called when user taps on the map
        /// </summary>
        /// <param name="sender">Sender control</param>
        /// <param name="e">event args</param>
        private async void MapBookMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            // Wait for double tap to fire
            await Task.Delay(500);

            // If view has been double tapped, set tapped to handled and flag back to false
            // If view has been tapped just once, perform identify
            if (this.isViewDoubleTapped == true)
            {
                e.Handled = true;
                this.isViewDoubleTapped = false;
            }
            else
            {
                // get the tap location in screen units
                Point tapScreenPoint = e.Position;

                var pixelTolerance = 20;
                var returnPopupsOnly = false;
                var maxLayerResults = 5;

                // identify all layers in the MapView, passing the tap point, tolerance, types to return, and max results
                IReadOnlyList<IdentifyLayerResult> idLayerResults = await this.MapBookMapView.IdentifyLayersAsync(tapScreenPoint, pixelTolerance, returnPopupsOnly, maxLayerResults);
                this.ViewModel.IdentifyCommand.Execute(idLayerResults);
            }
        }
    }
}
