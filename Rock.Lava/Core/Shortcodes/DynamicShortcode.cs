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
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Rock.Common;
using Rock.Lava.Utility;

namespace Rock.Lava.Shortcodes
{
    /// <summary>
    /// An implementation of a shortcode in the form of a Liquid block element.
    /// </summary>
    public class DynamicShortcodeBlock: DynamicShortcode, IRockLavaBlock
    {
        public DynamicShortcodeBlock()
        {
            //
        }

        public DynamicShortcodeBlock( DynamicShortcodeDefinition definition )
            : base( definition )
        {
            //
        }
    }

    /// <summary>
    /// An implementation of a shortcode that takes the form of an inline Liquid tag element.
    /// </summary>
    public class DynamicShortcodeTag : DynamicShortcode, IRockLavaTag
    {
        public DynamicShortcodeTag()
        {
            //
        }

        public DynamicShortcodeTag( DynamicShortcodeDefinition definition )
            : base ( definition )
        {
            //
        }
    }

    /// <summary>
    /// A shortcode that uses a parameterized Lava template supplied at runtime to dynamically generate a block or tag element in a Lava source document.
    /// </summary>
    public class DynamicShortcode : RockLavaShortcodeBase
    {
        string _elementAttributesMarkup = string.Empty;
        string _tagName = string.Empty;
        DynamicShortcodeDefinition _shortcode = null;
        private Dictionary<string, object> _internalMergeFields;
        StringBuilder _blockMarkup = new StringBuilder();

        const int _maxRecursionDepth = 10;

        /// <summary>
        /// Default constructor required for internal use.
        /// An instance created using this constructor must call the Initialize() method before use.
        /// </summary>
        public DynamicShortcode()
        {
            //
        }

        public DynamicShortcode( DynamicShortcodeDefinition definition )
        {
            this.Initialize( definition );

            AssertShortcodeIsInitialized();
        }

        public void Initialize( DynamicShortcodeDefinition definition )
        {
            if ( _shortcode != null )
            {
                throw new Exception();
            }

            _shortcode = definition;
        }

        public override LavaShortcodeTypeSpecifier ElementType
        {
            get
            {
                return _shortcode.ElementType;
            }
        }

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        /// <exception cref="System.Exception">Could not find the variable to place results in.</exception>
        public override void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            _elementAttributesMarkup = markup;
            _tagName = tagName;

            base.OnInitialize( tagName, markup, tokens );
        }

        private void AssertShortcodeIsInitialized()
        {
            if ( _shortcode == null )
            {
                throw new Exception( $"Shortcode configuration error. \"{_tagName}\" is not initialized." );
            }
        }

        public override void OnParsed( List<string> tokens )
        {
            _blockMarkup = new StringBuilder();

            //if ( !string.IsNullOrWhiteSpace( _blockMarkup.ToString() ) )
            //{
            //    //TODO :This should not be populated yet.
            //    int i = 0;
            //}

            var tokensCopy = new List<string>( tokens );

            // Get the block markup. The list of tokens contains all of the lava from the start tag to
            // the end of the template. This will pull out just the internals of the block.

            // We must take into consideration nested tags of the same type

            var endTagFound = false;

            // Create regular expressions for start and end tags.
            // In the source document, the Lava Shortcode element tag format is "{[ tagname ]}".
            // However, our pre-processing of the document substitutes the Lava-specific tag format for the Liquid-compatible tag format "{% tagname@ %}"
            var startTag = $@"{{\%\s*{ _tagName }\s*(.*?)\%}}";
            var endTag = $@"{{\%\s*end{ _tagName }\s*\%}}";

            var startTags = 0;

            Regex regExStart = new Regex( startTag );
            Regex regExEnd = new Regex( endTag );

            string token;
            while ( ( token = tokens.Shift() ) != null )
            {
                if ( Regex.Matches( _blockMarkup.ToString(), "Panel 1 content" ).Count > 1 )
                {
                    int i = 0;
                }

                Match startTagMatch = regExStart.Match( token );
                if ( startTagMatch.Success )
                {
                    startTags++; // increment the child tag counter
                    if ( startTags > 1 )
                    {
                        _blockMarkup.Append( token );
                    }
                }
                else
                {
                    Match endTagMatch = regExEnd.Match( token );

                    if ( endTagMatch.Success )
                    {
                        if ( startTags > 1 )
                        {
                            startTags--; // decrement the child tag counter
                            _blockMarkup.Append( token );
                        }
                        else
                        {
                            endTagFound = true;
                            break;
                        }
                    }
                    else
                    {
                        _blockMarkup.Append( token );
                    }
                }

            }

            // If this is a block, we need a closing tag.
            if ( this.ElementType == LavaShortcodeTypeSpecifier.Block
                 && !endTagFound )
            {
                AssertMissingDelimitation();
            }
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        //public override void Render( Context context, TextWriter result )
        public override void OnRender( ILavaContext context, TextWriter result )
        {
            if ( _shortcode == null )
            {
                result.Write( $"An error occurred while processing the {0} shortcode.", _tagName );
            }
            
            // Get the default settings for the shortcode, then apply the specified parameters.
            var parms = new Dictionary<string, object>();

            foreach ( var shortcodeParm in _shortcode.Parameters )
            {
                parms.AddOrReplace( shortcodeParm.Key, shortcodeParm.Value );
            }

            SetParametersFromElementAttributes( parms, _elementAttributesMarkup, context );

            // Set a unique id for the shortcode.
            parms.AddOrReplace( "uniqueid", "id-" + Guid.NewGuid().ToString() );

            // Apply the merge fields in the block context.
            var internalMergeFields = context.GetMergeFields();
            // new Dictionary<string, object>();

            foreach ( var item in parms )
                // context.GetMergeFields() )
            {
                internalMergeFields.AddOrReplace( item.Key, item.Value );
                 //parms.AddOrReplace( item.Key, item.Value );
            }

            //LoadBlockMergeFields( context, parms );            

            // Keep track of the recursion depth.
            int currentRecursionDepth = 0;

            if ( parms.ContainsKey( "RecursionDepth" ) )
            {
                currentRecursionDepth = parms["RecursionDepth"].ToString().AsInteger() + 1;

                if ( currentRecursionDepth > _maxRecursionDepth )
                {
                    result.Write( "A recursive loop was detected and processing of this shortcode has stopped." );
                    return;
                }
            }

            parms.AddOrReplace( "RecursionDepth", currentRecursionDepth );

            // Resolve any merge fields in the block content.
            // The block content will then be merged into the shortcode template to produce the final output.
            var blockMarkup = context.ResolveMergeFields( _blockMarkup.ToString(), internalMergeFields );

            // Extract any child elements from the block content.
            Dictionary<string, object> childParameters;

            blockMarkup = ExtractShortcodeBlockChildElements( blockMarkup, out childParameters );

            foreach ( var item in childParameters )
            {
                parms.AddOrReplace( item.Key, item.Value );
            }

            // After extracting the child parameters, merge the remaining block markup into the template.
            if ( blockMarkup.IsNotNullOrWhiteSpace() )
            {
                // JME (7/23/2019) Commented out the two lines below and substituted the line after to allow for better
                // processing of the block content. Testing was done on all existing shortcodes but leaving
                // this code in place in case a future edge case is found. Could/should remove this in the future.
                // Regex rgx = new Regex( @"{{\s*blockContent\s*}}", RegexOptions.IgnoreCase );
                // lavaTemplate = rgx.Replace( lavaTemplate, blockMarkup );
                parms.AddOrReplace( "blockContent", blockMarkup );

                parms.AddOrReplace( "blockContentExists", true );
            }
            else
            {
                parms.AddOrReplace( "blockContentExists", false );
            }

            // Now ensure there aren't any entity commands in the block that are not allowed.
            // This is necessary because the shortcode may be configured to allow more entities for processing
            // than the source block, template, action, etc. permits.
            var securityCheck = context.ResolveMergeFields( blockMarkup, new Dictionary<string, object>() );

            Regex securityPattern = new Regex( string.Format( Constants.Messages.NotAuthorizedMessage, ".*" ) );
            Match securityMatch = securityPattern.Match( securityCheck );

            if ( securityMatch.Success )
            {
                result.Write( securityMatch.Value ); // return security error message
            }
            else
            {
                // If the shortcode specifies a set of enabled Lava commands, set these for the current context.
                var blockCommands = context.GetEnabledCommands();

                if ( _shortcode.EnabledLavaCommands.Any() )
                {
                    context.SetEnabledCommands( _shortcode.EnabledLavaCommands );
                }

                // Resolve the child parameters in the template to get the final output.
                var lavaTemplate = _shortcode.TemplateMarkup;

                string results;

                LavaEngine.Instance.TryRender( lavaTemplate, out results, new LavaDictionary( parms ) );

                var expectedOutput = @"
Parameter 1: value1
Parameter 2: value2
Items:
Panel 1 - Panel 1 content.
Panel 2 - Panel 2 content.
Panel 3 - Panel 3 content.
";

            if ( results.RemoveSpaces() != expectedOutput.RemoveSpaces() )
            {
                    int i = 0;
            }

                result.Write( results.Trim() );

                // Revert the enabled commands to those of the block.
                context.SetEnabledCommands( blockCommands );

                // TODO: Removed 5/12/2020.
                //base.OnRender( context, result );
            }
        }

        /// <summary>
        /// Extracts the child elements from the content of a shortcode block.
        /// </summary>
        /// <param name="blockContent">Content of the block.</param>
        /// <param name="childParameters">The child parameters.</param>
        /// <returns></returns>
        private string ExtractShortcodeBlockChildElements( string blockContent, out Dictionary<string, object> childParameters )
        {
            childParameters = new Dictionary<string, object>();

            var startTagStartExpress = new Regex( @"\[\[\s*" );

            var matchExists = true;
            while ( matchExists )
            {
                var match = startTagStartExpress.Match( blockContent );
                if ( match.Success )
                {
                    int starTagStartIndex = match.Index;

                    // get the name of the parameter
                    var parmNameMatch = new Regex( @"[\w-]*" ).Match( blockContent, starTagStartIndex + match.Length );
                    if ( parmNameMatch.Success )
                    {
                        var parmNameStartIndex = parmNameMatch.Index;
                        var parmNameEndIndex = parmNameStartIndex + parmNameMatch.Length;
                        var parmName = blockContent.Substring( parmNameStartIndex, parmNameMatch.Length );

                        // get end of the tag index
                        var startTagEndIndex = blockContent.IndexOf( "]]", parmNameStartIndex ) + 2;

                        // get the tags parameters
                        var tagParms = blockContent.Substring( parmNameEndIndex, startTagEndIndex - parmNameEndIndex ).Trim();

                        // get the closing tag location
                        var endTagMatchExpression = String.Format( @"\[\[\s*end{0}\s*\]\]", parmName );
                        var endTagMatch = new Regex( endTagMatchExpression ).Match( blockContent, starTagStartIndex );

                        if ( endTagMatch != null )
                        {
                            var endTagStartIndex = endTagMatch.Index;
                            var endTagEndIndex = endTagStartIndex + endTagMatch.Length;

                            // get the parm content (the string between the two parm tags)
                            var parmContent = blockContent.Substring( startTagEndIndex, endTagStartIndex - startTagEndIndex ).Trim();

                            // create dynamic object from parms
                            var dynamicParm = new Dictionary<string, Object>();
                            dynamicParm.Add( "content", parmContent );

                            var parmItems = Regex.Matches( tagParms, @"(\S*?:'[^']+')" )
                                .Cast<Match>()
                                .Select( m => m.Value )
                                .ToList();

                            foreach ( var item in parmItems )
                            {
                                var itemParts = item.ToString().Split( new char[] { ':' }, 2 );
                                if ( itemParts.Length > 1 )
                                {
                                    dynamicParm.Add( itemParts[0].Trim().ToLower(), itemParts[1].Trim().Substring( 1, itemParts[1].Length - 2 ) );
                                }
                            }

                            // add new parm to a collection of parms and as a single parm if none exist
                            if ( childParameters.ContainsKey( parmName + "s" ) )
                            {
                                var parmList = (List<object>)childParameters[parmName + "s"];
                                parmList.Add( dynamicParm );
                            }
                            else
                            {
                                var parmList = new List<object>();
                                parmList.Add( dynamicParm );
                                childParameters.Add( parmName + "s", parmList );
                            }

                            if ( !childParameters.ContainsKey( parmName ) )
                            {
                                childParameters.Add( parmName, dynamicParm );
                            }

                            // pull this tag out of the block content
                            blockContent = blockContent.Remove( starTagStartIndex, endTagEndIndex - starTagStartIndex );
                        }
                        else
                        {
                            // there was no matching end tag, for safety sake we'd better bail out of loop
                            matchExists = false;
                            blockContent = blockContent + "Warning: missing end tag end" + parmName;
                        }
                    }
                    else
                    {
                        // there was no parm name on the tag, for safety sake we'd better bail out of loop
                        matchExists = false;
                        blockContent = blockContent + "Warning: invalid child parameter definition.";
                    }

                }
                else
                {
                    matchExists = false; // we're done here
                }
            }

            return blockContent.Trim();
        }

        /// <summary>
        /// Parses the markup.
        /// </summary>
        /// <param name="elementAttributesMarkup">The markup.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private void SetParametersFromElementAttributes( Dictionary<string, object> parameters, string elementAttributesMarkup, ILavaContext context )
        {
            // first run lava across the inputted markup
            var resolvedMarkup = context.ResolveMergeFields( elementAttributesMarkup, _internalMergeFields );

            var markupItems = Regex.Matches( resolvedMarkup, @"(\S*?:'[^']+')" )
                .Cast<Match>()
                .Select( m => m.Value )
                .ToList();

            foreach ( var item in markupItems )
            {
                var itemParts = item.ToString().Split( new char[] { ':' }, 2 );
                if ( itemParts.Length > 1 )
                {
                    parameters.AddOrReplace( itemParts[0].Trim().ToLower(), itemParts[1].Trim().Substring( 1, itemParts[1].Length - 2 ) );
                }
            }

            // OK, now let's look for any passed variables ala: name:variable
            var variableTokens = Regex.Matches( resolvedMarkup, @"\w*:\w+" )
                .Cast<Match>()
                .Select( m => m.Value )
                .ToList();

            foreach ( var item in variableTokens )
            {
                var itemParts = item.Trim().Split( new char[] { ':' }, 2 );
                if ( itemParts.Length > 1 )
                {
                    var scopeKey = itemParts[1].Trim();

                    var scopeObject = context.GetMergeFieldValue( scopeKey, null );

                    if ( scopeObject != null )
                    {
                        parameters.AddOrReplace( itemParts[0].Trim().ToLower(), scopeObject );
                        break;
                    }

                    // context.Scopes is a weird beast can't find a cleaner way to get the object than to iterate over it
                    //foreach ( var scopeItem in context.GetScopes() )
                    //{
                    //    var scopeObject = scopeItem.Where( x => x.Key == scopeKey ).FirstOrDefault();

                    //    if ( scopeObject.Value != null )
                    //    {
                    //        parms.AddOrReplace( itemParts[0].Trim().ToLower(), scopeObject.Value );
                    //        break;
                    //    }
                    //}
                }
            }
        }

        /// <summary>
        /// Loads the block merge fields.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="parms">The parms.</param>
        private void LoadBlockMergeFields( ILavaContext context, Dictionary<string, object> parms )
        {
            _internalMergeFields = new Dictionary<string, object>();

            foreach ( var item in context.GetMergeFields() )
            {
                _internalMergeFields.AddOrReplace( item.Key, item.Value );
                parms.AddOrReplace( item.Key, item.Value );
            }
        }
    }
}