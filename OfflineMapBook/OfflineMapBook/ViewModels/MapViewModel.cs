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
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Commands;
    using Esri.ArcGISRuntime.Data;
    using Esri.ArcGISRuntime.Mapping;
    using Esri.ArcGISRuntime.Tasks.Geocoding;
    using System.Windows;

    /// <summary>
    /// View model performs logic related to the map screen
    /// </summary>
    internal class MapViewModel : BaseViewModel
    {
        private Map map;
        private string searchText;
        private List<SuggestResult> suggestionsList;
        private Viewpoint viewPoint;
        private ICommand backCommand;
        private ICommand searchCommand;
        private ICommand identifyCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapViewModel"/> class.
        /// </summary>
        /// <param name="map">Current map</param>
        public MapViewModel(Map map)
        {
            this.Map = map;
            this.Locator = AppViewModel.Instance.Mmpk.LocatorTask;
            this.GetInfoFromLocatorAsync();
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
                if (this.viewPoint != value)
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
                    async (x) =>
                {
                    await this.GetSearchedLocationAsync((string)x);
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
                    async (x) =>
                {
                    await this.GetIdentifyInfoAsync((IReadOnlyList<IdentifyLayerResult>)x);
                }, true));
            }
        }

        /// <summary>
        /// Gets the list of layers used in the locator
        /// </summary>
        internal Dictionary<string, string> LocatorLayers { get; private set; }

        /// <summary>
        /// Loads the locator and gets locator info
        /// If the locator is composite and has multiple locators, it gets all of them
        /// Also gets the layers that each locator is associated with, to be used for feature selection
        /// </summary>
        /// <returns>Async task</returns>
        private async Task GetInfoFromLocatorAsync()
        {
            // Load locator
            await this.Locator.LoadAsync();
            this.LocatorInfo = this.Locator.LocatorInfo;

            // Get list of all the locators
            var locatorProperties = this.LocatorInfo.Properties;
            var locatorNames = locatorProperties["CL.Locator"].Split('|').ToList();

            // Get layer associated with each locator
            this.LocatorLayers = new Dictionary<string, string>();
            foreach (var locatorName in locatorNames)
            {
                var layerName = locatorProperties[string.Format("CL.{0}.Name", locatorName)];
                this.LocatorLayers.Add(locatorName, layerName);
            }
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
                    await this.SelectLocatedFeature(bestMatch);
                }
                else
                {
                    MessageBox.Show(string.Format("{0} was not found"));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("An error occured during your search. Please try again. If error persists, please contact yoru GIS Administrator"));
            }
        }

        /// <summary>
        /// Select the located feature
        /// </summary>
        /// <param name="locatedFeature">Located feature</param>
        /// <returns>Async task</returns>
        private async Task SelectLocatedFeature(GeocodeResult locatedFeature)
        {
            // Get the locator that was used to find the feature
            var locatorName = locatedFeature.Attributes["Loc_name"];
            var locatorLayer = this.LocatorLayers[locatorName.ToString()];

            // Get the layer from the map matching the locator layer
            // Remove spaces from the layer names coming from the map
            // This is to account for discrepancy between layer names in the map with spaces and layer names in the locator without spaces
            var layers = from lyr in this.Map.OperationalLayers where lyr.Name.Replace(" ", string.Empty) == locatorLayer select lyr;
            var layer = layers.First();

            // Test to make sure layer is feature layer
            if (layer is FeatureLayer)
            {
                // Get feature table to perform the query
                var featureTable = ((FeatureLayer)layer).FeatureTable;

                // Set up query parameters using geometry intersection to find the located feature
                // This is because the locator does not provide the searched field and we cannot do an attribute query
                var queryParams = new QueryParameters()
                {
                    Geometry = locatedFeature.DisplayLocation,
                    SpatialRelationship = SpatialRelationship.Intersects,
                    ReturnGeometry = true,
                };

                // Run the query
                var queryResult = await featureTable.QueryFeaturesAsync(queryParams);

                // Handle the returned results
                if (queryResult.Count() == 0)
                {
                    // If no feature is returned, just show user what the locator returned
                    // TODO: Zoom to location and place a pin in it
                }
                else if (queryResult.Count() == 1)
                {
                    // Select found feature
                    this.SelectAndZoomToFeature(queryResult.FirstOrDefault(), featureTable.FeatureLayer);
                }
                else
                {
                    // If multiple results return, check the attributes to find the searched feature
                    foreach (var feature in queryResult)
                    {
                        foreach (var attribute in feature.Attributes)
                        {
                            if (attribute.Value.ToString() == locatedFeature.Label)
                            {
                                // When an attribute matching the search string is found
                                // Select feature and exit
                                this.SelectAndZoomToFeature(feature, featureTable.FeatureLayer);
                                return;
                            }
                        }

                        // If no matching features were found, just show user what the locator returned
                        // TODO: Zoom to location and place a pin in it
                    }
                }
            }
        }

        /// <summary>
        /// Perform feature selection and create viewpoint around feature
        /// </summary>
        /// <param name="feature">Feature to be selected</param>
        /// <param name="featureLayer">Feature layer containing the feature</param>
        private void SelectAndZoomToFeature(Feature feature, FeatureLayer featureLayer)
        {
            // Clear all selected features in all map feature layers
            foreach (var layer in this.Map.OperationalLayers)
            {
                if (layer is FeatureLayer)
                {
                    ((FeatureLayer)layer).ClearSelection();
                }
            }

            // Set selection parameters
            featureLayer.SelectionWidth = 5;

            // Select feature
            featureLayer.SelectFeature(feature);

            // Set viewpoint to the feature's center point, and a zoom scale of 300
            this.ViewPoint = new Viewpoint(feature.Geometry.Extent.GetCenter(), 300);

            // Turn on the feature layer if it is not, otherwise the feature selection will not be visible
            if (featureLayer.IsVisible == false)
            {
                featureLayer.IsVisible = true;
            }
        }

        /// <summary>
        /// Gets the info to be displayed about the identified features
        /// </summary>
        /// <param name="identifyResult">List of results returned from the Map View</param>
        /// <returns>Async Task</returns>
        private async Task GetIdentifyInfoAsync(IReadOnlyList<IdentifyLayerResult> identifyResults)
        {
            var x = identifyResults[0].Popups;
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
