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
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Irony.Parsing;
using Microsoft.Extensions.Primitives;
using Rock.Lava.Blocks;

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// A wrapper for a Lava Block that enables it to be rendered by the Fluid templating engine.
    /// </summary>
    /// <remarks>
    /// This implementation uses a set of pre-registered factory methods to configure instances of the FluidBlockProxy
    /// dynamically at runtime.
    /// The FluidBlockProxy wraps a LavaBlock that is executed internally to render the element content.
    /// This approach allows the LavaBlock to be more easily adapted for use with alternative Liquid templating engines.
    /// </remarks>
    internal class FluidBlockProxy : ITagEx, ILiquidFrameworkElementRenderer
    {
        #region Static factory methods

        private static Dictionary<string, Func<string, IRockLavaBlock>> _factoryMethods = new Dictionary<string, Func<string, IRockLavaBlock>>( StringComparer.OrdinalIgnoreCase );

        public static void RegisterFactory( string name, Func<string, IRockLavaBlock> factoryMethod )
        {
            if ( string.IsNullOrWhiteSpace( name ) )
            {
                throw new ArgumentException( "Name must be specified." );
            }

            name = name.Trim().ToLower();

            _factoryMethods[name] = factoryMethod;
        }

        #endregion

        #region Debug Code

        public Guid Id = Guid.NewGuid();

        #endregion

        //private IRockLavaBlock _lavaBlock = null;
        private string _sourceElementName;

        public string SourceElementName
        {
            get
            {
                return _sourceElementName; // _lavaBlock.SourceElementName;
            }
        }

        #region ITag Implementation

        /// <summary>
        /// Retrieve the syntax rules for the argument markup in this element tag.
        /// The syntax is defined by a set of Irony.NET grammar rules that Fluid uses to parse the tag.
        /// </summary>
        /// <param name="grammar"></param>
        /// <returns></returns>
        public BnfTerm GetSyntax( FluidGrammar grammar )
        {
            var blockEndTag = "%}";
            
            // Lava syntax uses whitespace as a separator between arguments, which Fluid/Irony does not support.
            // Therefore we return a syntax for this element that captures the entire argument list as a single token
            // and we will then parse the arguments list later in the process.
            var lavaArgumentList = new FreeTextLiteral( "lavaElementAttributesMarkup", FreeTextOptions.AllowEmpty | FreeTextOptions.AllowEof, blockEndTag );

            // Return a syntax that allows an empty arguments list, a comma-delimited list per the standard Fluid implementation,
            // or a freetext string to support the whitespace-delimited argument list used by Lava syntax.
            return grammar.Empty | grammar.FilterArguments.Rule | lavaArgumentList;
        }

        //private ParseTreeNode _rootNode = null;

        public Statement Parse( ParseTreeNode node, ParserContext context )
        {
            throw new NotImplementedException();
        }

        public Statement Parse( ParseTreeNode node, LavaFluidParserContext context )
        {
            /* The Fluid framework parses the block into Liquid tokens using an adapted Irony.Net grammar.
             * Lava uses some syntax that is not recognized by Liquid, so we need to do some parsing of our own.
             * Also, some Lava blocks are designed to parse the raw source text of the block.
             * To maintain compatibility with that design, our parse process involves these steps:
             * 1. Extract the whitespace-delimited argument list from the open tag and parse it here.
             * 2. Extract text tokens from the source document, to allow Lava blocks to parse the raw source text.
             * The Lava parsing process is deferred to the Fluid rendering phase, because we need access to the template context that holds the template source document.
             *
             * Here, we simply add a Fluid statement to call a method to parse and render the block.
             * 
             * The source text will be passed to the Lava block during the rendering phase so it can be parsed and rendered at the same time.
            */
            //_rootNode = node;

            var blockName = node.Term.Name;
            
            var factoryMethod = _factoryMethods[blockName];

            System.Diagnostics.Debug.Print( $"Parsing FluidBlockProxy. [Id={Id.ToString()}, BlockName={blockName}, ThreadId={System.Threading.Thread.CurrentThread.ManagedThreadId}]" );

            var lavaBlock = factoryMethod( blockName );

            _sourceElementName = lavaBlock.SourceElementName;



            // Get the markup for the block attributes.
            var argsNode = context.CurrentBlock.Tag.ChildNodes[0].ChildNodes[0];

            var blockAttributesMarkup = argsNode.FindTokenAndGetText().Trim();

            var statements = context.CurrentBlock.Statements;

            // Custom blocks expect to receive the full set of tokens for the block excluding the opening tag.
            var tokens = new List<string>();
            
            tokens.Add( context.CurrentBlock.AdditionalData.InnerText );
            tokens.Add( context.CurrentBlock.AdditionalData.CloseTag );
            
            var renderBlockDelegate = new DelegateStatement( ( writer, encoder, ctx ) => WriteToAsync( writer, encoder, ctx, lavaBlock, blockName, blockAttributesMarkup, tokens, statements ) );

            return renderBlockDelegate; 
        }

        private ValueTask<Completion> WriteToAsync( TextWriter writer, TextEncoder encoder, TemplateContext context, IRockLavaBlock lavaBlock, string blockName, string blockAttributesMarkup, List<string> tokens, List<Statement> statements )
        {
            System.Diagnostics.Debug.Print( $"WriteToAsync FluidBlockProxy. [Id={Id}, BlockName={blockName}, Attributes={blockAttributesMarkup}, Tokens={tokens.Count}, ThreadId={System.Threading.Thread.CurrentThread.ManagedThreadId}]" );

            var lavaContext = new FluidLavaContext( context );

            var elementRenderer = lavaBlock as ILiquidFrameworkElementRenderer;

            if ( elementRenderer == null )
            {
                throw new Exception( "Block proxy cannot be rendered." );
            }

            // Initialize the block, then allow it to post-process the tokens parsed from the source template.
            lavaBlock.OnInitialize( blockName, blockAttributesMarkup, tokens );

            // Earlier implementations of Lava required that the document tokens passed to the block be consumed as they are processed.
            // This function is called each time the block is rendered, so we pass a copy of the token list to preserve compatibility
            // with custom blocks that implement this behavior.
            // TODO: Why? Is this actually the same behavior as the DotLiquid library?

            var parseTokens = tokens.ToList();

            lavaBlock.OnParsed( parseTokens );

            // Store the Fluid Statements required to render the block in the template context.
            lavaContext.SetInternalFieldValue( Constants.ContextKeys.SourceTemplateStatements, statements );

            // Execute the block rendering process.
            elementRenderer.Render( this, lavaContext, writer, encoder );

            return new ValueTask<Completion>( Completion.Normal );
        }

        #endregion

        private async ValueTask<Completion> WriteToDefaultAsync( TextWriter writer, TextEncoder encoder, TemplateContext context, List<Statement> statements )
        {
            Completion completion;

            if ( encoder == null )
            {
                encoder = global::Fluid.NullEncoder.Default;
            }

            foreach ( var statement in statements )
            {
                completion = await statement.WriteToAsync( writer, encoder, context );

                if ( completion != Completion.Normal )
                {
                    // Stop processing the block statements
                    return completion;
                }
            }

            return Completion.Normal;
        }

        #region ILiquidFrameworkRenderer implementation

        void ILiquidFrameworkElementRenderer.Render( ILiquidFrameworkElementRenderer baseRenderer, ILavaContext context, TextWriter writer, TextEncoder encoder )
        {
            var fluidContext = ( (FluidLavaContext)context ).FluidContext;

            var statements = context.GetInternalFieldValue( Constants.ContextKeys.SourceTemplateStatements ) as List<Statement>;

            var result = WriteToDefaultAsync( writer, encoder, fluidContext, statements );
        }

        public void OnStartup()
        {
            throw new NotImplementedException( "The OnStartup method is not a valid operation for the DotLiquidBlockProxy." );
        }

        void ILiquidFrameworkElementRenderer.Parse( ILiquidFrameworkElementRenderer baseRenderer, List<string> tokens, out List<object> nodes )
        {
            // TODO: May need to rework this?

            nodes = null;
        }

        #endregion
    }
}
