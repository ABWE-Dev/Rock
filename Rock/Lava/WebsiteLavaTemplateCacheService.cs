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
using Rock.Web.Cache;

namespace Rock.Lava
{
    /// <summary>
    /// An implementation of a LavaTemplateProvider that uses a website-based caching strategy to improve performance.
    /// </summary>
    public class WebsiteLavaTemplateCacheService : ILavaTemplateCacheService
    {
        /// <summary>
        /// A flag to indicate if caching is enabled for template objects.
        /// </summary>
        //public bool CachingIsEnabled
        //{
        //    get { return LavaTemplateCache.CachingIsEnabled; }
        //    set { LavaTemplateCache.CachingIsEnabled = value; }
        //}

        /// <summary>
        /// Returns a compiled LavaTemplate object from the template cache.
        /// If the template does not already exist, it will be created and added to the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="content">The content.</param>
        /// <param name="template"></param>
        /// <param name="retrievedFromCache"></param>
        /// <returns></returns>
        public void GetTemplate( string key, string content, out ILavaTemplate template, out bool retrievedFromCache )
        {
            LavaTemplateCache templateCacheEntry;

            LavaTemplateCache.Get( key, content, out templateCacheEntry, out retrievedFromCache );

            template = templateCacheEntry.Template;
        }

        public string GetTemplateKey( string content )
        {
            const int hashLength = 10;
            string templateKey;

            if ( string.IsNullOrEmpty( content ) )
            {
                /* [2020-08-01] DJL - Cache the null template specifically, but process other whitespace templates individually
                 * to ensure that the format of the final output is preserved.
                 */
                templateKey = string.Empty;
            }
            else if ( content.Length <= hashLength )
            {
                // If the content is less than the size of the MD5 hash,
                // simply use the content as the key to save processing time.
                templateKey = content;
            }
            else
            {
                // Calculate a hash of the content using xxHash.
                templateKey = content.XxHash();
            }

            return templateKey;

            // Retrieve the template from the cache, or add it to the cache with specified key.
            // It is essential that the LavaTemplate object is thread-safe.
            //var template = LavaTemplateCache.Get( templateKey, content ).Template;

            //return template;
        }

        public bool TryGetTemplate( string content, Func<ILavaTemplate> itemFactory, out ILavaTemplate template )
        {
            // The LavaTemplateCache doesn't provide a method of getting a compiled template by key unless the source content is also supplied.
            // This may be an intentional means of limiting the number of cache queries required to add a new item.
            var key = GetTemplateKey( content );

            var cachedTemplate = LavaTemplateCache.Get( key, content );

            if ( cachedTemplate != null )
            {
                template = cachedTemplate.Template;
            }
            else
            {
                template = null;
            }

            return ( template != null );
        }

    }
}
