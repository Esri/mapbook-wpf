// <copyright file="MapViewModel.cs" company="Esri">
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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using Commands;
    using Esri.ArcGISRuntime.Data;
    using Esri.ArcGISRuntime.Mapping;
    using Esri.ArcGISRuntime.Symbology;
    using Esri.ArcGISRuntime.Tasks.Geocoding;
    using Esri.ArcGISRuntime.UI;

    /// <summary>
    /// View model performs logic related to the map screen
    /// </summary>
    internal class MapViewModel : BaseViewModel
    {
        private Map map;
        private GraphicsOverlayCollection graphicsOverlays;
        private string searchText;
        private List<SuggestResult> suggestionsList;
        private Viewpoint viewPoint;
        private ICommand backCommand;
        private ICommand searchCommand;
        private ICommand identifyCommand;
        private ICommand zoomToBookmarkCommand;
        private ICommand closeIdentifyCommand;
        private ObservableCollection<IdentifyModel> identifyModelsList = new ObservableCollection<IdentifyModel>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MapViewModel"/> class.
        /// </summary>
        /// <param name="map">Current map</param>
        public MapViewModel(Map map)
        {
            this.Map = map;
            this.Locator = AppViewModel.Instance.Mmpk.LocatorTask;
            this.GraphicsOverlays = new GraphicsOverlayCollection();
            this.GetInfoFromLocatorAsync();
        }

        /// <summary>
        /// Gets or sets the map
        /// </summary>
        public Map Map
        {
            get
            {
                return this.map;
            }

            set
            {
                if (this.map != value)
                {
                    this.map = value;
                    this.OnPropertyChanged(nameof(this.Map));
                }
            }
        }

        /// <summary>
        /// Gets the graphics overlays collection to hold all graphics overlays
        /// </summary>
        public GraphicsOverlayCollection GraphicsOverlays
        {
            get
            {
                return this.graphicsOverlays;
            }

            private set
            {
                if (value != null)
                {
                    this.graphicsOverlays = value;
                    this.OnPropertyChanged(nameof(this.GraphicsOverlays));
                }
            }

        }

        /// <summary>
        /// Gets the map viewpoint
        /// </summary>
        public Viewpoint ViewPoint
        {
            get
            {
                return this.viewPoint;
            }

            private set
            {
                if (this.viewPoint != value && value != null)
                {
                    this.viewPoint = value;
                    this.OnPropertyChanged(nameof(this.ViewPoint));
                }
            }
        }

        /// <summary>
        /// Gets or sets the search text the user has entered
        /// </summary>
        public string SearchText
        {
            get
            {
                return this.searchText;
            }

            set
            {
                if (this.searchText != value)
                {
                    this.searchText = value;
                    this.OnPropertyChanged(nameof(this.SearchText));

                    // Call method to get location suggestions
                    this.GetLocationSuggestionsAsync(this.SearchText);
                }
            }
        }

        /// <summary>
        /// Gets the list of suggestions
        /// </summary>
        public List<SuggestResult> SuggestionsList
        {
            get
            {
                return this.suggestionsList;
            }

            private set
            {
                if (this.suggestionsList != value)
                {
                    this.suggestionsList = value;
                    this.OnPropertyChanged(nameof(this.SuggestionsList));
                }
            }
        }

        /// <summary>
        /// Gets the list of Identify models to be used to populate the identify control
        /// </summary>
        public ObservableCollection<IdentifyModel> IdentifyModelsList
        {
            get
            {
                return this.identifyModelsList;
            }

            private set
            {
                if (this.identifyModelsList != value)
                {
                    this.identifyModelsList = value;
                    this.OnPropertyChanged(nameof(this.IdentifyModelsList));
                }
            }
        }

        /// <summary>
        /// Gets the command to go back to the main screen
        /// </summary>
        public ICommand BackCommand
        {
            get
            {
                return this.backCommand ?? (this.backCommand = new SimpleCommand(() => this.BackToMainView(), true));

            }
        }

        /// <summary>
        /// Gets the command to search using the locator
        /// </summary>
        public ICommand SearchCommand
        {
            get
            {
                return this.searchCommand ?? (this.searchCommand = new ParameterCommand(
                    (x) =>
                    {
                        this.GetSearchedLocationAsync((string)x);
                    }, true));
            }
        }

        /// <summary>
        /// Gets the command to identify features
        /// </summary>
        public ICommand IdentifyCommand
        {
            get
            {
                return this.identifyCommand ?? (this.identifyCommand = new ParameterCommand(
                     (x) =>
                     {
                         this.GetIdentifyInfoAsync((IReadOnlyList<IdentifyLayerResult>)x);
                     }, true));
            }
        }

        /// <summary>
        /// Gets the command to zoom to selected bookmark
        /// </summary>
        public ICommand ZoomToBookmarkCommand
        {
            get
            {
                return this.zoomToBookmarkCommand ?? (this.zoomToBookmarkCommand = new ParameterCommand(
                (x) =>
                {
                    this.ViewPoint = (Viewpoint)x;
                }, true));
            }
        }

        /// <summary>
        /// Gets the command to close the identify popup
        /// </summary>
        public ICommand CloseIdentifyCommand
        {
            get
            {
                return this.closeIdentifyCommand ?? (this.closeIdentifyCommand = new SimpleCommand(() => this.IdentifyModelsList.Clear(), true));
            }
        }

        /// <summary>
        /// Gets the locator for the map
        /// </summary>
        public LocatorTask Locator { get; private set; }

        /// <summary>
        /// Gets the locator info
        /// </summary>
        internal LocatorInfo LocatorInfo { get; private set; }

        /// <summary>
        /// Loads the locator and gets locator info
        /// If the locator is composite and has multiple locators, it gets all of them
        /// Also gets the layers that each locator is associated with, to be used for feature selection
        /// </summary>
        /// <returns>Async task</returns>
        private async Task GetInfoFromLocatorAsync()
        {
            // Load locator and get locator info
            await this.Locator.LoadAsync();
            this.LocatorInfo = this.Locator.LocatorInfo;
        }

        /// <summary>
        /// Gets list of suggested locations from the locator based on user input
        /// </summary>
        /// <param name="userInput">User input</param>
        /// <returns>List of suggestions</returns>
        private async Task GetLocationSuggestionsAsync(string userInput)
        {
            try
            {
                if (this.LocatorInfo.SupportsSuggestions)
                {
                    // restrict the search to return no more than 10 suggestions
                    var suggestParams = new SuggestParameters { MaxResults = 10 };

                    // get suggestions for the text provided by the user
                    this.SuggestionsList = (await this.Locator.SuggestAsync(userInput, suggestParams)).ToList();
                }
            }
            catch
            {
                // If error happens, do not show suggestions
            }
        }

        /// <summary>
        /// Get location searched by user from the locator
        /// </summary>
        /// <param name="searchString">User input</param>
        /// <returns>Location that best matches the search string</returns>
        private async Task GetSearchedLocationAsync(string searchString)
        {
            try
            {
                // Geocode location and return the best match from the list
                var matches = await this.Locator.GeocodeAsync(searchString);
                var bestMatch = matches.FirstOrDefault();

                // Select located feature on map
                // If no feature was located, show a message to the user
                if (bestMatch != null)
                {
                    // Set viewpoint to the feature's extent
                    this.ViewPoint = new Viewpoint(bestMatch.Extent);

                    // Set pin in feature
                    if (this.GraphicsOverlays["PinsGraphicsOverlay"] == null)
                    {
                        this.GraphicsOverlays.Add(new GraphicsOverlay()
                        {
                            Id = "PinsGraphicsOverlay",
                        });
                    }

                    // TODO: replace red circle with pin
                    var graphic = new Graphic(bestMatch.DisplayLocation, new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Colors.Red, 10));
                    this.GraphicsOverlays["PinsGraphicsOverlay"].Graphics.Clear();
                    this.GraphicsOverlays["PinsGraphicsOverlay"].Graphics.Add(graphic);
                }
                else
                {
                    MessageBox.Show(string.Format("{0} was not found", searchString));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("An error occured during your search. Please try again. If error persists, please contact your GIS Administrator"));
            }
        }

        /// <summary>
        /// Gets the info to be displayed about the identified features
        /// </summary>
        /// <param name="identifyResults">List of results returned from the Map View</param>
        private void GetIdentifyInfoAsync(IReadOnlyList<IdentifyLayerResult> identifyResults)
        {
            if (identifyResults != null)
            {
                // Get each result and put them in the IdentifyModelsList
                this.IdentifyModelsList = new ObservableCollection<IdentifyModel>();
                foreach (var result in identifyResults)
                {
                    foreach (var geoelement in result.GeoElements)
                    {
                        // Set the layer name
                        var identifyModel = new IdentifyModel();
                        identifyModel.LayerName = result.LayerContent.Name;

                        identifyModel.Attributes = new Dictionary<string, object>();

                        // Set attribute values
                        // Datetime attributes are being formatted to display date only
                        foreach (var attribute in geoelement.Attributes)
                        {
                            if (attribute.Value is DateTimeOffset)
                            {
                                identifyModel.Attributes.Add(new KeyValuePair<string, object>(attribute.Key, ((DateTimeOffset)attribute.Value).ToString("d")));
                            }
                            else
                            {
                                identifyModel.Attributes.Add(attribute);
                            }
                        }

                        // Add new value to the list
                        this.IdentifyModelsList.Add(identifyModel);

                        // Return after first identify result is added. This insures only the top most result is displayed and selected
                        // TODO: Remove these lines once view is modified to handle multiple results
                        this.SelectFeature(geoelement as Feature, result.LayerContent as FeatureLayer);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Perform feature selection 
        /// </summary>
        /// <param name="feature">Feature to be selected</param>
        /// <param name="featureLayer">Feature layer containing the feature</param>
        private void SelectFeature(Feature feature, FeatureLayer featureLayer)
        {
            // Clear all selected features in all map feature layers
            foreach (var layer in this.Map.OperationalLayers.OfType<FeatureLayer>())
            {
                layer.ClearSelection();
            }

            // Set selection parameters
            featureLayer.SelectionWidth = 5;

            // Select feature
            if (feature != null)
            {
                featureLayer.SelectFeature(feature);
            }
        }

        /// <summary>
        /// Command attached to the button to go back to the main page
        /// </summary>
        private void BackToMainView()
        {
            AppViewModel.Instance.DisplayViewModel = new MainViewModel();
        }
    }
}
