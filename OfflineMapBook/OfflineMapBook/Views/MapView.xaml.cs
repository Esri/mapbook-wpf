using OfflineMapBook.ViewModels;
using System.Windows.Controls;

namespace OfflineMapBook
{
   
    /// <summary>
    /// Interaction logic for MapView.xaml
    /// </summary>
    public partial class MapView : UserControl
    {
        MapViewModel ViewModel { get; set; }
        public MapView()
        {
            InitializeComponent();
            this.DataContextChanged += MapView_DataContextChanged;            
        }

        private void MapView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null)
            {
                ViewModel = this.DataContext as MapViewModel;
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private async void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
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
