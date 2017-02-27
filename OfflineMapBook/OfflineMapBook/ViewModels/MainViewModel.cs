using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using OfflineMapBook.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OfflineMapBook.ViewModels
{
    class MainViewModel : BaseViewModel
    {
        private string mmpkName;
        private string mmpkThumbnail;
        private string mmpkDescription;
        private IReadOnlyList<Map> mapItems;
        private Map map;
        private LocatorTask locator;
      

        public IReadOnlyList<Map> MapItems
        {
            get
            {
                return mapItems;
            }

            set
            {
                mapItems = value;
                this.OnPropertyChanged(nameof(this.MapItems));
            }
        }

        public Esri.ArcGISRuntime.Portal.Item MmpkItem { get; set; }

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

        public LocatorTask Locator
        {
            get
            {
                return locator;
            }

            set
            {
                if (locator != value)
                {
                    locator = value;
                    this.OnPropertyChanged(nameof(this.Locator));
                }
            }
        }


        public MainViewModel()
        {
            InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            MmpkItem = AppViewModel.Instance.Mmpk.Item;
            MapItems = AppViewModel.Instance.Mmpk.Maps;
            Locator = AppViewModel.Instance.Mmpk.LocatorTask;
            }

        private ICommand _clickCommand;
        public ICommand ClickCommand
        {
            get
            {
                return _clickCommand ?? (_clickCommand = new ParameterCommand((x) => SubmitButtonAction((Map)x), true));
            }
        }

        private bool _canExecute;

        public void SubmitButtonAction(Map map)
        {
            AppViewModel.Instance.DisplayViewModel = new MapViewModel(map, locator);
        }
    }
}
