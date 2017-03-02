// <copyright file="IdentifyModel.cs" company="Esri">
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
namespace OfflineMapBook
{
    using System.Collections.Generic;

    /// <summary>
    /// Holds the Identify information for a feature
    /// </summary>
    internal class IdentifyModel
    {
        private string layerName;
        private IDictionary<string, object> attributes;

        /// <summary>
        /// Gets or sets the name of the layer identified
        /// </summary>
        public string LayerName { get; set; }

        /// <summary>
        /// Gets or sets the attributes of the feature identified
        /// </summary>
        public IDictionary<string, object> Attributes { get; set; }
    }
}
