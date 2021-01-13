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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using DotLiquid;
using DotLiquid.Exceptions;

using Rock.Common;
using Rock.Data;

namespace Rock.Lava.DotLiquid
{

    public class DotLiquidLavaContext : LavaContextBase
    {
        private Context _context;

        public DotLiquidLavaContext( Context context )
        {
            _context = context;
        }

        public Context DotLiquidContext
        {
            get
            {
                return _context;
            }
        }

        //public override IList<LavaDictionary> GetEnvironments()
        //{
        //    var environments = new List<LavaDictionary>();

        //    foreach ( var hash in _context.Environments )
        //    {
        //        environments.Add( new LavaDictionary( hash ) );
        //    }

        //    return environments;
        //}

        //public override IList<LavaDictionary> GetScopes()
        //{
        //    var environments = new List<LavaDictionary>();

        //    foreach ( var hash in _context.Scopes )
        //    {
        //        environments.Add( new LavaDictionary( hash ) );
        //    }

        //    return environments;
        //}

        public override void SetEnabledCommands( IEnumerable<string> commands )
        {
            if ( commands == null )
            {
                _context.Registers["EnabledCommands"] = string.Empty;
            }
            else
            {
                _context.Registers["EnabledCommands"] = commands.JoinStrings( "," );
            }
        }

        public override List<string> GetEnabledCommands()
        {
            // The set of enabled Lava Commands is stored in the DotLiquid Registers collection.
            if ( _context.Registers?.ContainsKey( "EnabledCommands" ) == true )
            {
                return _context.Registers["EnabledCommands"].ToString().Split( ",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries ).ToList();
            }

            return new List<string>();
        }

        /// <summary>
        /// Gets the dictionary of values that are active in the local scope.
        /// Values are defined by the outermost container first, and overridden by values defined in a contained scope.
        /// </summary>
        /// <returns></returns>
        public override LavaDictionary GetMergeFields()
        {
            var fields = new LavaDictionary();

            // First, get all of the variables defined in the local lava context.
            // In DotLiquid, the innermost scope is the first element in the collection.
            foreach ( var scope in _context.Scopes )
            {
                foreach ( var item in scope )
                {
                    fields.AddOrIgnore( item.Key, item.Value );
                }
            }

            // Second, add any variables defined by the block or container in which the template is being resolved.
            foreach ( var environment in _context.Environments )
            {
                foreach ( var item in environment )
                {
                    fields.AddOrIgnore( item.Key, item.Value );
                }
            }

            return fields;
        }

        /// <summary>
        /// Get the value of a field that is accessible for merging into a template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public override object GetMergeFieldValue( string key, object defaultValue )
        {
            if ( !_context.HasKey(key) )
            {
                return defaultValue;
            }

            return _context[key];
        }

        /// <summary>
        /// Get a dictionary of field values that are accessible for merging in to a template.
        /// </summary>
        //public override LavaDictionary GetMergeFields()
        //{
        //    var dictionary = new LavaDictionary();

        //    // TODO: Do we also need to merge in the Environment values stack here?

        //    // Get a flattened set of values from all Scopes in the current context.
        //    // Scopes are listed from innermost to outermost, and therefore need to be collated from outermost to innermost.
        //    for ( int i = _context.Scopes.Count - 1; i >= 0; i-- )
        //    {
        //        var scope = _context.Scopes[i];

        //        foreach (var kvp in scope )
        //        {
        //            dictionary[kvp.Key] = kvp.Value;
        //        }
        //    }

        //    return dictionary;
        //}

        public override string ResolveMergeFields( string content, IDictionary<string, object> mergeObjects, string enabledLavaCommands = null, bool encodeStrings = false, bool throwExceptionOnErrors = false )
        {
            try
            {
                // 7-9-2020 JME / NA
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

                var dotLiquidTemplate = GetTemplate( content );

                dotLiquidTemplate.Registers.AddOrReplace( "EnabledCommands", enabledLavaCommands );

                var output = dotLiquidTemplate.Render( Hash.FromDictionary( mergeObjects ) );

                //template.InstanceAssigns.AddOrReplace( "CurrentPerson", currentPersonOverride );

                return output;
            }
            catch ( Exception ex )
            {
                throw ex;
                //ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );
                //return "Error resolving Lava merge fields: " + ex.Message;
            }
        }

        //public override void SetMergeFieldValue( string key, object value )
        //{
        //    var fieldValue = GetDotLiquidCompatibleValue( value );

        //    // Use the default implementation to set a variable in the current scope.
        //    _context[key] = fieldValue;
        //}

        public override void SetMergeFieldValue( string key, object value, string scopeReference )
        {
            int? scopeIndex;

            if ( string.IsNullOrWhiteSpace( scopeReference ) )
            {
                scopeIndex = 0;
            }
            else
            { 
                scopeReference = scopeReference.Trim().ToLower();

                if ( scopeReference == "root" )
                {
                    scopeIndex = _context.Scopes.Count - 1;
                }
                else if ( scopeReference == "parent" )
                {
                    scopeIndex = 1;
                }
                else if ( scopeReference == "current" )
                {
                    scopeIndex = 0;
                }
                else
                {
                    scopeIndex = scopeReference.AsIntegerOrNull();
                }
            }

            if ( scopeIndex == null )
            {
                throw new Exception( $"SetMergeFieldValue failed. Scope reference \"{ scopeReference }\" is invalid." );
            }

            var fieldValue = GetDotLiquidCompatibleValue( value );

            // Set the variable in the specified scope.
            _context.Scopes[scopeIndex.Value][key] = fieldValue;
        }

        /// <summary>
        /// Uses Lava to resolve any merge codes within the content using the values in the merge objects.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="mergeObjects">The merge objects.</param>
        /// <param name="enabledLavaCommands">The enabled lava commands.</param>
        /// <param name="encodeStrings">if set to <c>true</c> [encode strings].</param>
        /// <param name="throwExceptionOnErrors">if set to <c>true</c> [throw exception on errors].</param>
        /// <returns></returns>
        //public string ResolveMergeFields( this string content, IDictionary<string, object> mergeObjects, string enabledLavaCommands, bool encodeStrings = false, bool throwExceptionOnErrors = false )
        //{
        //    try
        //    {
        //        if ( !content.HasMergeFields() )
        //        {
        //            return content ?? string.Empty;
        //        }

        //        if ( GlobalAttributesCache.Get().LavaSupportLevel == Lava.LavaSupportLevel.LegacyWithWarning && mergeObjects.ContainsKey( "GlobalAttribute" ) )
        //        {
        //            if ( hasLegacyGlobalAttributeLavaMergeFields.IsMatch( content ) )
        //            {
        //                Rock.Model.ExceptionLogService.LogException( new Rock.Lava.LegacyLavaSyntaxDetectedException( "GlobalAttribute", "" ), System.Web.HttpContext.Current );
        //            }
        //        }

        //        Template template = GetTemplate( content );
        //        template.Registers.AddOrReplace( "EnabledCommands", enabledLavaCommands );

        //        string result;

        //        if ( encodeStrings )
        //        {
        //            // if encodeStrings = true, we want any string values to be XML Encoded ( 
        //            RenderParameters renderParameters = new RenderParameters();
        //            renderParameters.LocalVariables = Hash.FromDictionary( mergeObjects );
        //            renderParameters.ValueTypeTransformers = new Dictionary<Type, Func<object, object>>();
        //            renderParameters.ValueTypeTransformers[typeof( string )] = EncodeStringTransformer;
        //            result = template.Render( renderParameters );
        //        }
        //        else
        //        {
        //            result = template.Render( Hash.FromDictionary( mergeObjects ) );
        //        }

        //        if ( throwExceptionOnErrors && template.Errors.Any() )
        //        {
        //            if ( template.Errors.Count == 1 )
        //            {
        //                throw template.Errors[0];
        //            }
        //            else
        //            {
        //                throw new AggregateException( template.Errors );
        //            }
        //        }

        //        return result;
        //    }
        //    catch ( System.Threading.ThreadAbortException )
        //    {
        //        // Do nothing...it's just a Lava PageRedirect that just happened.
        //        return string.Empty;
        //    }
        //    catch ( Exception ex )
        //    {
        //        if ( throwExceptionOnErrors )
        //        {
        //            throw;
        //        }
        //        else
        //        {
        //            //ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );
        //            return "Error resolving Lava merge fields: " + ex.Message;
        //        }
        //    }
        //}

        /// <summary>
        /// Looks for a parsed template in cache (if the content is 100 characters or less).
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        private Template GetTemplate( string content )
        {
            // Do not cache any content over 100 characters in length
            //if ( content?.Length > 100 )
            //{
            return Template.Parse( content );
            //}

            // Get template from cache
            //var template = LavaTemplateCache.Get( content ).Template;

            // Clear any previous errors
            //template.Errors.Clear();

            //return template;
        }

        public void ClearValues()
        {
            _context.ClearInstanceAssigns();
        }

        private object GetDotLiquidCompatibleValue( object value )
        {
            // Primitive values do not require any special processing.
            if ( value == null
                 || value is string
                 || value is IEnumerable
                 || value is decimal
                 || value is DateTime
                 || value is DateTimeOffset
                 || value is TimeSpan
                 || value is Guid
                 || value is Enum
                 || value is KeyValuePair<string, object>
                 )
            {
                return value;
            }

            var valueType = value.GetType();

            if ( valueType.IsPrimitive )
            {
                return value;
            }

            // For complex types, check if a specific transformer has been defined for the type.
            var safeTypeTransformer = Template.GetSafeTypeTransformer( valueType );

            if ( safeTypeTransformer != null )
            {
                return safeTypeTransformer( value );
            }

            // Check if the type is decorated with the LavaType attribute.
            var attr = (LavaTypeAttribute)valueType.GetCustomAttributes( typeof( LavaTypeAttribute ), false ).FirstOrDefault();

            if ( attr != null )
            {
                var allowedProperties = GetLavaTypeAllowedProperties( valueType, attr );

                return new DropProxy( value, allowedProperties );

                //return GetLavaTypeProxy( valueType  new DropProxy( value, attr.AllowedMembers );
            }

            // Check if any of the type properties are decorated with LavaInclude or LavaIgnore attributes.
            //xyzzy

            return value;
        }

        private string[] GetLavaTypeAllowedProperties( Type type, LavaTypeAttribute attr )
        {
            List<PropertyInfo> includedProperties;

            // Get the list of included properties, then remove the ignored properties.
            if ( attr.AllowedMembers == null || !attr.AllowedMembers.Any() )
            {
                // No included properties have been specified, so assume all are included.
                includedProperties = type.GetProperties().ToList();
            }
            else
            {
                includedProperties = type.GetProperties().Where( x => attr.AllowedMembers.Contains( x.Name, StringComparer.OrdinalIgnoreCase ) ).ToList();
            }

            var ignoredProperties = type.GetProperties().Where( x => x.GetCustomAttributes( typeof( LavaIgnoreAttribute ), false ).Any() ).ToList();

            var allowedProperties = includedProperties.Except( ignoredProperties ).Select( x => x.Name ).ToArray();

            return allowedProperties;

            //var attr = (LavaTypeAttribute)valueType.GetCustomAttributes( typeof( LavaTypeAttribute ), false ).First();

            //return new DropProxy( value, allowedProperties );


            //foreach ( var includedProperty in includedProperties )
            //{
            //    if ( ignoredProperties.Contains( includedProperty ) )
            //    {
            //        continue;
            //    }

            //    var newAccessor = new LavaTypeMemberAccessor( includedProperty );

            //    Register( type, includedProperty.Name, newAccessor );
            //}
        }

        //public void Register( Type type, string name, IMemberAccessor getter )
        //{
        //    if ( !_map.TryGetValue( type, out var typeMap ) )
        //    {
        //        typeMap = new Dictionary<string, IMemberAccessor>( IgnoreCasing
        //            ? StringComparer.OrdinalIgnoreCase
        //            : StringComparer.Ordinal );

        //        _map[type] = typeMap;
        //    }

        //    typeMap[name] = getter;
        //}

        #region Unused???

        //TODO: Remove this?
        //public override IDictionary<string, object> GetMergeFieldsInContainerScope()
        //{
        //    // get merge fields loaded by the block or container
        //    var internalMergeFields = new Dictionary<string, object>();

        //    if ( _context.Environments.Count > 0 )
        //    {
        //        foreach ( var item in _context.Environments[0] )
        //        {
        //            internalMergeFields.AddOrReplace( item.Key, item.Value );
        //        }
        //    }

        //    return internalMergeFields;
        //}

        // TODO: Remove this?
        //public override IDictionary<string, object> GetMergeFieldsInScope()
        //{
        //    var fields = new Dictionary<string, object>();

        //    // get variables defined in the lava source
        //    foreach ( var scope in _context.Scopes )
        //    {
        //        foreach ( var item in scope )
        //        {
        //            fields.AddOrReplace( item.Key, item.Value );
        //        }
        //    }

        //    return fields;
        //}

        public override object GetInternalFieldValue( string key )
        {
            return _context.Registers[key];
        }

        public override LavaDictionary GetInternalFields()
        {
            var values = new LavaDictionary();

            foreach ( var item in _context.Registers )
            {
                values.AddOrReplace( item.Key, item.Value );
            }

            // TODO: Should we merge in the Environment variables here too?
            //var lavaMergeFields = new Dictionary<string, object>();
            //if ( context.Environments?.Count > 0 )
            //{
            //    foreach ( var item in context.Environments[0] )
            //    {
            //        lavaMergeFields.Add( item.Key, item.Value );
            //    }
            //}

            return values;
        }

        public override void SetInternalFieldValue( string key, object value )
        {
            _context.Registers[key] = value;
        }

        public override void EnterChildScope()
        {
            // Push a new scope onto the stack.
            var newScope = new Hash();

            _context.Push( newScope );
        }

        public override void ExitChildScope()
        {
            _context.Pop();
        }

        /// <summary>
        /// Push new local scope on the stack. use <tt>Context#stack</tt> instead
        /// </summary>
        /// <param name="newScope"></param>
        //public override void Push( LavaDictionary newScope )
        //{
        //    if ( _context.Scopes.Count > 80 )
        //    {
        //        throw new StackLevelException( "ContextStackException" );
        //    }

        //    _context.Scopes.Insert( 0, newScope. Hash.FromDictionary( newScope ) );
        //}

        /// <summary>
        /// Pop from the stack. use <tt>Context#stack</tt> instead
        /// </summary>
        //public override LavaDictionary Pop()
        //{
        //    if ( _context.Scopes.Count == 1 )
        //    {
        //        throw new ContextException();
        //    }

        //    var result = LavaDictionary.FromDictionary( _context.Scopes[0] );

        //    _context.Scopes.RemoveAt( 0 );

        //    return result;
        //}

        //public override void Stack( LavaDictionary newScope, Action callback )
        //{
        //    Push( newScope );
        //    try
        //    {
        //        callback();
        //    }
        //    finally
        //    {
        //        Pop();
        //    }
        //}

        /// <summary>
        /// pushes a new local scope on the stack, pops it at the end of the block
        /// 
        /// Example:
        /// 
        /// context.stack do
        /// context['var'] = 'hi'
        /// end
        /// context['var] #=> nil
        /// </summary>
        /// <param name="newScope"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        //public override void Stack( Action callback )
        //{

        //    try
        //    {
        //        callback();
        //    }
        //    finally
        //    {
        //    }

        //}

        #endregion


    }
}