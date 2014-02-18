﻿// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Linq;
using System.ComponentModel;
using System.ServiceModel.Syndication;
using System.Runtime.Caching;
using System.Web.UI;

using DotLiquid;

using Rock;
using Rock.Attribute;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web;

namespace RockWeb.Blocks.Cms
{

    [DisplayName("RSS Feed")]
    [Category("CMS")]
    [Description("Gets and consumes and RSS Feed. The feed is rendered based on a provided liquid template. ")]
    [TextField("RSS Feed Url", "The Url of the RSS Feed to retrieve and consume", true, "", "Feed")]
    [IntegerField("Results per page", "How many results/articles to display on the page at a time. Default is 10.", false, 10, "Feed")]
    [IntegerField("Cache Duration", "The length of time (in minutes) that the RSS Feed data is stored in cache. If this value is 0, the feed will not be cached. Default is 20 minutes", false, 20, "Feed")]
    [TextField("CSS File", "An optional CSS file to add to the page for styling. Example \"Styles/rss.css\" would point to the stylesheet in the current theme's styles folder.", false, "", "Layout")]
    [CodeEditorField("Template", "The liquid template to use for rendering. This template should be in the theme's \"Assets/Liquid\" folder and should have an underscore prepended to the filename.", 
        CodeEditorMode.Liquid, CodeEditorTheme.Rock, 200, true, @"{% include 'RSSFeed' %}", "Layout")]
    [BooleanField("Enable Debug", "Flag indicating that the control should output the feed data that will be passed to Liquid for parsing.", false)]
    [LinkedPage("Detail Page")]
    public partial class RSSFeed : RockBlock
    {
        #region Private Properties

        private string TemplateCacheKey
        {
            get
            {
                return string.Format( "Rock:Template:{0}", BlockId );
            }
        }
        #endregion

        #region Control Methods
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            BlockUpdated += RSSFeed_BlockUpdated;
            AddConfigurationUpdateTrigger( upContent );    
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            SetNotificationBox( String.Empty, String.Empty );

            LoadFeed();
        }
        #endregion

        #region Page Events 
        protected void RSSFeed_BlockUpdated( object sender, EventArgs e )
        {
            ClearCache();
            pnlContent.Visible = false;
            LoadFeed();
        }
        #endregion

        #region Internal Methods
        private void ClearCache()
        {
            ObjectCache cache = MemoryCache.Default;
            SyndicationFeedHelper.ClearCachedFeed( GetAttributeValue( "RSSFeedUrl" ) );
            cache.Remove( TemplateCacheKey );
        }

        private Template GetTemplate()
        {
            string liquidFolder = System.Web.HttpContext.Current.Server.MapPath( ResolveRockUrl( "~~/Assets/Liquid" ) );
            Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
            Template.FileSystem = new DotLiquid.FileSystems.LocalFileSystem( liquidFolder );

            ObjectCache cache = MemoryCache.Default;
            Template template = null;

            if ( cache[TemplateCacheKey] != null )
            {
                template = (Template)cache[TemplateCacheKey];
            }
            else
            {
                template = Template.Parse( GetAttributeValue( "Template" ) );
                cache.Set( TemplateCacheKey, template, new CacheItemPolicy() );
            }

            return template;
        }

        private string LoadDebugData( Dictionary<string, object> feedDictionary )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if ( feedDictionary != null && feedDictionary.Count > 0 )
            {
                sb.AppendLine( "<ul id=\"debugFeed\">" );
                foreach ( var kvp in feedDictionary )
                {
                    sb.Append( FeedDebugNode( kvp ) );
                }
                sb.AppendLine( "</ul>" );
            }
            return sb.ToString();
        }

        private string FeedDebugNode( KeyValuePair<string, object> node )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if ( node.Value.GetType() == typeof( Dictionary<string, object> ) )
            {
                sb.AppendFormat( "<li><span>{0}</span>", node.Key );

                foreach ( var child in (Dictionary<string, object>)node.Value )
                {
                    sb.AppendLine( "<ul>" );
                    sb.Append( FeedDebugNode( child ) );
                    sb.AppendLine( "</ul>" );
                }
            }
            else if ( node.Value.GetType() == typeof( List<Dictionary<string, object>> ) )
            {
                List<Dictionary<string, object>> nodeList = (List<Dictionary<string, object>>)node.Value;

                foreach ( var listItem in nodeList )
                {
                    sb.AppendFormat( "<li><span>{0}</span>", node.Key );
                    sb.AppendLine( "<ul>" );
                    foreach ( var child in listItem )
                    {
                        sb.Append( FeedDebugNode( child ) );
                    }
                    sb.AppendLine( "</ul>" );
                }
            }
            else
            {
                sb.AppendFormat( "<li><span>{0}: {1}</span>", node.Key, node.Value == null ? String.Empty : System.Web.HttpUtility.HtmlEncode( node.Value.ToString() ) );
            }

            sb.Append( "</li>" );

            return sb.ToString();
        }

        private void LoadFeed()
        {

            string feedUrl = GetAttributeValue( "RSSFeedUrl" );

            Dictionary<string, string> messages = new Dictionary<string, string>();
            bool isError = false;

            try
            {

                Dictionary<string, object> feedDictionary = SyndicationFeedHelper.GetFeed( feedUrl, GetAttributeValue( "DetailPage" ), (int)GetAttributeValue( "CacheDuration" ).AsInteger(true), ref messages, ref isError );

                if ( feedDictionary != null )
                {
                    string content = String.Empty;
                    if ( GetAttributeValue( "EnableDebug" ).AsBoolean() )
                    {
                        content = LoadDebugData( feedDictionary );
                    }
                    else
                    {
                        content = GetTemplate().Render( Hash.FromDictionary( feedDictionary ) );
                    }


                    if ( content.Contains( "No such template" ) )
                    {
                        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match( GetAttributeValue( "Template" ), @"'([^']*)" );
                        if ( match.Success )
                        {
                            messages.Add( "Warning", string.Format( "Could not find the template _{0}.liquid in {1}.", match.Groups[1].Value, ResolveRockUrl( "~~/Assets/Liquid" ) ) );
                            isError = true;
                        }
                        else
                        {
                            messages.Add( "Warning", "Unable to parse the template name from settings." );
                            isError = true;

                        }
                    }
                    else
                    {
                        phRSSFeed.Controls.Clear();
                        phRSSFeed.Controls.Add( new LiteralControl( content ) );
                        
                    }

                    pnlContent.Visible = true;
                }


            }
            catch ( Exception ex )
            {
                if ( IsUserAuthorized( "Administrate" ) )
                {
                    throw ex;
                }
                else
                {
                    messages.Add( "exception", "An exception has occurred." );
                }
            }

            if ( messages.Count > 0 )
            {
                if ( IsUserAuthorized( "Administrate" ) )
                {
                    SetNotificationBox( messages.FirstOrDefault().Key, messages.FirstOrDefault().Value, isError ? NotificationBoxType.Warning : NotificationBoxType.Info );
                }
                else
                {
                    SetNotificationBox( "Content not available", "Oops. The requested content is not currently available. Please try again later." );
                }
            }

        }

        private void SetNotificationBox( string heading, string bodyText )
        {
            SetNotificationBox( heading, bodyText, NotificationBoxType.Info );
        }

        private void SetNotificationBox( string heading, string bodyText, NotificationBoxType boxType )
        {
            nbRSSFeed.Heading = heading;
            nbRSSFeed.Text = bodyText;
            nbRSSFeed.NotificationBoxType = boxType;

            nbRSSFeed.Visible = !( String.IsNullOrWhiteSpace( heading ) || String.IsNullOrWhiteSpace( bodyText ) );
        }

        #endregion
    }

  
}