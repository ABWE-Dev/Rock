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

namespace Rock.Lava
{
    /// <summary>
    /// Provides base functionality for a Lava template.
    /// </summary>
    public abstract class LavaTemplateBase : ILavaTemplate
    {
        //public abstract ILavaEngine LavaEngine { get; }

        /// <summary>
        /// The set of Lava commands permitted for this template.
        /// </summary>
        [Obsolete("Do Lava Commands need to be enabled (and cached) for a Template, or only at the context level?")]
        public IList<string> EnabledCommands { get; set; }

        /// <summary>
        /// Try to render the template.
        /// Errors will be included in the rendered output.
        /// </summary>
        /// <returns></returns>        
        public string Render()
        {
            return Render( null );
        }

        /// <summary>
        /// Try to render the template using the provided context values.
        /// Errors will be included in the rendered output.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>        
        public string Render( IDictionary<string, object> values )
        {
            string output;
            IList<Exception> errors;

            var isValid = TryRender( values, out output, out errors );

            if ( !isValid )
            {
                // Append error messages to the output.
                foreach ( var error in errors )
                {
                    output += "\nLiquid Error: " + error.Message;
                }
            }

            return output;           
        }

        //[Obsolete("Not used?")]
        //public abstract void SetContextValue( string key, object value );

        public bool TryRender( IDictionary<string, object> values, out string output, out IList<Exception> errors )
        {
            if ( values == null )
            {
                values = new Dictionary<string, object>();
            }

            // Add the EnabledCommands setting to the context.
            values["EnabledCommands"] = this.EnabledCommands;

            var parameters = new LavaRenderParameters();

            parameters.LocalVariables = values;

            return OnTryRender( parameters, out output, out errors );
        }

        public bool TryRender( LavaRenderParameters parameters, out string output, out IList<Exception> errors )
        {
            if ( parameters == null )
            {
                parameters = new LavaRenderParameters();
            }

            return OnTryRender( parameters, out output, out errors );
        }

        public bool TryRender( ILavaContext context, out string output, out IList<Exception> errors )
        {
            //if ( parameters == null )
            //{
            //    parameters = new LavaRenderParameters();
            //}

            return OnTryRender( context, out output, out errors );
        }

        /// <summary>
        /// Override this method to implement the rendering using a Liquid rendering engine implementation.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="output"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        protected abstract bool OnTryRender( LavaRenderParameters parameters, out string output, out IList<Exception> errors );
        protected abstract bool OnTryRender( ILavaContext context, out string output, out IList<Exception> errors );

    }
}
