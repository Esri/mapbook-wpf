// <copyright file="RuntimeImageConverter.cs" company="Esri">
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

namespace OfflineMapBook.Converters
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;
    using Esri.ArcGISRuntime.UI;

    /// <summary>
    /// Converts RuntimeImage to BitmapImage
    /// </summary>
    internal class RuntimeImageConverter : IValueConverter
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
            if (value is RuntimeImage && value != null)
            {
                return this.GetImageAsync(value as RuntimeImage).Result;
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

        /// <summary>
        /// Function to get BitmapImage from RuntimeImage
        /// </summary>
        /// <param name="rtImage">Runtime Image</param>
        /// <returns>Bitmap Image</returns>
        private async Task<BitmapImage> GetImageAsync(RuntimeImage rtImage)
        {
            if (rtImage.LoadStatus != Esri.ArcGISRuntime.LoadStatus.Loaded)
            {
                await rtImage.LoadAsync();
            }

            try
            {
                var stream = await rtImage.GetEncodedBufferAsync();
                var image = new BitmapImage();
                image.BeginInit();
                stream.Seek(0, SeekOrigin.Begin);
                image.StreamSource = stream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                return image;
            }
            catch
            {
                return null;
            }
        }
    }
}
