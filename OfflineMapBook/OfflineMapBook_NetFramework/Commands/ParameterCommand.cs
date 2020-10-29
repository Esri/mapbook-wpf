// <copyright file="ParameterCommand.cs" company="Esri">
//      Copyright (c) 2017 Esri. All rights reserved.
//
//      Licensed under the Apache License, Version 2.0 (the "License");
//      you may not use this file except in compliance with the License.
//      You may obtain a copy of the License at
//
//      https://www.apache.org/licenses/LICENSE-2.0
//
//      Unless required by applicable law or agreed to in writing, software
//      distributed under the License is distributed on an "AS IS" BASIS,
//      WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//      See the License for the specific language governing permissions and
//      limitations under the License.
// </copyright>
// <author>Mara Stoica</author>

namespace Esri.ArcGISRuntime.OpenSourceApps.OfflineMapBook.Commands
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// Generic command with parameter object
    /// </summary>
    internal class ParameterCommand : ICommand
    {
        private Action<object> action;
        private bool canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterCommand"/> class.
        /// </summary>
        /// <param name="action">Action of the command</param>
        /// <param name="canExecute">Determines whether command is enabled or not</param>
        public ParameterCommand(Action<object> action, bool canExecute)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Fires if can execute changes
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Sets whether command is enabled or not
        /// </summary>
        /// <param name="parameter">Command parameter</param>
        /// <returns>True or false</returns>
        public bool CanExecute(object parameter)
        {
            return this.canExecute;
        }

        /// <summary>
        /// Execute method for the command
        /// </summary>
        /// <param name="parameter">Command Parameter</param>
        public void Execute(object parameter)
        {
            this.action?.Invoke(parameter);
        }
    }
}