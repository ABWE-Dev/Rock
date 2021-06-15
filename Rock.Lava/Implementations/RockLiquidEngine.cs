﻿// <copyright>
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
using System.Reflection;
using DotLiquid;
using Rock.Lava.DotLiquid;

namespace Rock.Lava.RockLiquid
{
    /// <summary>
    /// Initialization class for the RockLiquid Lava Templating Engine.
    /// This engine provides pass-through execution of Lava code for Rock v12 or below, using a Rock-specific fork of the DotLiquid framework.
    /// It is intended as a fall-back option to help troubleshoot issues with more recent engine implementations.
    /// </summary>
    public class RockLiquidEngine : LavaEngineBase
    {
        /// <summary>
        /// The descriptive name of the engine.
        /// </summary>
        public override string EngineName
        {
            get
            {
                return "RockLiquid";
            }
        }

        /// <summary>
        /// The type specifier for the framework.
        /// </summary>
        public override LavaEngineTypeSpecifier EngineType
        {
            get
            {
                return LavaEngineTypeSpecifier.RockLiquid;
            }
        }

        /// <summary>
        /// Create a new template context containing the specified merge fields.
        /// </summary>
        /// <param name="mergeFields"></param>
        /// <returns></returns>

        protected override ILavaRenderContext OnCreateRenderContext()
        {
            // Create a new DotLiquid Context.
            // Set the flag to rethrow exceptions generated by the DotLiquid framework, so they can be intercepted and handled as Lava errors.
            var dotLiquidContext = new global::DotLiquid.Context( new List<Hash>(), new Hash(), new Hash(), rethrowErrors: true );

            var context = new DotLiquidRenderContext( dotLiquidContext );

            return context;
        }

        /// <summary>
        /// Configure the DotLiquid engine with the specified options.
        /// </summary>
        public override void OnSetConfiguration( LavaEngineConfigurationOptions options )
        {
            // DotLiquid uses a RubyDateFormat by default,
            // but since we aren't using Ruby, we want to disable that
            Liquid.UseRubyDateFormat = false;

            /* 2020-05-20 MDP (actually this comment was here a long time ago)
                NOTE: This means that all the built in template filters,
                and the RockFilters, will use CSharpNamingConvention.

                For example the dotliquid documentation says to do this for formatting dates: 
                {{ some_date_value | date:"MMM dd, yyyy" }}

                However, if CSharpNamingConvention is enabled, it needs to be: 
                {{ some_date_value | Date:"MMM dd, yyyy" }}
            */

            Template.NamingConvention = new global::DotLiquid.NamingConventions.CSharpNamingConvention();

            if ( options.FileSystem == null )
            {
                options.FileSystem = new DotLiquidFileSystem( new LavaNullFileSystem() );
            }

            Template.FileSystem = new DotLiquidFileSystem( options.FileSystem );

            Template.RegisterSafeType( typeof( Enum ), o => o.ToString() );
            Template.RegisterSafeType( typeof( DBNull ), o => null );
        }

        /// <summary>
        /// Register the filters implemented by the provided System.Type entries so they can be used to resolve templates.
        /// </summary>
        /// <param name="implementingType"></param>
        protected override void OnRegisterFilters( Type implementingType )
        {
            Template.RegisterFilter( implementingType );
        }

        protected override void OnRegisterFilter( MethodInfo filterMethodInfo, string filterName )
        {
            throw new LavaException( "RegisterFilter failed. Cannot register an individual filter in RockLiquid." );
        }

        /// <summary>
        /// Register a specific System.Type as available for referencing in a Lava template.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="allowedMembers"></param>
        public override void RegisterSafeType( Type type, IEnumerable<string> allowedMembers )
        {
            if ( allowedMembers == null )
            {
                allowedMembers = new string[0];
            }

            Template.RegisterSafeType( type, allowedMembers.ToArray() );
        }

        /// <summary>
        /// Register a Lava Tag element.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        public override void RegisterTag( string name, Func<string, ILavaTag> factoryMethod )
        {
            if ( name == null )
            {
                throw new ArgumentException( "Name must be specified." );
            }

            name = name.Trim().ToLower();

            var tag = factoryMethod( name );

            Template.RegisterTag( tag.GetType(), name );

            base.RegisterTag( name, factoryMethod );
        }

        /// <summary>
        /// Register a Lava Block element.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        public override void RegisterBlock( string name, Func<string, ILavaBlock> factoryMethod )
        {
            if ( name == null )
            {
                throw new ArgumentException( "Name must be specified." );
            }

            name = name.Trim().ToLower();

            var tag = factoryMethod( name );

            Template.RegisterTag( tag.GetType(), name );

            base.RegisterBlock( name, factoryMethod );
        }

        /// <summary>
        /// Render the Lava template using the DotLiquid rendering engine.
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="output"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override LavaRenderResult OnRenderTemplate( ILavaTemplate template, LavaRenderParameters parameters )
        {
            var result = new LavaRenderResult();

            try
            {
                var renderSettings = new RenderParameters();

                // Set the flag to rethrow exceptions generated by the DotLiquid framework, so they can be intercepted and handled as Lava errors.
                // Note that this flag is required in addition to the Context(rethrowErrors) constructor parameter.
                renderSettings.RethrowErrors = true;

                var templateContext = parameters.Context as DotLiquidRenderContext;

                if ( templateContext == null )
                {
                    throw new LavaException( "Invalid LavaContext parameter. This context type is not compatible with the DotLiquid templating engine." );
                }

                var dotLiquidContext = templateContext.DotLiquidContext;

                renderSettings.Context = dotLiquidContext;

                if ( parameters.ShouldEncodeStringsAsXml )
                {
                    renderSettings.ValueTypeTransformers = new Dictionary<Type, Func<object, object>>();
                    renderSettings.ValueTypeTransformers.Add( typeof( string ), EncodeStringTransformer );
                }

                // Call the Render method of the underlying DotLiquid template.
                var templateProxy = template as DotLiquidTemplateProxy;

                result.Text = templateProxy.DotLiquidTemplate.Render( renderSettings );

                if ( renderSettings.Context.Errors != null )
                {
                    if ( renderSettings.Context.Errors.Count > 1 )
                    {
                        result.Error = new AggregateException( renderSettings.Context.Errors );
                    }
                    else
                    {
                        result.Error = renderSettings.Context.Errors.FirstOrDefault();
                    }
                }
            }
            catch ( Exception ex )
            {
                string output;

                ProcessException( ex, parameters.ExceptionHandlingStrategy, out output );

                result.Text = output;
                result.Error = ex;
            }

            return result;
        }

        /// <summary>
        /// Encodes string values that are processed by a lava filter
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        private static object EncodeStringTransformer( object s )
        {
            string val = ( s as string );

            if ( !string.IsNullOrEmpty( val ) )
            {
                return val.EncodeXml();
            }
            else
            {
                return s;
            }
        }

        protected override ILavaTemplate OnParseTemplate( string inputTemplate )
        {
            // Create a new DotLiquid template and wrap it in a proxy for use with the Lava engine.
            var dotLiquidTemplate = CreateNewDotLiquidTemplate( inputTemplate );

            var lavaTemplate = new DotLiquidTemplateProxy( dotLiquidTemplate );

            return lavaTemplate;
        }

        /// <summary>
        /// Compare two objects for equivalence according to the applicable Lava equality rules for the input object types.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>True if the two objects are considered equal.</returns>
        public override bool AreEqualValue( object left, object right )
        {
            var condition = global::DotLiquid.Condition.Operators["=="];

            return condition( left, right );
        }

        private Template CreateNewDotLiquidTemplate( string inputTemplate )
        {
            // Remove custom comments from the source, but make no other changes because we are using the RockLiquid framework.
            var converter = new LavaToLiquidTemplateConverter();

            var liquidTemplate = converter.RemoveLavaComments( inputTemplate );

            var template = Template.Parse( liquidTemplate );

            /* 
             * 2/19/2020 - JPH
             * The DotLiquid library's Template object was not originally designed to be thread safe, but a PR has since
             * been merged into that repository to add this functionality (https://github.com/dotliquid/dotliquid/pull/220).
             * We have cherry-picked the PR's changes into our DotLiquid project, allowing the Template to operate safely
             * in a multithreaded context, which can happen often with our cached Template instances.
             *
             * Reason: Rock Issue #4084, Weird Behavior with Lava Includes
             */
            template.MakeThreadSafe();

            return template;
        }
    }
}
