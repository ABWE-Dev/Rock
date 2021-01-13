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
using System.Collections.Generic;
using System.Diagnostics;
using Fluid.Ast;
using Irony.Parsing;
using Microsoft.Extensions.Primitives;


namespace Rock.Lava.Fluid
{
    public class LavaFluidParserContext
    {

        internal Stack<BlockContextEx> _blocks { get; } = new Stack<BlockContextEx>();

        public BlockContextEx CurrentBlock { get; private set; } = new BlockContextEx( null );



        /// <summary>
        /// Invoked when a block is entered to create a new statements context
        /// which will received all subsequent statements.
        /// </summary>
        //public void EnterBlock( ParseTreeNode tag )
        //{
            
        //}

        public void EnterBlock( ParseTreeNode tag, StringSegment segment, int start, int end )
        {
            EnterBlock( tag, start, segment.Substring( start, end - start + 1 ) );
        }

        /// <summary>
        /// Invoked when a block is entered to create a new statements context
        /// which will received all subsequent statements.
        /// </summary>
        public void EnterBlock( ParseTreeNode tag, int startPosition, string openTagText )
        {
            // Push the current block info to the stack.
            _blocks.Push( CurrentBlock );

            // Start a new block context.
            CurrentBlock = new BlockContextEx( tag );

            var newBlockInfo = new BlockInfo
            {
                StartPosition = startPosition,
                OpenTag = openTagText
            };

            CurrentBlock.AdditionalData = newBlockInfo;
        }

        /// <summary>
        /// Invoked when a section is entered to create a new statements context
        /// which will received all subsequent statements.
        /// </summary>
        public void EnterBlockSection( string name, TagStatement statement )
        {
            Debug.Print( $"Entering Block: ThreadId={System.Threading.Thread.CurrentThread.ManagedThreadId}, CurrentBlockCount={_blocks.Count}" );

            CurrentBlock.EnterBlock( name, statement );
        }

        /// <summary>
        /// Invoked when the end of a block has been reached.
        /// It resets the current statements context to the outer block.
        /// </summary>
        public void ExitBlock()
        {
            Debug.Print( $"Exiting Block: ThreadId={System.Threading.Thread.CurrentThread.ManagedThreadId}, CurrentBlockCount={_blocks.Count}" );

            CurrentBlock = _blocks.Pop();
        }
    }
}
