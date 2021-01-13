// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System;
using System.Collections.Generic;

namespace Rock.Lava
{
    /// <summary>
    /// Represents the configuration and data used by the Lava Engine to resolve a Lava template.
    /// </summary>
    public interface ILavaContext
    {
        /// <summary>
        /// Gets a named value that is for internal use only. Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="key"></param>
        object GetInternalFieldValue( string key );

        /// <summary>
        /// Gets the collection of variables defined for internal use only.  Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        LavaDictionary GetInternalFields();

        /// <summary>
        /// Sets a named value that is for internal use only. Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetInternalFieldValue( string key, object value );

        /// <summary>
        /// Sets a collection of named values for internal use only. Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="values"></param>
        void SetInternalFieldValues( IDictionary<string, object> values );

        /// <summary>
        /// Sets a collection of named values for internal use only. Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="values"></param>
        void SetInternalFieldValues( LavaDictionary values );

        /// <summary>
        /// The set of Lava Commands that are enabled for this context.
        /// </summary>
        List<string> GetEnabledCommands();

        /// <summary>
        /// Sets the list of Lava commands enabled for this template.
        /// </summary>
        /// <param name="commands"></param>
        void SetEnabledCommands( IEnumerable<string> commands );

        /// <summary>
        /// Sets the list of Lava commands enabled for this template.
        /// </summary>
        /// <param name="commandList">A delimited list of command names.</param>
        /// <param name="delimiter">The list delimiter.</param>
        void SetEnabledCommands( string commandList, string delimiter = "," );

        /// <summary>
        /// Gets the value of a field that is accessible for merging into a template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        object GetMergeFieldValue( string key, object defaultValue );

        /// <summary>
        /// Gets the collection of user-defined variables in the current context that are accessible in a template.
        /// </summary>
        LavaDictionary GetMergeFields();

        /// <summary>
        /// Sets the value of a field that is accessible for merging into a template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        void SetMergeFieldValue( string key, object value );

        /// <summary>
        /// Sets the value of a field that is accessible for merging into a template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="scopeSelector>"root|parent|current", or the index number of a scope in the current stack.</param>
        /// <returns></returns>
        void SetMergeFieldValue( string key, object value, string scopeSelector );

        /// <summary>
        /// Sets a collection of user-defined variables in the current context that are internally available to custom filters and tags.
        /// </summary>
        /// <param name="values"></param>
        void SetMergeFieldValues( IDictionary<string, object> values );

        /// <summary>
        /// Get or set the value of a field that is accessible for merging into a template.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object this[string key] { get; set; }

        /// <summary>
        /// Executes the specified action in a new child scope.
        /// </summary>
        /// <param name="callback"></param>
        void ExecuteInChildScope( Action<ILavaContext> callback );

        /// <summary>
        /// Creates a new child scope. Values added to the child scope will be released once
        /// <see cref="ExitChildScope" /> is called. Values in the parent scope remain available to the child scope.
        /// </summary>
        void EnterChildScope();

        /// <summary>
        /// Exits the current scope that has been created by <see cref="EnterChildScope" />
        /// </summary>
        void ExitChildScope();

        /// <summary>
        /// Use the information in this context to resolve the specified Lava template.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="mergeObjects"></param>
        /// <param name="enabledLavaCommands"></param>
        /// <param name="encodeStrings"></param>
        /// <param name="throwExceptionOnErrors"></param>
        /// <returns></returns>
[Obsolete("Should use LavaEngine.TryRender instead?")]
        string ResolveMergeFields( string content, IDictionary<string, object> mergeObjects, string enabledLavaCommands = null, bool encodeStrings = false, bool throwExceptionOnErrors = false );

    }
}
