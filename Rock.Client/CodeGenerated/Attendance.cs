//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
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


namespace Rock.Client
{
    /// <summary>
    /// Base client model for Attendance that only includes the non-virtual fields. Use this for PUT/POSTs
    /// </summary>
    public partial class AttendanceEntity
    {
        /// <summary />
        public int Id { get; set; }

        /// <summary />
        public int? AttendanceCheckInSessionId { get; set; }

        /// <summary />
        public int? AttendanceCodeId { get; set; }

        /// <summary />
        public int? CampusId { get; set; }

        /// <summary />
        public int? CheckedInByPersonAliasId { get; set; }

        /// <summary />
        public int? CheckedOutByPersonAliasId { get; set; }

        /// <summary />
        public int? DeclineReasonValueId { get; set; }

        /// <summary />
        public int? DeviceId { get; set; }

        /// <summary />
        public bool? DidAttend { get; set; } = true;

        /// <summary />
        // Made Obsolete in Rock "1.8"
        [Obsolete( "Use Occurrence.DidNotOccur instead", true )]
        public bool? DidNotOccur { get; set; }

        /// <summary />
        public DateTime? EndDateTime { get; set; }

        /// <summary />
        public Guid? ForeignGuid { get; set; }

        /// <summary />
        public string ForeignKey { get; set; }

        /// <summary />
        // Made Obsolete in Rock "1.8"
        [Obsolete( "Use Occurrence.GroupId instead", true )]
        public int? GroupId { get; set; }

        /// <summary />
        // Made Obsolete in Rock "1.8"
        [Obsolete( "Use Occurrence.LocationId instead", true )]
        public int? LocationId { get; set; }

        /// <summary>
        /// If the ModifiedByPersonAliasId is being set manually and should not be overwritten with current user when saved, set this value to true
        /// </summary>
        public bool ModifiedAuditValuesAlreadyUpdated { get; set; }

        /// <summary />
        public string Note { get; set; }

        /// <summary />
        public int OccurrenceId { get; set; }

        /// <summary />
        public int? PersonAliasId { get; set; }

        /// <summary />
        public int? PresentByPersonAliasId { get; set; }

        /// <summary />
        public DateTime? PresentDateTime { get; set; }

        /// <summary />
        public bool? Processed { get; set; }

        /// <summary />
        public int? QualifierValueId { get; set; }

        /// <summary />
        public bool? RequestedToAttend { get; set; }

        /// <summary />
        public Rock.Client.Enums.RSVP RSVP { get; set; } = Rock.Client.Enums.RSVP.Unknown;

        /// <summary />
        public DateTime? RSVPDateTime { get; set; }

        /// <summary />
        public bool? ScheduleConfirmationSent { get; set; }

        /// <summary />
        public int? ScheduledByPersonAliasId { get; set; }

        /// <summary />
        public bool? ScheduledToAttend { get; set; }

        /// <summary />
        // Made Obsolete in Rock "1.8"
        [Obsolete( "Use Occurrence.ScheduleId instead", true )]
        public int? ScheduleId { get; set; }

        /// <summary />
        public bool? ScheduleReminderSent { get; set; }

        /// <summary />
        public int? SearchResultGroupId { get; set; }

        /// <summary />
        public int? SearchTypeValueId { get; set; }

        /// <summary />
        public string SearchValue { get; set; }

        /// <summary />
        public DateTime StartDateTime { get; set; }

        /// <summary />
        // Made Obsolete in Rock "1.8"
        [Obsolete( "Use Occurrence.SundayDate instead", true )]
        public DateTime SundayDate { get; set; }

        /// <summary>
        /// Leave this as NULL to let Rock set this
        /// </summary>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// This does not need to be set or changed. Rock will always set this to the current date/time when saved to the database.
        /// </summary>
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Leave this as NULL to let Rock set this
        /// </summary>
        public int? CreatedByPersonAliasId { get; set; }

        /// <summary>
        /// If you need to set this manually, set ModifiedAuditValuesAlreadyUpdated=True to prevent Rock from setting it
        /// </summary>
        public int? ModifiedByPersonAliasId { get; set; }

        /// <summary />
        public Guid Guid { get; set; }

        /// <summary />
        public int? ForeignId { get; set; }

        /// <summary>
        /// Copies the base properties from a source Attendance object
        /// </summary>
        /// <param name="source">The source.</param>
        public void CopyPropertiesFrom( Attendance source )
        {
            this.Id = source.Id;
            this.AttendanceCheckInSessionId = source.AttendanceCheckInSessionId;
            this.AttendanceCodeId = source.AttendanceCodeId;
            this.CampusId = source.CampusId;
            this.CheckedInByPersonAliasId = source.CheckedInByPersonAliasId;
            this.CheckedOutByPersonAliasId = source.CheckedOutByPersonAliasId;
            this.DeclineReasonValueId = source.DeclineReasonValueId;
            this.DeviceId = source.DeviceId;
            this.DidAttend = source.DidAttend;
            this.EndDateTime = source.EndDateTime;
            this.ForeignGuid = source.ForeignGuid;
            this.ForeignKey = source.ForeignKey;
            this.ModifiedAuditValuesAlreadyUpdated = source.ModifiedAuditValuesAlreadyUpdated;
            this.Note = source.Note;
            this.OccurrenceId = source.OccurrenceId;
            this.PersonAliasId = source.PersonAliasId;
            this.PresentByPersonAliasId = source.PresentByPersonAliasId;
            this.PresentDateTime = source.PresentDateTime;
            this.Processed = source.Processed;
            this.QualifierValueId = source.QualifierValueId;
            this.RequestedToAttend = source.RequestedToAttend;
            this.RSVP = source.RSVP;
            this.RSVPDateTime = source.RSVPDateTime;
            this.ScheduleConfirmationSent = source.ScheduleConfirmationSent;
            this.ScheduledByPersonAliasId = source.ScheduledByPersonAliasId;
            this.ScheduledToAttend = source.ScheduledToAttend;
            this.ScheduleReminderSent = source.ScheduleReminderSent;
            this.SearchResultGroupId = source.SearchResultGroupId;
            this.SearchTypeValueId = source.SearchTypeValueId;
            this.SearchValue = source.SearchValue;
            this.StartDateTime = source.StartDateTime;
            this.CreatedDateTime = source.CreatedDateTime;
            this.ModifiedDateTime = source.ModifiedDateTime;
            this.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            this.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            this.Guid = source.Guid;
            this.ForeignId = source.ForeignId;

        }
    }

    /// <summary>
    /// Client model for Attendance that includes all the fields that are available for GETs. Use this for GETs (use AttendanceEntity for POST/PUTs)
    /// </summary>
    public partial class Attendance : AttendanceEntity
    {
        /// <summary />
        public AttendanceCode AttendanceCode { get; set; }

        /// <summary />
        public PersonAlias CheckedOutByPersonAlias { get; set; }

        /// <summary />
        public DefinedValue DeclineReasonValue { get; set; }

        /// <summary />
        public Device Device { get; set; }

        /// <summary />
        public PersonAlias PresentByPersonAlias { get; set; }

        /// <summary />
        public DefinedValue Qualifier { get; set; }

        /// <summary />
        public PersonAlias ScheduledByPersonAlias { get; set; }

        /// <summary />
        public DefinedValue SearchTypeValue { get; set; }

        /// <summary>
        /// NOTE: Attributes are only populated when ?loadAttributes is specified. Options for loadAttributes are true, false, 'simple', 'expanded' 
        /// </summary>
        public Dictionary<string, Rock.Client.Attribute> Attributes { get; set; }

        /// <summary>
        /// NOTE: AttributeValues are only populated when ?loadAttributes is specified. Options for loadAttributes are true, false, 'simple', 'expanded' 
        /// </summary>
        public Dictionary<string, Rock.Client.AttributeValue> AttributeValues { get; set; }
    }
}
