using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OfflineMapBook.Converters
{
    internal class IndexToImageConverter : IValueConverter
    {
        /// <summary>
        /// Convert a RuntimeImage to a BitmapImage
        /// </summary>
        /// <param name="value">Converted value</param>
        /// <param name="targetType">Target type</param>
        /// <param name="parameter">Parameter object</param>
        /// <param name="culture">Culture Info</param>
        /// <returns>Visibility status</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int && value != null)
            {
                return string.Format("/Images/MapThumbnail{0}.png", value);
            }

            return value;
        }

        /// <summary>
        /// Convert back method
        /// </summary>
        /// <param name="value">Converted value</param>
        /// <param name="targetType">Target type</param>
        /// <param name="parameter">Parameter object</param>
        /// <param name="culture">Culture Info</param>
        /// <returns>Visibility status</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("Converter cannot convert back.");
        }
    }
}
