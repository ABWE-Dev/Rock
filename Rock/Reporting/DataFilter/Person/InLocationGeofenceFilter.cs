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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using System.Web.UI.WebControls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people on whether any of their family's map locations are within the geofenced boundary of the specified location" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person In Location Geofence Filter" )]
    public class InLocationGeofenceFilter : DataFilterComponent
    {
        #region Properties

        /// <summary>
        /// Gets the entity type that filter applies to.
        /// </summary>
        /// <value>
        /// The entity that filter applies to.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return typeof( Rock.Model.Person ).FullName; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Additional Filters"; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        /// </value>
        public override string GetTitle( Type entityType )
        {
            return "In Location Geofence";
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before 
        /// referencing this property.
        /// </summary>
        /// <value>
        /// The client format script.
        /// </value>
        public override string GetClientFormatSelection( Type entityType )
        {
            return @"
function() {
  var locationName = $('.location-picker', $content).find('.selected-names').text();
  var result = 'In location geofence ' + locationName;
  return result;
}
";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string result = "In location geofence";

            var rockContext = new RockContext();
            var location = new LocationService( rockContext ).Get( selection.AsGuid() );
            if ( location != null && !string.IsNullOrWhiteSpace( location.Name ) )
            {
                result += string.Format( ": {0}", location.Name );
            }

            return result;
        }

        /// <summary>
        /// The LocationPicker
        /// </summary>
        private LocationPicker lp = null;

        private RockDropDownList ddlLocationType = null;

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            lp = new LocationPicker();
            lp.ID = filterControl.ID + "_lp";
            lp.Label = "Location";
            lp.AllowedPickerModes = LocationPickerMode.Named | LocationPickerMode.Polygon;
            lp.CurrentPickerMode = lp.GetBestPickerModeForLocation( null );
            lp.CssClass = "col-lg-4";
            filterControl.Controls.Add( lp );

            Panel panel = new Panel();
            panel.CssClass = "col-lg-8";
            filterControl.Controls.Add( panel );

            ddlLocationType = new RockDropDownList();
            ddlLocationType.ID = filterControl.ID + "_ddlLocationType";
            ddlLocationType.Label = "Location Type";
            ddlLocationType.DataValueField = "Id";
            ddlLocationType.DataTextField = "Value";
            DefinedTypeCache locationDefinedType = DefinedTypeCache.Read( SystemGuid.DefinedType.GROUP_LOCATION_TYPE.AsGuid() );
            ddlLocationType.BindToDefinedType( locationDefinedType );
            ddlLocationType.Items.Insert( 0, new ListItem( "(All Location Types)", "" ) );
            panel.Controls.Add( ddlLocationType );

            return new Control[3] { lp, ddlLocationType, panel };
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            if ( controls.Count() >= 3 )
            {
                ( controls[0] as LocationPicker ).RenderControl( writer );
                ( controls[2] as Panel ).RenderControl( writer );
            }
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</parm>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var location = ( controls[0] as LocationPicker ).Location;
            var locationTypeId = ( controls[1] as RockDropDownList ).SelectedValue;

            var locationGuid = location != null ? location.Guid : Guid.Empty;

            return string.Format( "{0}|{1}", locationGuid, locationTypeId );
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var selections = selection.SplitDelimitedValues();
            Guid locationGuid = selections[0].AsGuid();

            var location = new LocationService( new RockContext() ).Get( locationGuid );
            if ( location != null )
            {
                LocationPicker locationPicker = controls[0] as LocationPicker;
                locationPicker.GetBestPickerModeForLocation( location );
                locationPicker.Location = location;
            }

            if ( selections.Length >= 2 )
            {
                ( controls[1] as RockDropDownList ).SetValue( selections[1] );
            }
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var selections = selection.SplitDelimitedValues();
            Guid locationGuid = selections[0].AsGuid();

            RockContext rockContext = ( RockContext ) serviceInstance.Context;

            DbGeography geoFence = new LocationService( rockContext )
                .Get( locationGuid ).GeoFence;

            Guid familyGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
            int familyGroupTypeId = new GroupTypeService( rockContext ).Get( familyGroupTypeGuid ).Id;

            var groupLocationQry = new GroupLocationService( rockContext )
                .GetMappedLocationsByGeofences( new List<DbGeography> { geoFence } )
                .Where( gl => gl.Group.GroupType.Id == familyGroupTypeId );

            if ( selections.Length >= 2 )
            {
                var locationTypeId = selections[1].AsInteger();
                groupLocationQry = groupLocationQry.Where( gl => gl.GroupLocationTypeValueId == locationTypeId );
            }

            var groupMemberQry = groupLocationQry.SelectMany( g => g.Group.Members );

            var qry = new PersonService( rockContext ).Queryable()
                .Where( p => groupMemberQry.Any( xx => xx.PersonId == p.Id ) );

            Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

            return extractedFilterExpression;
        }

        #endregion
    }
}