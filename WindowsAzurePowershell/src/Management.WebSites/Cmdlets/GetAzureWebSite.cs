﻿// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Management.WebSites.Cmdlets
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.ServiceModel;
    using Common;
    using Services;
    using IServiceManagement = Services.IWebsitesServiceManagement;

    /// <summary>
    /// Gets an azure website.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureWebSite")]
    public class GetAzureWebSiteCommand : WebsitesCmdletBase
    {
        /// <summary>
        /// Initializes a new instance of the GetAzureWebSiteCommand class.
        /// </summary>
        public GetAzureWebSiteCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the GetAzureWebSiteCommand class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        public GetAzureWebSiteCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        protected virtual void WriteWebsite(Website website)
        {
            WriteObject(website, true);
        }

        internal IList<Website> GetWebsiteProcess()
        {
            IList<Website> allWebsites = new List<Website>();
            InvokeInOperationContext(() =>
            {
                try
                {
                    var webspaces = RetryCall(s => Channel.GetWebspaces(s));
                    WaitForOperation(CommandRuntime.ToString());

                    foreach(var webspace in webspaces)
                    {
                        var currentWebsites = RetryCall(s => Channel.GetWebsites(s, webspace.Name,
                            new[] { "repositoryuri", "publishingpassword", "publishingusername" }));
                        
                        WaitForOperation(CommandRuntime.ToString());

                        foreach (var website in currentWebsites)
                        {
                            WriteWebsite(website);
                            allWebsites.Add(website);
                        }
                    }
                }
                catch (CommunicationException ex)
                {
                    WriteErrorDetails(ex);
                }
            });

            return allWebsites;
        }

        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();
                GetWebsiteProcess();
            }
            catch (Exception ex)
            {
                SafeWriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}
