using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OfflineMapBook.ViewModels
{
    sealed class AppViewModel : BaseViewModel
    {
        private BaseViewModel _displayViewModel;

        public MobileMapPackage Mmpk { get; private set; }

        public BaseViewModel DisplayViewModel
        {
            get { return _displayViewModel; }
            set
            {
                _displayViewModel = value;
                OnPropertyChanged("DisplayViewModel");
            }
        }


        public static AppViewModel Instance { get; set; }

        internal static AppViewModel Create(MobileMapPackage mmpk)
        {
            var appViewModel = new AppViewModel();
            appViewModel.Mmpk = mmpk;
            
            return appViewModel;
        }


    }
}
