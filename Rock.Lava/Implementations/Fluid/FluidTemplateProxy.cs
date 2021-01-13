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
using Fluid;
using Fluid.Ast;
using Rock.Lava;
using Rock.Lava.DotLiquid;
using Rock.Lava.Fluid;
using Rock.Lava.Shortcodes;

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// A wrapper for a Fluid Template for use with the Rock Lava library.
    /// </summary>
    /// <remarks>
    /// This class should exist in the Rock.Lava.Fluid library.
    /// </remarks>
    public class FluidTemplateProxy : LavaTemplateBase
    {
        #region Constructors

        private LavaFluidTemplate _template;

        //public override ILavaEngine LavaEngine => throw new NotImplementedException();

        public FluidTemplateProxy( LavaFluidTemplate template )
        {
            _template = template;
        }

        #endregion

        public LavaFluidTemplate FluidTemplate
        {
            get
            {
                return _template;
            }
        }

        //public void Dispose()
        //{
        //    //
        //}

        //public void RegisterSafeType( Type type, string[] allowedMembers = null )
        //{
        //    // Not required?
        //}

        //[Obsolete]
        //public override void SetContextValue( string key, object value )
        //{
        //    throw new NotImplementedException();
        //}

        protected override bool OnTryRender( ILavaContext context, out string output, out IList<Exception> errors )
        {
            output = null;
            errors = new List<Exception>();

            var proxyContext = context as FluidLavaContext;

            var fluidTemplateContext = proxyContext?.FluidContext;

            // Copy the local variables into the Fluid Template context.
            // TODO: What about InstanceAssigns and Registers?
            //if ( parameters.LocalVariables != null )
            //{
            //    foreach ( var p in parameters.LocalVariables )
            //    {
            //        templateContext.SetValue( p.Key, p.Value );
            //    }
            //}

            try
            {
                if ( context != null && fluidTemplateContext == null )
                {
                    throw new LavaException( "Invalid context type." );
                }

                output = _template.Render( fluidTemplateContext );
            }
            catch ( Exception ex )
            {
                errors.Add( ex );

                return false;
            }

            return true;
        }

        protected override bool OnTryRender( LavaRenderParameters parameters, out string output, out IList<Exception> errors )
        {
            output = null;
            errors = new List<Exception>();

            var templateContext = new TemplateContext();

            // Copy the local variables into the Fluid Template context.
            // TODO: What about InstanceAssigns and Registers?
            if ( parameters.LocalVariables != null )
            {
                foreach ( var p in parameters.LocalVariables )
                {
                    templateContext.SetValue( p.Key, p.Value );
                }
            }

            try
            {
                output = _template.Render( templateContext );
            }
            catch (Exception ex)
            {
                errors.Add( ex );

                return false;
            }
            
            return true;
        }
    }
}
