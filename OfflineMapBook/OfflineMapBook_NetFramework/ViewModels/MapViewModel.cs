// <copyright file="MapViewModel.cs" company="Esri">
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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
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
        private ICommand clearCommand;
        private ICommand searchCommand;
        private ICommand identifyCommand;
        private ICommand zoomToBookmarkCommand;
        private ICommand movePreviousCommand;
        private ICommand moveNextCommand;
        private ICommand closeIdentifyCommand;
        private IdentifyModel currentIdentifiedFeature;
        private int currentIdentifiedFeatureIndex;
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
            _ = this.GetInfoFromLocatorAsync();
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
                    _ = this.GetLocationSuggestionsAsync(this.SearchText);
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
        /// Gets the feature currently displayed as the actively identified feature
        /// </summary>
        public IdentifyModel CurrentIdentifiedFeature
        {
            get
            {
                return this.currentIdentifiedFeature;
            }

            private set
            {
                if (this.currentIdentifiedFeature != value && value != null)
                {
                    this.currentIdentifiedFeature = value;
                    this.OnPropertyChanged(nameof(this.CurrentIdentifiedFeature));
                    this.SelectFeature(this.CurrentIdentifiedFeature.IdentifiedFeature);
                }
            }
        }

        /// <summary>
        /// Gets the index of the actively identified feature to be used to display to user and for navigation to previous and next features
        /// </summary>
        public int CurrentIdentifiedFeatureIndex
        {
            get
            {
                return this.currentIdentifiedFeatureIndex;
            }

            private set
            {
                if (this.currentIdentifiedFeatureIndex != value && this.IdentifyModelsList.ElementAt(value - 1) != null)
                {
                    this.currentIdentifiedFeatureIndex = value;
                    this.OnPropertyChanged(nameof(this.CurrentIdentifiedFeatureIndex));
                    this.CurrentIdentifiedFeature = this.IdentifyModelsList[this.currentIdentifiedFeatureIndex - 1];
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
        /// Gets the command to go back to the main screen
        /// </summary>
        public ICommand ClearCommand
        {
            get
            {
                return this.clearCommand ?? (this.clearCommand = new SimpleCommand(() => this.ClearGraphicsAndSelections(), true));
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
                    (x) => _ = this.GetSearchedLocationAsync((string)x), true));
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
        /// Gets the command to move to the previously identified feature
        /// </summary>
        public ICommand MovePrevious
        {
            get
            {
                return this.movePreviousCommand ?? (this.movePreviousCommand = new SimpleCommand(() => this.NavigateIdentifiedFeatures(-1), true));
            }
        }

        /// <summary>
        /// Gets the command to move to the next identified feature
        /// </summary>
        public ICommand MoveNext
        {
            get
            {
                return this.moveNextCommand ?? (this.moveNextCommand = new SimpleCommand(() => this.NavigateIdentifiedFeatures(1), true));
            }
        }

        /// <summary>
        /// Gets the locator for the map
        /// </summary>yea
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

                    var mapPin = new PictureMarkerSymbol(new Uri("pack://application:,,,/Esri.ArcGISRuntime.OpenSourceApps.OfflineMapBook;component/Resources/MapPin.png"));

                    // Set marker size and offset so the tip of the pin points to the feature
                    // TODO: Remove workaround when Anchoring functionality becomes available
                    mapPin.Height = Properties.Settings.Default.MapPinHeight;
                    mapPin.Width = Properties.Settings.Default.MapPinHeight / 2;
                    mapPin.OffsetY = mapPin.Height / 2;

                    var graphic = new Graphic(bestMatch.DisplayLocation, mapPin);
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
                MessageBox.Show(
                    string.Format("An error occured during your search: {0} Please try again. If error persists, please contact your GIS Administrator", ex.Message),
                    "Error During Search",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Gets the info to be displayed about the identified features
        /// </summary>
        /// <param name="identifyResults">List of results returned from the Map View</param>
        private void GetIdentifyInfoAsync(IReadOnlyList<IdentifyLayerResult> identifyResults)
        {
            // Current IdentifyCommand only handles FeatureLayer identified results (under GeoElements)
            // Developers to handle other types of identified results
            // ArcGISMapImageLayer would have results in SublayerResults.
            if (identifyResults != null)
            {
                // Get each result and put them in the IdentifyModelsList
                this.IdentifyModelsList = new ObservableCollection<IdentifyModel>();
                foreach (var result in identifyResults)
                {
                    foreach (var geoelement in result.GeoElements)
                    {
                        if (geoelement is Feature)
                        {
                            // Set the layer name
                            var identifyModel = new IdentifyModel
                            {
                                LayerName = result.LayerContent.Name,

                                IdentifiedFeature = (Feature)geoelement
                            };

                            // Add new value to the list
                            this.IdentifyModelsList.Add(identifyModel);
                        }
                    }

                    // Set first feature as the current feature and set the index to use in the UI
                    if (this.IdentifyModelsList.Count > 0)
                    {
                        this.CurrentIdentifiedFeature = this.IdentifyModelsList[0];
                        this.CurrentIdentifiedFeatureIndex = 1;
                    }
                }
            }
        }

        /// <summary>
        /// Perform feature selection 
        /// </summary>
        /// <param name="feature">Feature to be selected</param>
        private void SelectFeature(Feature feature)
        {
            if (feature?.FeatureTable.Layer is FeatureLayer featureLayer)
            {
                // Clear all selected features in all map feature layers
                foreach (var layer in this.Map.OperationalLayers.OfType<FeatureLayer>())
                {
                    try
                    {
                        layer.ClearSelection();
                    }
                    catch { }
                }

                // Select feature
                featureLayer.SelectFeature(feature);
            }
        }

        private void NavigateIdentifiedFeatures(int direction)
        {
            var index = this.CurrentIdentifiedFeatureIndex - 1;
            var newIndex = index + direction;

            // if we were already at the first feature, loop back around to the end
            // if we are already at the last feature, loop back to the beginning
            if (newIndex < 0)
            {
                this.CurrentIdentifiedFeatureIndex = this.IdentifyModelsList.Count();
            }
            else if (newIndex == this.IdentifyModelsList.Count())
            {
                this.CurrentIdentifiedFeatureIndex = 1;
            }
            else
            {
                this.CurrentIdentifiedFeatureIndex = newIndex + 1;
            }
        }

        /// <summary>
        /// Command attached to the button to go back to the main page
        /// </summary>
        private void BackToMainView()
        {
            AppViewModel.Instance.DisplayViewModel = new MainViewModel();
        }

        /// <summary>
        /// Command attached to the Clear button to clear map pins and selections
        /// </summary>
        private void ClearGraphicsAndSelections()
        {
            foreach (var graphicsOverlay in this.GraphicsOverlays)
            {
                graphicsOverlay.Graphics.Clear();
            }

            foreach (var featureLayer in this.Map.OperationalLayers.OfType<FeatureLayer>())
            {
                featureLayer.ClearSelection();
            }

            this.IdentifyModelsList.Clear();
            this.SearchText = string.Empty;
        }
    }
}
