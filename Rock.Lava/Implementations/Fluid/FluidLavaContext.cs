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
using System.Linq;
using Fluid;
using Fluid.Values;
using Rock.Common;
using Rock.Lava.DotLiquid;

namespace Rock.Lava.Fluid
{
    public class FluidLavaContext : LavaContextBase
    {
        private TemplateContext _context;

        public FluidLavaContext( TemplateContext context )
        {
            _context = context;
        }

        public TemplateContext FluidContext
        {
            get
            {
                return _context;
            }
        }

        //public object this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        //public IList<IDictionary<string, object>> Environments
        //{
        //    get
        //    {
        //        return null;
        //        //var environments = new List<IDictionary<string, object>>();

        //        //foreach ( var hash in _context.Environments )
        //        //{
        //        //    environments.Add( hash as IDictionary<string, object> );
        //        //}

        //        //return environments;
        //    }
        //}

        //public IList<IDictionary<string, object>> Scopes
        //{
        //    get
        //    {
        //        return null;
        //        //var environments = new List<IDictionary<string, object>>();

        //        //foreach ( var hash in _context.Scopes )
        //        //{
        //        //    environments.Add( hash as IDictionary<string, object> );
        //        //}

        //        //return environments;
        //    }

        //}

        public override List<string> GetEnabledCommands()
        {
            // The set of enabled Lava Commands is stored in the Fluid AmbientValues collection.
            if ( _context.AmbientValues?.ContainsKey( "EnabledCommands" ) == true )
            {
                return _context.AmbientValues["EnabledCommands"].ToString().Split( ",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries ).ToList();
            }

            return new List<string>();
        }

        //public override LavaDictionary GetMergeFieldsInLocalScope()
        //{
        //    var fields = new LavaDictionary();

        //    // First, get all of the variables defined in the local lava context
        //    var properties = GetScopeDefinedValues( _context.LocalScope );

        //    foreach ( var item in properties )
        //    {
        //        //foreach ( var item in scope )
        //        //{
        //            fields.AddOrReplace( item.Key, item.Value );
        //        //}
        //    }

        //    // Second, apply overrides defined by the block or container in which the template is being resolved.
        //    //foreach ( var environment in _context.AmbientValues )
        //    //{
        //    foreach ( var item in _context.AmbientValues )
        //    {
        //        fields.AddOrReplace( item.Key, item.Value );
        //    }
        //    //}

        //    return fields;
        //}
        //public override IDictionary<string, object> GetMergeFieldsInContainerScope()
        //{
        //    return GetMergeFieldsInLocalScope();
        //    //throw new NotImplementedException();
        //}

        //public IDictionary<string, object> GetMergeFieldsInEnvironment()
        //{
        //    // TODO: Not possible in Fluid unless we use reflection???


        //    // get merge fields loaded by the block or container
        //    var internalMergeFields = new Dictionary<string, object>();

        //    //if ( _context.LocalScope. .AmbientValues _context.AmbientValues.. .Environments.Count > 0 )
        //    //{
        //    //    foreach ( var item in _context.Environments[0] )
        //    //    {
        //    //        internalMergeFields.AddOrReplace( item.Key, item.Value );
        //    //    }
        //    //}

        //    return internalMergeFields;
        //}
        //public override IDictionary<string, object> GetMergeFieldsInScope()
        //{
        //    return new Dictionary<string, object>( _context.AmbientValues );
        //}

        public override object GetMergeFieldValue( string key, object defaultValue )
        {
            if ( !_context.AmbientValues.ContainsKey( key ) )
            {
                return defaultValue;
            }

            return _context.AmbientValues[key];

        }

        public override LavaDictionary GetMergeFields()
        {
            var dictionary = new LavaDictionary( this.GetScopeAggregatedValues( _context.LocalScope ) );

            return dictionary;
        }

        /// <summary>
        /// Render a specified Lava template in the current context.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="mergeObjects"></param>
        /// <param name="enabledLavaCommands"></param>
        /// <param name="encodeStrings"></param>
        /// <param name="throwExceptionOnErrors"></param>
        /// <returns></returns>
        public override string ResolveMergeFields( string content, IDictionary<string, object> mergeObjects, string enabledLavaCommands = null, bool encodeStrings = false, bool throwExceptionOnErrors = false )
        {
            try
            {
                // 7-9-2020 JME  / NA
                // We decided to remove the check for lava merge fields here as this method is specifically
                // made to resolve them. The performance increase for text without lava is acceptable as in
                // a vast majority of cases the string will have lava (that's what this method is for). In
                // these cases there is a performance tax (though small) on the vast majority of calls.

                // If there have not been any EnabledLavaCommands explicitly set, then use the global defaults.
                if ( enabledLavaCommands == null )
                {
                    // TODO:    
                    //enabledLavaCommands = GlobalAttributesCache.Value( "DefaultEnabledLavaCommands" );
                }

                var fluidTemplate = GetTemplate( content );

                //var context = new TemplateContext();

                // Add the merge objects to the context.
                if ( mergeObjects != null )
                {
                    foreach ( var key in mergeObjects.Keys )
                    {
                        _context.LocalScope.SetValue( key, mergeObjects[key] );
                    }
                }

                _context.LocalScope.SetValue( "EnabledCommands", enabledLavaCommands );


                //context.AmbientValues = new Dictionary<string, object>( mergeObjects );
                //template.InstanceAssigns.AddOrReplace( "CurrentPerson", currentPersonOverride );

                //context.Model = mergeObjects;

                //var scope = context.LocalScope;

                //context.SetValue()

                var output = fluidTemplate.Render( _context );

                return output;
            }
            catch ( Exception ex )
            {
                throw ex;
                //ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );
                //return "Error resolving Lava merge fields: " + ex.Message;
            }
        }

        //public override string ResolveMergeFields( string content, IDictionary<string, object> mergeObjects )
        //{
        //    var enabledCommands = string.Empty; // GlobalAttributesCache.Get().GetValue( "DefaultEnabledLavaCommands" );
        //    return ResolveMergeFields( content, mergeObjects, enabledCommands );
        //}

        //public override string ResolveMergeFields( string content, IDictionary<string, object> mergeObjects, string enabledLavaCommands, bool encodeStrings = false, bool throwExceptionOnErrors = false )
        //{
        //    throw new NotImplementedException();
        //}

        //public override string ResolveMergeFields( string content, IDictionary<string, object> mergeObjects )
        //{
        //    throw new NotImplementedException();
        //}

        //public void SetEnabledCommands( IEnumerable<string> commands )
        //{
        //    throw new NotImplementedException();
        //}

        //public void SetEnabledCommands( string commandList, string delimiter = "," )
        //{
        //    throw new NotImplementedException();
        //}

        public override void SetEnabledCommands( IEnumerable<string> commands )
        {
            if ( commands == null )
            {
                _context.AmbientValues["EnabledCommands"] = string.Empty;
            }
            else
            {
                _context.AmbientValues["EnabledCommands"] = commands.JoinStrings( "," );
            }
        }

        //public void SetMergeFieldValue( string key, object value )
        //{
        //    throw new NotImplementedException();
        //}

        //public void SetMergeFieldValue( string key, object value, string scopeSelector )
        //{
        //    throw new NotImplementedException();
        //}

        //public override void SetMergeFieldValue( string key, object value )
        //{
        //    throw new NotImplementedException();
        //}

        //public override void SetMergeFieldValue( string key, object value, string scopeSelector )
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Set a merge field value within the specified scope.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="scopeReference">root|parent|current</param>
        public override void SetMergeFieldValue( string key, object value, string scopeReference )
        {
            //int? scopeIndex = null;

            if ( string.IsNullOrWhiteSpace( scopeReference ) )
            {
                scopeReference = "current";
            }

            scopeReference = scopeReference.Trim().ToLower();

            if ( scopeReference == "root" )
            {
                //var scope = GetRootScope( _context.LocalScope );
                //scope.SetValue( key, value );

                _context.SetValue( key, value );
            }
            //else if ( scopeReference == "parent" )
            //{
            //    scopeIndex = null; // 1;
            //}
            else if ( scopeReference == "current" || scopeReference == "0" )
            {
                //scopeIndex = 0;
                _context.SetValue( key, value );
            }
            else
            {
                throw new LavaException( $"SetMergeFieldValue failed. Scope reference \"{ scopeReference }\" is invalid." );
                //scopeIndex = scopeReference.AsIntegerOrNull();
            }

            //if ( scopeIndex == null )
            //{
            //    throw new LavaException( $"SetMergeFieldValue failed. Scope reference \"{ scopeReference }\" is invalid." );
            //}
            //else if ( scopeIndex == 0 )
            //{
            //    _context.SetValue( key, value );
            //}

            //var fieldValue = GetDotLiquidCompatibleValue( value );

            // Set the variable in the specified scope.


            //_context.Scopes[scopeIndex.Value][key] = fieldValue;
        }
        //public void SetMergeFieldValues( LavaDictionary values )
        //{
        //    throw new NotImplementedException();
        //}

        //public override void SetMergeFieldValues( LavaDictionary values )
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Sets a named value that is for internal use only. Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public override void SetInternalFieldValue( string key, object value )
        {
            // In the Fluid framework, internal values are stored in the AmbientValues collection.
            _context.AmbientValues[key] = value;
        }

        //public override void SetInternalFieldValues( LavaDictionary values )
        //{
        //    foreach ( var kvp in values )
        //    {
        //        _context.AmbientValues[kvp.Key] = kvp.Value;
        //    }
        //}

        /// <summary>
        /// Gets a named value that is for internal use only. Internal values are not available to be resolved in the Lava Template.
        /// </summary>
        /// <param name="key"></param>
        public override object GetInternalFieldValue( string key, object defaultValue = null )
        {
            // In the Fluid framework, internal values are stored in the AmbientValues collection.
            object value;

            var exists = _context.AmbientValues.TryGetValue( key, out value );

            if ( exists )
            {
                return value;
            }

            return defaultValue;
        }
        public override LavaDictionary GetInternalFields()
        {
            var values = new LavaDictionary();

            foreach ( var item in _context.AmbientValues )
            {
                values.AddOrReplace( item.Key, item.Value );
            }

            return values;
        }

        //public void SetValue( string key, object value )
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Looks for a parsed template in cache (if the content is 100 characters or less).
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        private LavaFluidTemplate GetTemplate( string content )
        {
            var template = LavaFluidTemplate.Parse( content );

            // Do not cache any content over 100 characters in length
            //if ( content?.Length > 100 )
            //{
            //return template;
            //}

            // Get template from cache
            //var template = LavaTemplateCache.Get( content ).Template;

            // Clear any previous errors
            //template.Errors.Clear();

            return template;
        }

        //private Scope GetRootScope( Scope scope )
        //{
        //    var parentScope = GetParentScope( scope );

        //    while ( parentScope != null )
        //    {
        //        scope = parentScope;

        //        parentScope = GetParentScope( parentScope );
        //    }

        //    return scope;
        //}

        private Scope GetParentScope( Scope scope )
        {
            Scope parentScope = null;

            var parentField = scope.GetType().GetField( "_parent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );

            if ( parentField != null )
            {
                parentScope = parentField.GetValue( scope ) as Scope;
            }

            return parentScope;
        }

        /// <summary>
        /// Gets an aggregated set of key/value pairs for variables in the current scope and outer scopes.
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        private Dictionary<string, object> GetScopeAggregatedValues( Scope scope )
        {
            var dictionary = new Dictionary<string, object>( StringComparer.OrdinalIgnoreCase );

            while ( scope != null )
            {
                var properties = GetScopeDefinedValues( scope );

                foreach ( var key in properties.Keys )
                {
                    dictionary.AddOrIgnore( key, properties[key] );
                }

                scope = GetParentScope( scope );
            }

            return dictionary;
        }

        /// <summary>
        /// Gets an aggregated set of key/value pairs for variables defined in the current scope.
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        private Dictionary<string, object> GetScopeDefinedValues( Scope scope )
        {
            var dictionary = new Dictionary<string, object>( StringComparer.OrdinalIgnoreCase );

            // Fluid does not provide access to the key collection for the scope, so we need to use Reflection to get the underlying dictionary.
            var propertiesField = scope.GetType().GetField( "_properties", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance );

            if ( propertiesField != null )
            {
                var properties = propertiesField.GetValue( scope ) as Dictionary<string, FluidValue>;

                if ( properties != null )
                {
                    foreach ( var key in properties.Keys )
                    {
                        dictionary.AddOrReplace( key, properties[key].ToRealObjectValue() );
                    }
                }
            }

            return dictionary;
        }

        public override void EnterChildScope()
        {
            _context.EnterChildScope();
        }

        public override void ExitChildScope()
        {
            _context.ReleaseScope();
        }
    }
}
