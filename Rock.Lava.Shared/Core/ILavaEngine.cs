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
using System.IO;

namespace Rock.Lava
{
    public enum LavaEngineTypeSpecifier
    {
        // DotLiquid is an open-source implementation of the Liquid templating language. [https://github.com/dotliquid/dotliquid]
        DotLiquid = 1,
        // Fluid is an open-source implementation of the Liquid templating language. [https://github.com/sebastienros/fluid]
        Fluid = 2
    }

    /// <summary>
    /// Represents a Lava Template.
    /// </summary>
    public interface ILavaEngine
    {
        /// <summary>
        /// Gets or sets a flag to determine if compiled Lava templates can be cached and reused.
        /// </summary>
        bool TemplateCachingIsEnabled { get; set; }

        /// <summary>
        /// Set configuration options for the Lava engine.
        /// </summary>
        /// <param name="options"></param>
        void Initialize( LavaEngineConfigurationOptions options = null );
        //void Initialize( ILavaFileSystem fileSystem, IList<Type> filterImplementationTypes = null );

        /// <summary>
        /// The descriptive name of the templating framework on which Lava is currently operating.
        /// </summary>
        string EngineName { get; }

        /// <summary>
        /// The Liquid template framework used to parse and render Lava templates.
        /// </summary>
        LavaEngineTypeSpecifier EngineType { get; }

        /// <summary>
        /// Get a new context instance.
        /// </summary>
        /// <returns></returns>
        ILavaContext NewContext();

        void RegisterTag( string name, Func<string, IRockLavaTag> factoryMethod );
        void RegisterBlock( string name, Func<string, IRockLavaBlock> factoryMethod );

        /// <summary>
        /// Registers a shortcode with a factory method that provides the definition of the shortcode at runtime.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factoryMethod"></param>
        void RegisterDynamicShortcode( string name, Func<string, DynamicShortcodeDefinition> factoryMethod );

        void RegisterStaticShortcode( string name, Func<string, IRockShortcode> factoryMethod );

        /// <summary>
        /// Registers a shortcode with a factory method that provides the name andd definition of the shortcode dynamically.
        /// </summary>
        /// <param name="factoryMethod"></param>
        void RegisterStaticShortcode( Func<string, IRockShortcode> factoryMethod );

        Dictionary<string, ILavaElementInfo> GetRegisteredElements();

        //void UnregisterShortcode( string name );

        //Type GetShortcodeType( string name );

        /// <summary>
        /// Try to render the provided template
        /// </summary>
        /// <param name="inputTemplate"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        bool TryRender( string inputTemplate, out string output );

        bool TryRender( string inputTemplate, out string output, LavaDictionary mergeValues );

        bool TryRender( string inputTemplate, out string output, ILavaContext context );
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="allowedMembers"></param>
        void RegisterSafeType( Type type, string[] allowedMembers = null );

        bool TryParseTemplate( string inputTemplate, out ILavaTemplate template );

        //ILavaTemplate ParseTemplate( string inputTemplate );

        /// <summary>
        /// Compare two objects for equivalence according to the applicable Lava equality rules for the input object types.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>True if the two objects are considered equal.</returns>
        bool AreEqualValue( object left, object right );

        /// <summary>
        /// Gets or sets the strategy for handling exceptions encountered during the rendering process.
        /// </summary>
        ExceptionHandlingStrategySpecifier ExceptionHandlingStrategy { get; set; }
    }

    public enum ExceptionHandlingStrategySpecifier
    {
        Throw = 0,
        RenderToOutput = 1,
        Ignore = 2

    }
}
