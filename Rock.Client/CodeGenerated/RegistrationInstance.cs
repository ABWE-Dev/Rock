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
    /// Base client model for RegistrationInstance that only includes the non-virtual fields. Use this for PUT/POSTs
    /// </summary>
    public partial class RegistrationInstanceEntity
    {
        /// <summary />
        public int Id { get; set; }

        /// <summary />
        public int? AccountId { get; set; }

        /// <summary />
        public string AdditionalConfirmationDetails { get; set; }

        /// <summary />
        public string AdditionalReminderDetails { get; set; }

        /// <summary />
        public string ContactEmail { get; set; }

        /// <summary />
        public int? ContactPersonAliasId { get; set; }

        /// <summary />
        public string ContactPhone { get; set; }

        /// <summary />
        public decimal? Cost { get; set; }

        /// <summary />
        public decimal? DefaultPayment { get; set; }

        /// <summary />
        public string Details { get; set; }

        /// <summary />
        public DateTime? EndDateTime { get; set; }

        /// <summary />
        public int? ExternalGatewayFundId { get; set; }

        /// <summary />
        public int? ExternalGatewayMerchantId { get; set; }

        /// <summary />
        public Guid? ForeignGuid { get; set; }

        /// <summary />
        public string ForeignKey { get; set; }

        /// <summary />
        public bool IsActive { get; set; } = true;

        /// <summary />
        public int? MaxAttendees { get; set; }

        /// <summary />
        public decimal? MinimumInitialPayment { get; set; }

        /// <summary>
        /// If the ModifiedByPersonAliasId is being set manually and should not be overwritten with current user when saved, set this value to true
        /// </summary>
        public bool ModifiedAuditValuesAlreadyUpdated { get; set; }

        /// <summary />
        public string Name { get; set; }

        /// <summary />
        public string PaymentRedirectData { get; set; }

        /// <summary />
        public string RegistrationInstructions { get; set; }

        /// <summary />
        public int? RegistrationMeteringThreshold { get; set; }

        /// <summary />
        public int RegistrationTemplateId { get; set; }

        /// <summary />
        public int? RegistrationWorkflowTypeId { get; set; }

        /// <summary />
        public bool ReminderSent { get; set; }

        /// <summary />
        public DateTime? SendReminderDateTime { get; set; }

        /// <summary />
        public DateTime? StartDateTime { get; set; }

        /// <summary />
        public bool TimeoutIsEnabled { get; set; }

        /// <summary />
        public int? TimeoutLengthMinutes { get; set; }

        /// <summary />
        public int? TimeoutThreshold { get; set; }

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
        /// Copies the base properties from a source RegistrationInstance object
        /// </summary>
        /// <param name="source">The source.</param>
        public void CopyPropertiesFrom( RegistrationInstance source )
        {
            this.Id = source.Id;
            this.AccountId = source.AccountId;
            this.AdditionalConfirmationDetails = source.AdditionalConfirmationDetails;
            this.AdditionalReminderDetails = source.AdditionalReminderDetails;
            this.ContactEmail = source.ContactEmail;
            this.ContactPersonAliasId = source.ContactPersonAliasId;
            this.ContactPhone = source.ContactPhone;
            this.Cost = source.Cost;
            this.DefaultPayment = source.DefaultPayment;
            this.Details = source.Details;
            this.EndDateTime = source.EndDateTime;
            this.ExternalGatewayFundId = source.ExternalGatewayFundId;
            this.ExternalGatewayMerchantId = source.ExternalGatewayMerchantId;
            this.ForeignGuid = source.ForeignGuid;
            this.ForeignKey = source.ForeignKey;
            this.IsActive = source.IsActive;
            this.MaxAttendees = source.MaxAttendees;
            this.MinimumInitialPayment = source.MinimumInitialPayment;
            this.ModifiedAuditValuesAlreadyUpdated = source.ModifiedAuditValuesAlreadyUpdated;
            this.Name = source.Name;
            this.PaymentRedirectData = source.PaymentRedirectData;
            this.RegistrationInstructions = source.RegistrationInstructions;
            this.RegistrationMeteringThreshold = source.RegistrationMeteringThreshold;
            this.RegistrationTemplateId = source.RegistrationTemplateId;
            this.RegistrationWorkflowTypeId = source.RegistrationWorkflowTypeId;
            this.ReminderSent = source.ReminderSent;
            this.SendReminderDateTime = source.SendReminderDateTime;
            this.StartDateTime = source.StartDateTime;
            this.TimeoutIsEnabled = source.TimeoutIsEnabled;
            this.TimeoutLengthMinutes = source.TimeoutLengthMinutes;
            this.TimeoutThreshold = source.TimeoutThreshold;
            this.CreatedDateTime = source.CreatedDateTime;
            this.ModifiedDateTime = source.ModifiedDateTime;
            this.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            this.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            this.Guid = source.Guid;
            this.ForeignId = source.ForeignId;

        }
    }

    /// <summary>
    /// Client model for RegistrationInstance that includes all the fields that are available for GETs. Use this for GETs (use RegistrationInstanceEntity for POST/PUTs)
    /// </summary>
    public partial class RegistrationInstance : RegistrationInstanceEntity
    {
        /// <summary />
        public FinancialAccount Account { get; set; }

        /// <summary />
        public PersonAlias ContactPersonAlias { get; set; }

        /// <summary />
        public RegistrationTemplate RegistrationTemplate { get; set; }

        /// <summary />
        public WorkflowType RegistrationWorkflowType { get; set; }

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
