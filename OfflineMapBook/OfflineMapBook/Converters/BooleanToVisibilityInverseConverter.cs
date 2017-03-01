// <copyright file="BooleanToVisibilityInverseConverter.cs" company="Esri">
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
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Converts boolean to visibility by inverting values so false is visible and true is not visible
    /// </summary>
    public class BooleanToVisibilityInverseConverter : IValueConverter
    {

        /// <summary>
        /// Convert method
        /// </summary>
        /// <param name="value">Converted value</param>
        /// <param name="targetType">Target type</param>
        /// <param name="parameter">Parameter object</param>
        /// <param name="culture">Culture Info</param>
        /// <returns>Visibility status</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(Visibility))
            {
                var visible = System.Convert.ToBoolean(value, culture);
                visible = !visible;
                return visible ? Visibility.Visible : Visibility.Collapsed;
            }

            throw new InvalidOperationException("Converter can only convert to value of type Visibility.");
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
