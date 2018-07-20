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
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using RestSharp;

using Rock.Attribute;
using Rock.Financial;
using Rock.Model;
using Rock.Cache;
using Rock.Security;

namespace Rock.NMI
{
    /// <summary>
    /// NMI Payment Gateway
    /// </summary>
    [Description( "NMI Gateway" )]
    [Export( typeof( GatewayComponent ) )]
    [ExportMetadata( "ComponentName", "NMI Gateway" )]

    [TextField( "Security Key", "The API key", true, "", "", 0 )]
    [TextField( "Admin Username", "The username of an NMI user", true, "", "", 1 )]
    [TextField( "Admin Password", "The password of an NMI user", true, "", "", 2, "AdminPassword", true )]
    [TextField( "Three Step API URL", "The URL of the NMI Three Step API", true, "https://secure.networkmerchants.com/api/v2/three-step", "", 3, "APIUrl" )]
    [TextField( "Query API URL", "The URL of the NMI Query API", true, "https://secure.networkmerchants.com/api/query.php", "", 4, "QueryUrl" )]
    [BooleanField( "Prompt for Name On Card", "Should users be prompted to enter name on the card", false, "", 5, "PromptForName" )]
    [BooleanField( "Prompt for Billing Address", "Should users be prompted to enter billing address", false, "", 7, "PromptForAddress" )]
    public class Gateway : ThreeStepGatewayComponent
    {

        #region Gateway Component Implementation

        /// <summary>
        /// Gets the step2 form URL.
        /// </summary>
        /// <value>
        /// The step2 form URL.
        /// </value>
        public override string Step2FormUrl
        {
            get
            {
                return string.Format( "~/NMIGatewayStep2.html?timestamp={0}", RockDateTime.Now.Ticks );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the gateway requires the name on card for CC processing
        /// </summary>
        /// <value>
        /// <c>true</c> if [name on card required]; otherwise, <c>false</c>.
        /// </value>
        public override bool PromptForNameOnCard( FinancialGateway financialGateway )
        {
            return GetAttributeValue( financialGateway, "PromptForName" ).AsBoolean();
        }

        /// <summary>
        /// Gets a value indicating whether gateway provider needs first and last name on credit card as two distinct fields.
        /// </summary>
        /// <value>
        /// <c>true</c> if [split name on card]; otherwise, <c>false</c>.
        /// </value>
        public override bool SplitNameOnCard
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Prompts for the person name associated with a bank account.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <returns></returns>
        public override bool PromptForBankAccountName( FinancialGateway financialGateway )
        {
            return true;
        }

        /// <summary>
        /// Gets a value indicating whether [address required].
        /// </summary>
        /// <value>
        /// <c>true</c> if [address required]; otherwise, <c>false</c>.
        /// </value>
        public override bool PromptForBillingAddress( FinancialGateway financialGateway )
        {
            return GetAttributeValue( financialGateway, "PromptForAddress" ).AsBoolean();
        }

        /// <summary>
        /// Gets the supported payment schedules.
        /// </summary>
        /// <value>
        /// The supported payment schedules.
        /// </value>
        public override List<CacheDefinedValue> SupportedPaymentSchedules
        {
            get
            {
                var values = new List<CacheDefinedValue>();
                values.Add( CacheDefinedValue.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME ) );
                values.Add( CacheDefinedValue.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_WEEKLY ) );
                values.Add( CacheDefinedValue.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_BIWEEKLY ) );
                values.Add( CacheDefinedValue.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY ) );
                return values;
            }
        }

        /// <summary>
        /// Returnes a boolean value indicating if 'Saved Account' functionality is supported for frequency (i.e. one-time vs repeating )
        /// </summary>
        /// <param name="isRepeating">if set to <c>true</c> [is repeating].</param>
        /// <returns></returns>
        public override bool SupportsSavedAccount( bool isRepeating )
        {
            return true;
        }
        
        /// <summary>
        /// Gets the financial transaction parameters that are passed to step 1
        /// </summary>
        /// <param name="redirectUrl">The redirect URL.</param>
        /// <returns></returns>
        public override Dictionary<string, string> GetStep1Parameters( string redirectUrl )
        {
            var parameters = new Dictionary<string, string>();
            parameters.Add( "redirect-url", redirectUrl );
            return parameters;
        }

        /// <summary>
        /// Gets the financial transaction parameters that are passed to step 3
        /// </summary>
        /// <param name="paymentInfo">The payment information.</param>
        /// <returns></returns>
        public override Dictionary<string, string> GetStep3Parameters( PaymentInfo paymentInfo )
        {
            var parameters = new Dictionary<string, string>();

            var referencePayment = paymentInfo as ReferencePaymentInfo;
            if ( referencePayment != null )
            {
                parameters.Add( "customer-vault-id", referencePayment.ReferenceNumber );
            }
            return parameters;
        }

        /// <summary>
        /// Charges the specified payment info.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override FinancialTransaction Charge( FinancialGateway financialGateway, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = "The Payment Gateway only supports a three-step charge.";
            return null;
        }

        /// <summary>
        /// Performs the first step of a three-step charge
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        /// Url to post the Step2 request to
        /// </returns>
        /// <exception cref="System.ArgumentNullException">paymentInfo</exception>
        public override string ChargeStep1( FinancialGateway financialGateway, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;

            try
            {
                // Make sure valid parameters were passed
                if ( financialGateway == null )
                {
                    throw new ArgumentNullException( "finacialGateway" );
                }
                if ( paymentInfo == null )
                {
                    throw new ArgumentNullException( "paymentInfo" );
                }

                // If this is a reference payment, the three-step process is not required, so just return an empty 
                //string so that block knows to skip to step 3
                if ( paymentInfo is ReferencePaymentInfo )
                {
                    return string.Empty;
                }

                // Create XML for a Hybrid Validate/AddCustomer Transaction (First Step)
                XElement rootElement = GetRoot( financialGateway, "validate", paymentInfo );
                rootElement.Add(
                    new XElement( "add-customer" ),
                    new XElement( "ip-address", paymentInfo.IPAddress ),
                    new XElement( "order-description", paymentInfo.Description ) );
                rootElement.Add( GetBilling( paymentInfo ) );

                // Post the XML
                XDocument xdoc = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), rootElement );
                var result = PostToGateway( financialGateway, xdoc, out errorMessage );

                // If result was valid, return the url that Step 2 should post to
                if ( result != null )
                {
                    return result.GetValueOrNull( "form-url" );
                }

                return null;
            }

            catch ( WebException webException )
            {
                string message = GetResponseMessage( webException.Response.GetResponseStream() );
                errorMessage = webException.Message + " - " + message;
                return null;
            }

            catch ( Exception ex )
            {
                errorMessage = ex.Message;
                return null;
            }

        }

        /// <summary>
        /// Performs the final step of a three-step charge.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="resultQueryString">The result query string from step 2.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override FinancialTransaction ChargeStep3( FinancialGateway financialGateway, string resultQueryString, out string errorMessage )
        {
            return ChargeStep3( financialGateway, null, resultQueryString, out errorMessage );
        }

        /// <summary>
        /// Performs the final step of a three-step charge. This method includes a PaymentInfo object that can be used if needed to finalize any transaction.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="resultQueryString">The result query string.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override FinancialTransaction ChargeStep3( FinancialGateway financialGateway, PaymentInfo paymentInfo, string resultQueryString, out string errorMessage )
        {
            errorMessage = string.Empty;

            try
            {
                // Make sure valid parameters were passed
                if ( financialGateway == null )
                {
                    throw new ArgumentNullException( "finacialGateway" );
                }
                if ( paymentInfo == null )
                {
                    throw new ArgumentNullException( "paymentInfo" );
                }

                // Check to see if third step is not needed or succesfully completed
                if ( !CompleteThirdStep( financialGateway, paymentInfo, resultQueryString, out errorMessage ))
                {
                    return null;
                }

                // Create the XML for a Sale transaction
                var rootElement = GetRoot( financialGateway, "sale", paymentInfo );
                rootElement.Add(
                    new XElement( "ip-address", paymentInfo.IPAddress ),
                    new XElement( "currency", "USD" ),
                    new XElement( "amount", paymentInfo.Amount.ToString() ),
                    new XElement( "order-description", paymentInfo.Description ),
                    new XElement( "tax-amount", "0.00" ),
                    new XElement( "shipping-amount", "0.00" ) );

                // Post the Sale transaction
                var xdoc = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), rootElement );
                var result = PostToGateway( financialGateway, xdoc, out errorMessage );

                // If not valid, return null
                if ( result == null )
                {
                    return null;
                }

                // Otherwise create a transaction record
                var transaction = new FinancialTransaction();
                transaction.TransactionCode = result.GetValueOrNull( "transaction-id" );
                transaction.ForeignKey = paymentInfo.AdditionalParameters.GetValueOrNull( "customer-vault-id" );
                transaction.FinancialPaymentDetail = new FinancialPaymentDetail();

                string ccNumber = result.GetValueOrNull( "billing_cc-number" );
                if ( !string.IsNullOrWhiteSpace( ccNumber ) )
                {
                    // cc payment
                    var curType = CacheDefinedValue.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD );
                    transaction.FinancialPaymentDetail.NameOnCardEncrypted = Encryption.EncryptString( $"{result.GetValueOrNull( "billing_first-name" )} {result.GetValueOrNull( "billing_last-name" )}" );
                    transaction.FinancialPaymentDetail.CurrencyTypeValueId = curType != null ? curType.Id : (int?)null;
                    transaction.FinancialPaymentDetail.CreditCardTypeValueId = CreditCardPaymentInfo.GetCreditCardType( ccNumber.Replace( '*', '1' ).AsNumeric() )?.Id;
                    transaction.FinancialPaymentDetail.AccountNumberMasked = ccNumber.Masked( true );

                    string mmyy = result.GetValueOrNull( "billing_cc-exp" );
                    if ( !string.IsNullOrWhiteSpace( mmyy ) && mmyy.Length == 4 )
                    {
                        transaction.FinancialPaymentDetail.ExpirationMonthEncrypted = Encryption.EncryptString( mmyy.Substring( 0, 2 ) );
                        transaction.FinancialPaymentDetail.ExpirationYearEncrypted = Encryption.EncryptString( mmyy.Substring( 2, 2 ) );
                    }
                }
                else
                {
                    // ach payment
                    var curType = CacheDefinedValue.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH );
                    transaction.FinancialPaymentDetail.CurrencyTypeValueId = curType != null ? curType.Id : (int?)null;
                    transaction.FinancialPaymentDetail.AccountNumberMasked = result.GetValueOrNull( "billing_account-number" ).Masked( true );
                }

                transaction.AdditionalLavaFields = new Dictionary<string,object>();
                foreach( var keyVal in result )
                {
                    transaction.AdditionalLavaFields.Add( keyVal.Key, keyVal.Value );
                }

                return transaction;
            }

            catch ( WebException webException )
            {
                string message = GetResponseMessage( webException.Response.GetResponseStream() );
                errorMessage = webException.Message + " - " + message;
                return null;
            }

            catch ( Exception ex )
            {
                errorMessage = ex.Message;
                return null;
            }

        }

        /// <summary>
        /// Credits (Refunds) the specified transaction.
        /// </summary>
        /// <param name="origTransaction">The original transaction.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="comment">The comment.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override FinancialTransaction Credit( FinancialTransaction origTransaction, decimal amount, string comment, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( origTransaction != null && 
                !string.IsNullOrWhiteSpace( origTransaction.TransactionCode ) &&
                origTransaction.FinancialGateway != null )
            {
                var rootElement = GetRoot( origTransaction.FinancialGateway, "refund", null );
                rootElement.Add( new XElement( "transaction-id", origTransaction.TransactionCode ) );
                rootElement.Add( new XElement( "amount", amount.ToString( "0.00" ) ) );

                XDocument xdoc = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), rootElement );
                var result = PostToGateway( origTransaction.FinancialGateway, xdoc, out errorMessage );

                if ( result == null )
                {
                    return null;
                }

                var transaction = new FinancialTransaction();
                transaction.TransactionCode = result.GetValueOrNull( "transaction-id" );
                return transaction;
            }
            else
            {
                errorMessage = "Invalid original transaction, transaction code, or gateway.";
            }

            return null;
        }

        /// <summary>
        /// Adds the scheduled payment.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="schedule">The schedule.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override FinancialScheduledTransaction AddScheduledPayment( FinancialGateway financialGateway, PaymentSchedule schedule, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = "The Payment Gateway only supports adding scheduled payment using a three-step process.";
            return null;
        }

        /// <summary>
        /// Performs the first step of adding a new payment schedule
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="schedule">The schedule.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">paymentInfo</exception>
        public override string AddScheduledPaymentStep1( FinancialGateway financialGateway, PaymentSchedule schedule, PaymentInfo paymentInfo, out string errorMessage )
        {
            // The Scheduled Payment Step 1 is exactly same as Charge Step 1 (Validate if not using saved account, otherwise return empty string and skip to step 3)
            return ChargeStep1( financialGateway, paymentInfo, out errorMessage );
        }

        /// <summary>
        /// Performs the third step of adding a new payment schedule
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="resultQueryString">The result query string from step 2.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">tokenId</exception>
        public override FinancialScheduledTransaction AddScheduledPaymentStep3( FinancialGateway financialGateway, string resultQueryString, out string errorMessage )
        {
            return AddScheduledPaymentStep3( financialGateway, null, null, resultQueryString, out errorMessage );
        }

        /// <summary>
        /// Performs the third step of adding a new payment schedule. This method includes a PaymentSchedule and PaymentInfo object that can be used if needed to finalize any transaction.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="schedule">The schedule.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="resultQueryString">The result query string.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">paymentInfo</exception>
        public override FinancialScheduledTransaction AddScheduledPaymentStep3( FinancialGateway financialGateway, PaymentSchedule schedule, PaymentInfo paymentInfo, string resultQueryString, out string errorMessage )
        {
            errorMessage = string.Empty;

            try
            {
                // Make sure valid parameters were passed
                if ( financialGateway == null )
                {
                    throw new ArgumentNullException( "finacialGateway" );
                }
                if ( schedule == null )
                {
                    throw new ArgumentNullException( "schedule" );
                }
                if ( paymentInfo == null )
                {
                    throw new ArgumentNullException( "paymentInfo" );
                }

                // Check to see if third step is not needed or succesfully completed
                if ( !CompleteThirdStep( financialGateway, paymentInfo, resultQueryString, out errorMessage ) )
                {
                    return null;
                }

                // Start to create the XML for an Add Subscription txn
                var rootElement = GetRoot( financialGateway, "add-subscription", paymentInfo );
                rootElement.Add(
                    new XElement( "start-date", schedule.StartDate.ToString( "yyyyMMdd" ) ),
                    new XElement( "order-description", paymentInfo.Description ),
                    new XElement( "currency", "USD" ),
                    new XElement( "tax-amount", "0.00" ) );

                // Add the plan details
                rootElement.Add( GetPlan( schedule, paymentInfo ) );

                // Post the Add Subscription transaction
                XDocument xdoc = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), rootElement );
                var result = PostToGateway( financialGateway, xdoc, out errorMessage );

                // If not valid, return null
                if ( result == null )
                {
                    return null;
                }

                // Otherwise create a scheduled transaction record
                var scheduledTransaction = new FinancialScheduledTransaction();
                scheduledTransaction.IsActive = true;
                scheduledTransaction.GatewayScheduleId = result.GetValueOrNull( "subscription-id" );
                scheduledTransaction.FinancialGatewayId = financialGateway.Id;
                scheduledTransaction.ForeignKey = paymentInfo.AdditionalParameters.GetValueOrNull( "customer-vault-id" );
                scheduledTransaction.FinancialPaymentDetail = new FinancialPaymentDetail();
                string ccNumber = result.GetValueOrNull( "billing_cc-number" );
                if ( !string.IsNullOrWhiteSpace( ccNumber ) )
                {
                    // cc payment
                    var curType = CacheDefinedValue.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD );
                    scheduledTransaction.FinancialPaymentDetail.CurrencyTypeValueId = curType != null ? curType.Id : (int?)null;
                    scheduledTransaction.FinancialPaymentDetail.CreditCardTypeValueId = CreditCardPaymentInfo.GetCreditCardType( ccNumber.Replace( '*', '1' ).AsNumeric() )?.Id;
                    scheduledTransaction.FinancialPaymentDetail.AccountNumberMasked = ccNumber.Masked( true );

                    string mmyy = result.GetValueOrNull( "billing_cc-exp" );
                    if ( !string.IsNullOrWhiteSpace( mmyy ) && mmyy.Length == 4 )
                    {
                        scheduledTransaction.FinancialPaymentDetail.ExpirationMonthEncrypted = Encryption.EncryptString( mmyy.Substring( 0, 2 ) );
                        scheduledTransaction.FinancialPaymentDetail.ExpirationYearEncrypted = Encryption.EncryptString( mmyy.Substring( 2, 2 ) );
                    }
                }
                else
                {
                    // ach payment
                    var curType = CacheDefinedValue.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH );
                    scheduledTransaction.FinancialPaymentDetail.CurrencyTypeValueId = curType != null ? curType.Id : (int?)null;
                    scheduledTransaction.FinancialPaymentDetail.AccountNumberMasked = result.GetValueOrNull( "billing_account_number" ).Masked( true );
                }

                GetScheduledPaymentStatus( scheduledTransaction, out errorMessage );

                return scheduledTransaction;
            }

            catch ( WebException webException )
            {
                string message = GetResponseMessage( webException.Response.GetResponseStream() );
                errorMessage = webException.Message + " - " + message;
                return null;
            }

            catch ( Exception ex )
            {
                errorMessage = ex.Message;
                return null;
            }


        }

        /// <summary>
        /// Flag indicating if gateway supports reactivating a scheduled payment.
        /// </summary>
        /// <returns></returns>
        public override bool ReactivateScheduledPaymentSupported
        {
            get { return false; }
        }

        /// <summary>
        /// Reactivates the scheduled payment.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool ReactivateScheduledPayment( FinancialScheduledTransaction transaction, out string errorMessage )
        {
            errorMessage = "The payment gateway associated with this scheduled tranaction (NMI) does not support reactivating scheduled transactions. A new scheduled transaction should be created instead.";
            return false;
        }

        /// <summary>
        /// Flag indicating if gateway supports updating the amount/frequency of a scheduled payment.
        /// </summary>
        public override bool UpdateScheduledPaymentSupported
        {
            get { return true; }
        }

        /// <summary>
        /// Flag indicating if gateway supports updating the payment method of a scheduled payment.
        /// </summary>
        /// <value>
        /// <c>true</c> if [update scheduled payment method supported]; otherwise, <c>false</c>.
        /// </value>
        public override bool UpdateScheduledPaymentMethodSupported
        {
            get { return false; }
        }

        /// <summary>
        /// Updates the scheduled payment.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool UpdateScheduledPayment( FinancialScheduledTransaction transaction, PaymentInfo paymentInfo, out string errorMessage )
        {
            string vaultId = transaction.ForeignKey;
            if ( vaultId.IsNullOrWhiteSpace() )
            {
                errorMessage = "The selected scheduled transaction does not have a valid customer vault id, and can not be updated.";
                return false;
            }

            // First try to delete the previous subscription
            if ( !CancelScheduledPayment( transaction, out errorMessage ))
            {
                // If an error occurred while trying to delete the previous subscription, but it was due to 
                // subscription not existing, that's ok, so don't stop.
                if ( !errorMessage.StartsWith( "No recurring subscriptions found" ) )
                {
                    errorMessage = $"An error occurred trying to delete the original schedule details: {errorMessage}";
                    return false;
                }
            }

            // Start to create the XML for an Add Subscription txn
            var rootElement = GetRoot( transaction.FinancialGateway, "add-subscription", paymentInfo );
            rootElement.Add(
                new XElement( "customer-vault-id", vaultId ),
                new XElement( "start-date", transaction.StartDate.ToString( "yyyyMMdd" ) ),
                new XElement( "order-description", paymentInfo.Description ),
                new XElement( "currency", "USD" ),
                new XElement( "tax-amount", "0.00" ) );

            // Add the plan details
            rootElement.Add( GetPlan( transaction, paymentInfo ) );

            // Post the Add Subscription transaction
            XDocument xdoc = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), rootElement );
            var result = PostToGateway( transaction.FinancialGateway, xdoc, out errorMessage );

            // If not valid, return null
            if ( result == null )
            {
                return false;
            }

            // Otherwise update the current scheduled transaction with the new information
            transaction.IsActive = true;
            transaction.GatewayScheduleId = result.GetValueOrNull( "subscription-id" );

            return true;
        }

        /// <summary>
        /// Performs the first step of adding a new payment schedule
        /// </summary>
        /// <param name="transaction">The scheduled transaction.</param>
        /// <param name="schedule">The schedule.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">paymentInfo</exception>
        public override string UpdateScheduledPaymentStep1( FinancialScheduledTransaction transaction, PaymentSchedule schedule, PaymentInfo paymentInfo, out string errorMessage )
        {
            // The Scheduled Payment Step 1 is exactly same as Charge Step 1 (Validate if not using saved account, otherwise return empty string and skip to step 3)
            return ChargeStep1( transaction.FinancialGateway, paymentInfo, out errorMessage );
        }

        /// <summary>
        /// Performs the third step of adding a new payment schedule
        /// </summary>
        /// <param name="transaction">The scheduled transaction.</param>
        /// <param name="resultQueryString">The result query string from step 2.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">tokenId</exception>
        public override bool UpdateScheduledPaymentStep3( FinancialScheduledTransaction transaction, string resultQueryString, out string errorMessage )
        {
            return UpdateScheduledPaymentStep3( transaction, null, null, resultQueryString, out errorMessage );
        }

        /// <summary>
        /// Performs the third step of adding a new payment schedule. This method includes a PaymentSchedule and PaymentInfo object that can be used if needed to finalize any transaction.
        /// </summary>
        /// <param name="transaction">The scheduled transaction.</param>
        /// <param name="schedule">The schedule.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="resultQueryString">The result query string.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">paymentInfo</exception>
        public override bool UpdateScheduledPaymentStep3( FinancialScheduledTransaction transaction, PaymentSchedule schedule, PaymentInfo paymentInfo, string resultQueryString, out string errorMessage )
        {
            errorMessage = string.Empty;

            string vaultId = transaction.ForeignKey;
            if ( vaultId.IsNullOrWhiteSpace() )
            {
                errorMessage = "The selected scheduled transaction does not have a valid customer vault id, and can not be updated.";
                return null;
            }

            // First try to delete the previous subscription
            if ( !CancelScheduledPayment( transaction, out errorMessage ) )
            {
                // If an error occurred while trying to delete the previous subscription, but it was due to 
                // subscription not existing, that's ok, so don't stop.
                if ( !errorMessage.StartsWith( "No recurring subscriptions found" ) )
                {
                    errorMessage = $"An error occurred trying to delete the original schedule details: {errorMessage}";
                    return null;
                }
            }

            // If this is an update to existing transaction and payment info did not change, just add a new subscription (we did not use a 3step process)
            if ( paymentInfo is ReferencePaymentInfo )
            { 
            // Start to create the XML for an Add Subscription txn
            var rootElement = GetRoot( transaction.FinancialGateway, "add-subscription", paymentInfo );
            rootElement.Add(
                new XElement( "customer-vault-id", vaultId ),
                new XElement( "start-date", transaction.StartDate.ToString( "yyyyMMdd" ) ),
                new XElement( "order-description", paymentInfo.Description ),
                new XElement( "currency", "USD" ),
                new XElement( "tax-amount", "0.00" ) );

            // Add the plan details
            rootElement.Add( GetPlan( transaction, paymentInfo ) );

            // Post the Add Subscription transaction
            XDocument xdoc = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), rootElement );
            var result = PostToGateway( transaction.FinancialGateway, xdoc, out errorMessage );

            // If not valid, return null
            if ( result == null )
            {
                return false;
            }

            // Otherwise update the current scheduled transaction with the new information
            transaction.IsActive = true;
            transaction.GatewayScheduleId = result.GetValueOrNull( "subscription-id" );

            return true;











            try
            {
                // Make sure valid parameters were passed
                if ( financialGateway == null )
                {
                    throw new ArgumentNullException( "finacialGateway" );
                }
                if ( schedule == null )
                {
                    throw new ArgumentNullException( "schedule" );
                }
                if ( paymentInfo == null )
                {
                    throw new ArgumentNullException( "paymentInfo" );
                }

                // Check to see if third step is not needed or succesfully completed
                if ( !CompleteThirdStep( financialGateway, paymentInfo, resultQueryString, out errorMessage ) )
                {
                    return null;
                }

                // Start to create the XML for an Add Subscription txn
                var rootElement = GetRoot( financialGateway, "add-subscription", paymentInfo );
                rootElement.Add(
                    new XElement( "start-date", schedule.StartDate.ToString( "yyyyMMdd" ) ),
                    new XElement( "order-description", paymentInfo.Description ),
                    new XElement( "currency", "USD" ),
                    new XElement( "tax-amount", "0.00" ) );

                // Add the plan details
                rootElement.Add( GetPlan( schedule, paymentInfo ) );

                // Post the Add Subscription transaction
                XDocument xdoc = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), rootElement );
                var result = PostToGateway( financialGateway, xdoc, out errorMessage );

                // If not valid, return null
                if ( result == null )
                {
                    return null;
                }

                // Otherwise create a scheduled transaction record
                var scheduledTransaction = new FinancialScheduledTransaction();
                scheduledTransaction.IsActive = true;
                scheduledTransaction.GatewayScheduleId = result.GetValueOrNull( "subscription-id" );
                scheduledTransaction.FinancialGatewayId = financialGateway.Id;
                scheduledTransaction.ForeignKey = paymentInfo.AdditionalParameters.GetValueOrNull( "customer-vault-id" );
                scheduledTransaction.FinancialPaymentDetail = new FinancialPaymentDetail();
                string ccNumber = result.GetValueOrNull( "billing_cc-number" );
                if ( !string.IsNullOrWhiteSpace( ccNumber ) )
                {
                    // cc payment
                    var curType = CacheDefinedValue.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD );
                    scheduledTransaction.FinancialPaymentDetail.CurrencyTypeValueId = curType != null ? curType.Id : (int?)null;
                    scheduledTransaction.FinancialPaymentDetail.CreditCardTypeValueId = CreditCardPaymentInfo.GetCreditCardType( ccNumber.Replace( '*', '1' ).AsNumeric() )?.Id;
                    scheduledTransaction.FinancialPaymentDetail.AccountNumberMasked = ccNumber.Masked( true );

                    string mmyy = result.GetValueOrNull( "billing_cc-exp" );
                    if ( !string.IsNullOrWhiteSpace( mmyy ) && mmyy.Length == 4 )
                    {
                        scheduledTransaction.FinancialPaymentDetail.ExpirationMonthEncrypted = Encryption.EncryptString( mmyy.Substring( 0, 2 ) );
                        scheduledTransaction.FinancialPaymentDetail.ExpirationYearEncrypted = Encryption.EncryptString( mmyy.Substring( 2, 2 ) );
                    }
                }
                else
                {
                    // ach payment
                    var curType = CacheDefinedValue.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH );
                    scheduledTransaction.FinancialPaymentDetail.CurrencyTypeValueId = curType != null ? curType.Id : (int?)null;
                    scheduledTransaction.FinancialPaymentDetail.AccountNumberMasked = result.GetValueOrNull( "billing_account_number" ).Masked( true );
                }

                GetScheduledPaymentStatus( scheduledTransaction, out errorMessage );

                return scheduledTransaction;
            }

            catch ( WebException webException )
            {
                string message = GetResponseMessage( webException.Response.GetResponseStream() );
                errorMessage = webException.Message + " - " + message;
                return null;
            }

            catch ( Exception ex )
            {
                errorMessage = ex.Message;
                return null;
            }


        }

        /// <summary>
        /// Cancels the scheduled payment.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool CancelScheduledPayment( FinancialScheduledTransaction transaction, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( transaction != null &&
                !string.IsNullOrWhiteSpace( transaction.GatewayScheduleId ) &&
                transaction.FinancialGateway != null )
            {
                var rootElement = GetRoot( transaction.FinancialGateway, "delete-subscription", null );
                rootElement.Add( new XElement( "subscription-id", transaction.GatewayScheduleId ) );

                XDocument xdoc = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), rootElement );
                var result = PostToGateway( transaction.FinancialGateway, xdoc, out errorMessage );

                if ( result == null )
                {
                    return false;
                }

                transaction.IsActive = false;
                return true;
            }
            else
            {
                errorMessage = "Invalid original transaction, transaction code, or gateway.";
            }

            return false;
        }

        /// <summary>
        /// Gets the scheduled payment status supported.
        /// </summary>
        /// <returns></returns>
        public override bool GetScheduledPaymentStatusSupported
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the scheduled payment status.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool GetScheduledPaymentStatus( FinancialScheduledTransaction transaction, out string errorMessage )
        {
            // NMI does not support getting the status of a scheduled transaction.
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Gets the payments that have been processed for any scheduled transactions
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override List<Payment> GetPayments( FinancialGateway financialGateway, DateTime startDate, DateTime endDate, out string errorMessage )
        {
            errorMessage = string.Empty;

            var txns = new List<Payment>();

            var restClient = new RestClient( GetAttributeValue( financialGateway, "QueryUrl" ) );
            var restRequest = new RestRequest( Method.GET );

            restRequest.AddParameter( "username", GetAttributeValue( financialGateway, "AdminUsername" ) );
            restRequest.AddParameter( "password", GetAttributeValue( financialGateway, "AdminPassword" ) );
            restRequest.AddParameter( "start_date", startDate.ToString( "yyyyMMddHHmmss" ) );
            restRequest.AddParameter( "end_date", endDate.ToString( "yyyyMMddHHmmss" ) );

            try
            {
                var response = restClient.Execute( restRequest );
                if ( response != null )
                {
                    if ( response.StatusCode == HttpStatusCode.OK )
                    {
                        var xdocResult = GetXmlResponse( response );
                        if ( xdocResult != null )
                        {
                            var errorResponse = xdocResult.Root.Element( "error_response" );
                            if ( errorResponse != null )
                            {
                                errorMessage = errorResponse.Value;
                            }
                            else
                            {
                                foreach ( var xTxn in xdocResult.Root.Elements( "transaction" ) )
                                {
                                    Payment payment = new Payment();
                                    payment.TransactionCode = GetXElementValue( xTxn, "transaction_id" );
                                    payment.Status = GetXElementValue( xTxn, "condition" ).FixCase();
                                    payment.IsFailure =
                                        payment.Status == "Failed" ||
                                        payment.Status == "Abandoned" ||
                                        payment.Status == "Canceled";
                                    payment.TransactionCode = GetXElementValue( xTxn, "transaction_id" );
                                    payment.GatewayScheduleId = GetXElementValue( xTxn, "original_transaction_id" ).Trim();

                                    var statusMessage = new StringBuilder();
                                    DateTime? txnDateTime = null;

                                    foreach ( var xAction in xTxn.Elements( "action" ) )
                                    {
                                        DateTime? actionDate = ParseDateValue( GetXElementValue( xAction, "date" ) );
                                        string actionType = GetXElementValue( xAction, "action_type" );
                                        string responseText = GetXElementValue( xAction, "response_text" );

                                        if ( actionDate.HasValue )
                                        {
                                            statusMessage.AppendFormat( "{0} {1}: {2}; Status: {3}",
                                                actionDate.Value.ToShortDateString(), actionDate.Value.ToShortTimeString(),
                                                actionType.FixCase(), responseText );
                                            statusMessage.AppendLine();
                                        }

                                        decimal? txnAmount = GetXElementValue( xAction, "amount" ).AsDecimalOrNull();
                                        if ( txnAmount.HasValue && actionDate.HasValue )
                                        {
                                            payment.Amount = txnAmount.Value;
                                        }

                                        if ( actionType == "sale" )
                                        {
                                            txnDateTime = actionDate.Value;
                                        }

                                        if ( actionType == "settle")
                                        {
                                            payment.IsSettled = true;
                                            payment.SettledGroupId = GetXElementValue( xAction, "processor_batch_id" ).Trim();
                                            payment.SettledDate = actionDate;
                                            txnDateTime = txnDateTime.HasValue ? txnDateTime.Value : actionDate.Value;
                                        }
                                    }

                                    if ( txnDateTime.HasValue )
                                    {
                                        payment.TransactionDateTime = txnDateTime.Value;
                                        payment.StatusMessage = statusMessage.ToString();
                                        txns.Add( payment );
                                    }
                                }

                            }
                        }
                        else
                        {
                            errorMessage = "Invalid XML Document Returned From Gateway!";
                        }
                    }
                    else
                    {
                        errorMessage = string.Format( "Invalid Response from Gateway: [{0}] {1}", response.StatusCode.ConvertToString(), response.ErrorMessage );
                    }
                }
                else
                {
                    errorMessage = "Null Response From Gateway!";
                }
            }
            catch ( WebException webException )
            {
                string message = GetResponseMessage( webException.Response.GetResponseStream() );
                throw new Exception( webException.Message + " - " + message );
            }

            return txns;
        }

        /// <summary>
        /// Gets an optional reference identifier needed to process future transaction from saved account.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override string GetReferenceNumber( FinancialTransaction transaction, out string errorMessage )
        {
            errorMessage = string.Empty;
            return transaction.ForeignKey;
        }

        /// <summary>
        /// Gets an optional reference identifier needed to process future transaction from saved account.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override string GetReferenceNumber( FinancialScheduledTransaction scheduledTransaction, out string errorMessage )
        {
            errorMessage = string.Empty;
            return scheduledTransaction.ForeignKey;
        }

        /// <summary>
        /// Gets the root.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <returns></returns>
        private XElement GetRoot( FinancialGateway financialGateway, string elementName, PaymentInfo paymentInfo )
        {
            XElement rootElement = new XElement( elementName,
                new XElement( "api-key", GetAttributeValue( financialGateway, "SecurityKey" ) )
            );

            if ( paymentInfo != null && paymentInfo.AdditionalParameters != null )
            {
                foreach ( var keyValue in paymentInfo.AdditionalParameters )
                {
                    XElement xElement = new XElement( keyValue.Key, keyValue.Value );
                    rootElement.Add( xElement );
                }
            }

            return rootElement;
        }

        /// <summary>
        /// Completes the third step.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="resultQueryString">The result query string.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private bool CompleteThirdStep ( FinancialGateway financialGateway, PaymentInfo paymentInfo, string resultQueryString, out string errorMessage )
        {
            // If this was for a saved account, there's nothing to complete
            if ( paymentInfo is ReferencePaymentInfo )
            {
                errorMessage = string.Empty;
                return true;
            }

            // Create XML for completing third step of three-step Validate process
            var rootElement = GetRoot( financialGateway, "complete-action", paymentInfo );
            rootElement.Add( new XElement( "token-id", resultQueryString.Substring( 10 ) ) );

            // Post the Complete txn
            var xdoc = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), rootElement );
            var result = PostToGateway( financialGateway, xdoc, out errorMessage );

            // If validate was successful, save the customer's new vault id to the payment info record 
            if ( result != null )
            {
                string customerVaultId = result.GetValueOrNull( "customer-vault-id" );
                if ( customerVaultId.IsNotNullOrWhitespace() )
                {
                    if ( paymentInfo.AdditionalParameters == null )
                    {
                        paymentInfo.AdditionalParameters = new Dictionary<string, string>();
                    }
                    paymentInfo.AdditionalParameters.Add( "customer-vault-id", result.GetValueOrNull( "customer-vault-id" ) );
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates a billing XML element
        /// </summary>
        /// <param name="paymentInfo">The payment information.</param>
        /// <returns></returns>
        private XElement GetBilling( PaymentInfo paymentInfo )
        {
            XElement billingElement = new XElement( "billing",
                new XElement( "first-name", paymentInfo.FirstName ),
                new XElement( "last-name", paymentInfo.LastName ),
                new XElement( "address1", paymentInfo.Street1 ),
                new XElement( "address2", paymentInfo.Street2 ),
                new XElement( "city", paymentInfo.City ),
                new XElement( "state", paymentInfo.State ),
                new XElement( "postal", paymentInfo.PostalCode ),
                new XElement( "country", paymentInfo.Country ),
                new XElement( "phone", paymentInfo.Phone ),
                new XElement( "email", paymentInfo.Email )
            );

            if ( paymentInfo is ACHPaymentInfo )
            {
                var ach = paymentInfo as ACHPaymentInfo;
                billingElement.Add( new XElement( "account-type", ach.AccountType == BankAccountType.Savings ? "savings" : "checking" ) );
                billingElement.Add( new XElement( "entity-type", "personal" ) );
            }

            return billingElement;
        }

        /// <summary>
        /// Gets the plan.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <returns></returns>
        private XElement GetPlan( FinancialScheduledTransaction scheduledTransaction, PaymentInfo paymentInfo )
        {
            var schedule = new PaymentSchedule();
            schedule.TransactionFrequencyValue = CacheDefinedValue.Get( scheduledTransaction.TransactionFrequencyValueId );
            schedule.NumberOfPayments = scheduledTransaction.NumberOfPayments;
            schedule.StartDate = schedule.StartDate;

            return GetPlan( schedule, paymentInfo );
        }

        /// <summary>
        /// Creates a scheduled transaction plan XML element
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <returns></returns>
        private XElement GetPlan( PaymentSchedule schedule, PaymentInfo paymentInfo )
        {
            var selectedFrequencyGuid = schedule.TransactionFrequencyValue.Guid.ToString().ToUpper();

            if ( selectedFrequencyGuid == Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME )
            {
                // Make sure number of payments is set to 1 for one-time future payments
                schedule.NumberOfPayments = 1;
            }

            XElement planElement = new XElement( "plan",
                new XElement( "payments", schedule.NumberOfPayments.HasValue ? schedule.NumberOfPayments.Value.ToString() : "0" ),
                new XElement( "amount", paymentInfo.Amount.ToString() ) );

            switch ( selectedFrequencyGuid )
            {
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME:
                    planElement.Add( new XElement( "month-frequency", "12" ) );
                    planElement.Add( new XElement( "day-of-month", schedule.StartDate.Day.ToString() ) );
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_WEEKLY:
                    planElement.Add( new XElement( "day-frequency", "7" ) );
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_BIWEEKLY:
                    planElement.Add( new XElement( "day-frequency", "14" ) );
                    break;
                case Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY:
                    planElement.Add( new XElement( "month-frequency", "1" ) );
                    planElement.Add( new XElement( "day-of-month", schedule.StartDate.Day.ToString() ) );
                    break;
            }

            return planElement;
        }

        /// <summary>
        /// Posts to gateway.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        private Dictionary<string, string> PostToGateway( FinancialGateway financialGateway, XDocument data, out string errorMessage )
        {
            var restClient = new RestClient( GetAttributeValue( financialGateway, "APIUrl" ) );
            var restRequest = new RestRequest( Method.POST );
            restRequest.RequestFormat = DataFormat.Xml;
            restRequest.AddParameter( "text/xml", data.ToString(), ParameterType.RequestBody );

            try
            {
                var response = restClient.Execute( restRequest );
                var xdocResult = GetXmlResponse( response );
                if ( xdocResult != null )
                {
                    // Convert XML result to a dictionary
                    var result = new Dictionary<string, string>();
                    foreach ( XElement element in xdocResult.Root.Elements() )
                    {
                        if ( element.HasElements )
                        {
                            string prefix = element.Name.LocalName;
                            foreach ( XElement childElement in element.Elements() )
                            {
                                result.AddOrIgnore( prefix + "_" + childElement.Name.LocalName, childElement.Value.Trim() );
                            }
                        }
                        else
                        {
                            result.AddOrIgnore( element.Name.LocalName, element.Value.Trim() );
                        }
                    }

                    if ( result == null )
                    {
                        errorMessage = "Invalid Response from NMI!";
                        return null;
                    }

                    if ( result.GetValueOrNull( "result" ) != "1" )
                    {
                        errorMessage = result.GetValueOrNull( "result-text" );

                        string resultCodeMessage = GetResultCodeMessage( result );
                        if ( resultCodeMessage.IsNotNullOrWhitespace() )
                        {
                            errorMessage += string.Format( " ({0})", resultCodeMessage );
                        }

                        // write result error as an exception
                        ExceptionLogService.LogException( new Exception( $"Error processing NMI transaction. Result Code: {result.GetValueOrNull( "result-code" )} ({resultCodeMessage}). Result text: {result.GetValueOrNull( "result-text" )}. Card Holder Name: {result.GetValueOrNull( "first-name" )} {result.GetValueOrNull( "last-name" )}. Amount: {result.GetValueOrNull( "total-amount" )}. Transaction id: {result.GetValueOrNull( "transaction-id" )}. Descriptor: {result.GetValueOrNull( "descriptor" )}. Order description: {result.GetValueOrNull( "order-description" )}." ) );

                        return null;
                    }

                    errorMessage = string.Empty;
                    return result;
                }
                else
                {
                    errorMessage = "Invalid Response from NMI!";
                    return null;
                }
            }
            catch ( WebException webException )
            {
                string message = GetResponseMessage( webException.Response.GetResponseStream() );
                throw new Exception( webException.Message + " - " + message );
            }
            catch ( Exception ex )
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets the response as an XDocument
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        private XDocument GetXmlResponse( IRestResponse response )
        {
            if ( response.StatusCode == HttpStatusCode.OK &&
                response.Content.Trim().Length > 0 &&
                response.Content.Contains( "<?xml" ) )
            {
                return XDocument.Parse( response.Content );
            }
            
            return null;
        }

        /// <summary>
        /// Gets the result code message.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private string GetResultCodeMessage(Dictionary<string, string> result)
        {
            switch( result.GetValueOrNull( "result-code" ).AsInteger() )
            {
                case 100:
                    {
                        return "Transaction was approved.";
                    }
                case 200:
                    {
                        return "Transaction was declined by processor.";
                    }
                case 201:
                    {
                        return "Do not honor.";
                    }
                case 202:
                    {
                        return "Insufficient funds.";
                    }
                case 203:
                    {
                        return "Over limit.";
                    }
                case 204:
                    {
                        return "Transaction not allowed.";
                    }
                case 220:
                    {
                        return "Incorrect payment information.";
                    }
                case 221:
                    {
                        return "No such card issuer.";
                    }
                case 222:
                    {
                        return "No card number on file with issuer.";
                    }
                case 223:
                    {
                        return "Expired card.";
                    }
                case 224:
                    {
                        return "Invalid expiration date.";
                    }
                case 225:
                    {
                        return "Invalid card security code.";
                    }
                case 240:
                    {
                        return "Call issuer for further information.";
                    }
                case 250: // pickup card
                case 251: // lost card
                case 252: // stolen card
                case 253: // fradulent card
                    {
                        // these are more sensitive declines so sanitize them a bit but provide a code for later lookup
                        return string.Format("This card was declined (code: {0}).", result.GetValueOrNull( "result-code" ) );
                    }
                case 260:
                    {
                        return string.Format("Declined with further instructions available. ({0})", result.GetValueOrNull( "result-text" ) );
                    }
                case 261:
                    {
                        return "Declined-Stop all recurring payments.";
                    }
                case 262:
                    {
                        return "Declined-Stop this recurring program.";
                    }
                case 263:
                    {
                        return "Declined-Update cardholder data available.";
                    }
                case 264:
                    {
                        return "Declined-Retry in a few days.";
                    }
                case 300:
                    {
                        return "Transaction was rejected by gateway.";
                    }
                case 400:
                    {
                        return "Transaction error returned by processor.";
                    }
                case 410:
                    {
                        return "Invalid merchant configuration.";
                    }
                case 411:
                    {
                        return "Merchant account is inactive.";
                    }
                case 420:
                    {
                        return "Communication error.";
                    }
                case 421:
                    {
                        return "Communication error with issuer.";
                    }
                case 430:
                    {
                        return "Duplicate transaction at processor.";
                    }
                case 440:
                    {
                        return "Processor format error.";
                    }
                case 441:
                    {
                        return "Invalid transaction information.";
                    }
                case 460:
                    {
                        return "Processor feature not available.";
                    }
                case 461:
                    {
                        return "Unsupported card type.";
                    }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the response message.
        /// </summary>
        /// <param name="responseStream">The response stream.</param>
        /// <returns></returns>
        private string GetResponseMessage( Stream responseStream )
        {
            Stream receiveStream = responseStream;
            Encoding encode = System.Text.Encoding.GetEncoding( "utf-8" );
            StreamReader readStream = new StreamReader( receiveStream, encode );

            StringBuilder sb = new StringBuilder();
            Char[] read = new Char[8192];
            int count = 0;
            do
            {
                count = readStream.Read( read, 0, 8192 );
                String str = new String( read, 0, count );
                sb.Append( str );
            }
            while ( count > 0 );

            return sb.ToString();
        }

        private string GetXElementValue( XElement parentElement, string elementName )
        {
            var x = parentElement.Element( elementName );
            if ( x != null )
            {
                return x.Value;
            }
            return string.Empty;
        }

        private DateTime? ParseDateValue( string dateString )
        {
            if ( !string.IsNullOrWhiteSpace( dateString ) && dateString.Length >= 14 )
            {
                int year = dateString.Substring( 0, 4 ).AsInteger();
                int month = dateString.Substring( 4, 2 ).AsInteger();
                int day = dateString.Substring( 6, 2 ).AsInteger();
                int hour = dateString.Substring( 8, 2 ).AsInteger();
                int min = dateString.Substring( 10, 2 ).AsInteger();
                int sec = dateString.Substring( 12, 2 ).AsInteger();

                return new DateTime( year, month, day, hour, min, sec );
            }

            return DateTime.MinValue;

        }
        #endregion

    }
}
