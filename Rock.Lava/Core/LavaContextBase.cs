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

using Rock.Common;

namespace Rock.Lava
{
    public abstract class LavaContextBase : ILavaContext
    {
        public object this[string key]
        {
            get
            {
                return GetMergeFieldValue( key, null );
            }
            set
            {
                SetMergeFieldValue( key, value );
            }
        }

        public abstract List<string> GetEnabledCommands();
        //public abstract IList<LavaDictionary> GetEnvironments();
        //public abstract LavaDictionary GetMergeFieldsInLocalScope();
        //public abstract IDictionary<string, object> GetMergeFieldsInContainerScope();
        //public abstract IDictionary<string, object> GetMergeFieldsInScope();

        public abstract object GetMergeFieldValue( string key, object defaultValue = null );

        public abstract LavaDictionary GetMergeFields();
        //public abstract IList<LavaDictionary> GetScopes();

        public abstract string ResolveMergeFields( string content, IDictionary<string, object> mergeObjects, string enabledLavaCommands = null, bool encodeStrings = false, bool throwExceptionOnErrors = false );
        public abstract void SetEnabledCommands( IEnumerable<string> commands );
        public void SetEnabledCommands( string commandList, string delimiter = "," )
        {
            var commands = commandList.SplitDelimitedValues( delimiter );

            SetEnabledCommands( commands );
        }

        /// <summary>
        /// Sets a named value that is available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetMergeFieldValue( string key, object value )
        {
            SetMergeFieldValue( key, value, null );
        }

        public abstract void SetMergeFieldValue( string key, object value, string scopeSelector );

        public void SetMergeFieldValues( LavaDictionary values )
        {
            SetMergeFieldValues( values as IDictionary<string, object> );
        }

        public virtual void SetMergeFieldValues( IDictionary<string, object> values )
        {
            if ( values == null )
            {
                return;
            }

            foreach ( var kvp in values )
            {
                SetMergeFieldValue( kvp.Key, kvp.Value );
            }
        }

        /// <summary>
        /// Gets a named value that is for internal use only. Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="key"></param>
        public abstract object GetInternalFieldValue( string key, object defaultValue = null );

        /// <summary>
        /// Gets the collection of variables defined for internal use only.  Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        public abstract LavaDictionary GetInternalFields();

        /// <summary>
        /// Sets a named value that is for internal use only. Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public abstract void SetInternalFieldValue( string key, object value );

        public void SetInternalFieldValues( LavaDictionary values )
        {
            SetInternalFieldValues( values as IDictionary<string, object> );
        }

        public virtual void SetInternalFieldValues( IDictionary<string, object> values )
        {
            if ( values == null )
            {
                return;
            }

            foreach ( var kvp in values )
            {
                SetInternalFieldValue( kvp.Key, kvp.Value );
            }
        }

        public void ExecuteInChildScope( Action<ILavaContext> callback )
        {
            EnterChildScope();
            try
            {
                callback( this );
            }
            finally
            {
                ExitChildScope();
            }
        }
        public abstract void EnterChildScope();
        public abstract void ExitChildScope();
    }
}