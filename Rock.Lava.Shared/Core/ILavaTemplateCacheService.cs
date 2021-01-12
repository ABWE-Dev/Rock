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

namespace Rock.Lava
{
    /// <summary>
    /// A component that is capable of providing a compiled Lava template object from a Lava source document.
    /// The strategy for compiling, caching and retrieving the object is determined by the provider.
    /// </summary>
    public interface ILavaTemplateCacheService
    {
        /// <summary>
        /// A flag to indicate if caching is enabled for template objects.
        /// Caching allows the compiler to cache and reuse compiled template objects.
        /// If caching is disabled, a new template is compiled from the source document on each request.
        /// </summary>
        //bool CachingIsEnabled { get; set; }

        /// <summary>
        /// Returns a compiled LavaTemplate object from the template cache.
        /// If the template does not already exist, it will be created and added to the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="content">The content.</param>
        /// <param name="template"></param>
        /// <param name="retrievedFromCache"></param>
        /// <returns></returns>
        //void GetTemplate( string key, string content, out ILavaTemplate template, out bool retrievedFromCache );

        /// <summary>
        /// Gets an item from cache, and if not found, executes the itemFactory to create the item and add it to the cache.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="itemFactory">The item factory.</param>
        /// <param name="template"></param>
        /// <returns></returns>
        //ILavaTemplate GetOrAddExisting( string key, Func<ILavaTemplate> itemFactory ); //, Func<List<string>> keyFactory = null )
        bool TryGetTemplate( string content, Func<ILavaTemplate> itemFactory, out ILavaTemplate template );

        /// <summary>
        /// Calculate a unique cache key for the specified template content.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        //string GetTemplateKey( string content );

        //{
        //    string qualifiedKey = QualifiedKey( key );
        //    var value = RockCacheManager<T>.Instance.Cache.Get( qualifiedKey );

        //    if ( value != null )
        //    {
        //        return value;
        //    }

        //    if ( itemFactory == null )
        //        return default( T );

        //    value = itemFactory();
        //    if ( value != null )
        //    {
        //        UpdateCacheItem( key, value, keyFactory );
        //    }

        //    return value;
        //}

        /// <summary>
        /// Returns a compiled LavaTemplate object from the template cache, if it exists.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="template"></param>
        /// <returns>true, if the template was retrieved from the cache.</returns>
        //bool TryGetTemplate( string key, out ILavaTemplate template );
    }
}