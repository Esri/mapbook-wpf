using Esri.ArcGISRuntime.UI;
using System;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.IO;

namespace OfflineMapBook.Converters
{
    class RuntimeImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
           System.Globalization.CultureInfo culture)
        {
            if (value is RuntimeImage)
            {
                return GetImageAsync(value as RuntimeImage).Result;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
          System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                return !(bool)value;
            }
            return value;
        }

        private async Task<BitmapImage> GetImageAsync(RuntimeImage rtImage)
        {
            if (rtImage.LoadStatus != Esri.ArcGISRuntime.LoadStatus.Loaded)
            {
                await rtImage.LoadAsync();
            }
            var stream = await rtImage.GetEncodedBufferAsync();
            try
            {
                var image = new BitmapImage();
              
                image.BeginInit();
                stream.Seek(0, SeekOrigin.Begin);
                image.StreamSource = stream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                return image;
            }
            catch (Exception ex)
            {

                return null;
            }

            
        }
    }
}
