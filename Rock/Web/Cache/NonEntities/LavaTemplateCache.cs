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
using System.Text.RegularExpressions;
using Rock.Lava;

namespace Rock.Web.Cache
{
    /// <summary>
    /// An implementation of a provider for Lava Template objects that supports caching in a web environnment.
    /// </summary>
    public class LavaTemplateCache : ItemCache<LavaTemplateCache>
    {
        #region Constructors

        /// <summary>
        /// Use Static Get() method to instantiate a new Global Attributes object
        /// </summary>
        private LavaTemplateCache()
        {
            DefaultLifespan = TimeSpan.FromMinutes( 10 );
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Template object.
        /// </summary>
        public ILavaTemplate Template { get; set; }

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets the or add existing.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <returns></returns>
        [RockObsolete( "1.8" )]
        [Obsolete( "No longer needed", true )]
        public static LavaTemplateCache GetOrAddExisting( string key, Func<LavaTemplateCache> valueFactory )
        {
            return ItemCache<LavaTemplateCache>.GetOrAddExisting( key, null );
        }

        /// <summary>
        /// Returns LavaTemplate object from cache.  If template does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static LavaTemplateCache Get( string content )
        {
            return Get( content, content );
        }

        /// <summary>
        /// Reads the specified content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        [RockObsolete( "1.8" )]
        [Obsolete("Use Get instead", true )]
        public static LavaTemplateCache Read( string content )
        {
            return Get( content );
        }

        /// <summary>
        /// Returns LavaTemplate object from cache.  If template does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static LavaTemplateCache Get( string key, string content )
        {
            LavaTemplateCache template;
            bool fromCache;

            Get( key, content, out template, out fromCache );

            return template;
        }

        /// <summary>
        /// Returns a LavaTemplate object from the template cache.
        /// If the template does not already exist, it will be created and added to the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="content">The content.</param>
        /// <param name="template"></param>
        /// <param name="retrievedFromCache"></param>
        /// <returns></returns>
        public static void Get( string key, string content, out LavaTemplateCache template, out bool retrievedFromCache )
        {
            // If cache items need to be serialized, do not cache the template because it isn't serializable.
            if ( RockCache.IsCacheSerialized )
            {
                template = Load( content );
                retrievedFromCache = false;
                return;
            }

            var fromCache = false;

            template = ItemCache<LavaTemplateCache>.GetOrAddExisting( key, () =>
            {
                fromCache = true;
                return Load( content );
            } );

            retrievedFromCache = fromCache;

            return;
        }

        private static LavaTemplateCache Load( string content )
        {
            // Strip out Lava comments before parsing the template because they are not recognized by standard Liquid syntax.
            content = LavaHelper.RemoveLavaComments( content );

            ILavaTemplate template;

            LavaEngine.Instance.TryParseTemplate( content, out template );

            var lavaTemplate = new LavaTemplateCache { Template = template };

            return lavaTemplate;
        }

        #endregion
    }
}