using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using OfflineMapBook.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OfflineMapBook.ViewModels
{
    class MapViewModel : BaseViewModel
    {
        private Map map;
        private string searchText;
        private List<SuggestResult> suggestionsList;
        private Viewpoint viewPoint;
        public LocatorTask Locator { get; private set; }

        public Map Map
        {
            get
            {
                return map;
            }

            set
            {
                if (map != value)
                {
                    map = value;
                    this.OnPropertyChanged(nameof(this.Map));
                }
            }
        }

        public string SearchText
        {
            get
            {
                return searchText;
            }
            set
            {
                if (searchText != value)
                {
                    searchText = value;
                    this.OnPropertyChanged(nameof(this.SearchText));
                    GetLocationSuggestionsAsync(this.SearchText);
                }
            }
        }

        public List<SuggestResult> SuggestionsList
        {
            get
            {
                return suggestionsList;
            }
            private set
            {
                if (suggestionsList != value)
                {
                    suggestionsList = value;
                    this.OnPropertyChanged(nameof(this.SuggestionsList));
                }
            }
        }

        public Viewpoint ViewPoint
        {
            get
            {
                return viewPoint;
            }

            set
            {
                if (viewPoint != value)
                {
                    viewPoint = value;
                    this.OnPropertyChanged(nameof(this.ViewPoint));
                }
            }
        }

        internal LocatorInfo LocatorInfo { get; private set; }

        internal Dictionary<string, string> LocatorLayers { get; private set; }

        public MapViewModel(Map map, LocatorTask locator)
        {
            this.Map = map;
            this.Locator = locator;
            GetInfoFromLocator();
        }

        private async Task GetInfoFromLocator()
        {
            await this.Locator.LoadAsync();
            this.LocatorInfo = this.Locator.LocatorInfo;

            // Get list of layers in the locator
            var locatorProperties = this.LocatorInfo.Properties;
            var locatorNames = locatorProperties["CL.Locator"].Split('|').ToList();

            LocatorLayers = new Dictionary<string, string>();
            foreach (var locatorName in locatorNames)
            {
                var layerName = locatorProperties[string.Format("CL.{0}.Name", locatorName)];
                LocatorLayers.Add(locatorName, layerName);
            }
        }

        internal async Task GetLocationSuggestionsAsync(string userInput)
        {
            try
            {   
                var l = LocatorInfo.IntersectionResultAttributes;
                if (LocatorInfo.SupportsSuggestions)
                {
                    // restrict the search to return no more than 10 suggestions
                    var suggestParams = new SuggestParameters { MaxResults = 10 };

                    // get suggestions for the text provided by the user
                    SuggestionsList = (await this.Locator.SuggestAsync(userInput, suggestParams)).ToList() ;
                }
            }
            catch
            {
                // If error happens, do not show suggestions
            }
        }

        internal async Task GetSearchedLocationAsync(string searchString)
        {
            try
            {
                // Geocode location and return the best match from the list
                var matches = await this.Locator.GeocodeAsync(searchString);
                var bestMatch = matches.FirstOrDefault();

                // Select located feature on map
                await SelectLocatedFeature(bestMatch);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private async Task SelectLocatedFeature(GeocodeResult locatedFeature)
        {
            var locatorName = locatedFeature.Attributes["Loc_name"];
            var locatorLayer = LocatorLayers[locatorName.ToString()];

            var layers = from lyr in Map.OperationalLayers where (lyr.Name).Replace(" ", "") == locatorLayer select lyr;
            var layer = layers.First();
            if (layer is FeatureLayer)
            {
                var featureTable = ((FeatureLayer)layer).FeatureTable;
                var queryParams = new QueryParameters()
                {
                    Geometry = locatedFeature.DisplayLocation,
                    SpatialRelationship = SpatialRelationship.Intersects,
                    ReturnGeometry = true,
                    WhereClause = "1=1"
                };
                var queryResult = await featureTable.QueryFeaturesAsync(queryParams);

                if (queryResult.Count() == 0)
                {
                    // TODO: Do nothing, tell the user feature wasn't found? 
                }
                else if (queryResult.Count() == 1)
                {
                    // Select feature
                    SelectAndZoomToFeature(queryResult.FirstOrDefault(), featureTable.FeatureLayer);
                }
                else
                {
                    // if multiple results return, check the attributes to find the searched feature
                    foreach (var feature in queryResult)
                    {
                        foreach (var attribute in feature.Attributes)
                        {
                            if (attribute.Value.ToString() == locatedFeature.Label)
                            {
                                // Select feature
                                SelectAndZoomToFeature(feature, featureTable.FeatureLayer);
                                return;
                            }
                        }
                    }
                }

            }
        }

        private void SelectAndZoomToFeature(Feature feature, FeatureLayer featureLayer)
        {
            // Select feature, zoom to it, and turn layer on if it's off
            foreach (var layer in Map.OperationalLayers)
            {
                if (layer is FeatureLayer)
                {
                    ((FeatureLayer)layer).ClearSelection();
                }
            }
            featureLayer.SelectionWidth = 5;
            featureLayer.SelectFeature(feature);         
            ViewPoint = new Viewpoint(feature.Geometry.Extent.GetCenter(), 300);
            if (featureLayer.IsVisible == false)
            {
                featureLayer.IsVisible = true;
            }
        }


        private ICommand _clickCommand;
        public ICommand ClickCommand
        {
            get
            {
                return _clickCommand ?? (_clickCommand = new SimpleCommand(() => BackCommand(), true));

            }
        }

        /// <summary>
        /// Command attached to the button to go back to the main page
        /// </summary>
        public void BackCommand()
        {
            AppViewModel.Instance.DisplayViewModel = new MainViewModel();
        }

        private ICommand _searchCommand;
        public ICommand SearchCommand
        {
            get
            {
                return _searchCommand ?? (_searchCommand = new ParameterCommand(async (x) => 
                {
                    await GetSearchedLocationAsync((string)x);
                    }, true));
            }
        }
    }
}
