using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using OfflineMapBook.ViewModels;

namespace OfflineMapBook
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            if (AppViewModel.Instance == null)
            {
                LoadMmpkAsync();
            }           
        }

        internal async Task LoadMmpkAsync()
        {
            var mmpk = await MobileMapPackage.OpenAsync(@"C:\Users\mara8799\Downloads\OfflineMapbook_v3.mmpk");
            AppViewModel.Instance = AppViewModel.Create(mmpk);
            this.DataContext = AppViewModel.Instance;
            AppViewModel.Instance.DisplayViewModel = new MainViewModel();
        }
    }
}
