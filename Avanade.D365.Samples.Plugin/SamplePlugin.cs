using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;

// Microsoft Dynamics CRM namespace(s)
using Microsoft.Xrm.Sdk;

namespace Avanade.D365.Samples.Plugin
{
    public class SamplePlugin : IPlugin
    {
        /// <summary>
        /// A plug-in that does something when some event occurs.
        /// </summary>
        /// <remarks>
        /// Register this plug-in on some message, some entity,
        /// </remarks>
        public void Execute(IServiceProvider serviceProvider)
        {
            #region Constants
            string noteSubject = "Account Created";
            #endregion

            //Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // The InputParameters collection contains all the data passed in the message request. For Plugins Registered to Delete Messages, Type will be 'EntityReference' NOT 'Entity'
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.
                Entity entity = (Entity)context.InputParameters["Target"];

                // Verify that the target entity represents your target entity type.
                // If not, this plug-in was not registered correctly.
                if (entity.LogicalName != "account") //i.e. "account"
                {
                    tracingService.Trace("SamplePlugin: Invalid Context Entity: {0}", entity.LogicalName);
                    return;
                }
                try
                {

                    // Obtain the organization service reference.
                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    //This OrganizationService uses the security context of the user who triggered the plugin (saved / deleted a record etc)
                    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                    //This Organization Service Impersonates the System Administrator
                    IOrganizationService impersonatedService = serviceFactory.CreateOrganizationService(null);

                    // Do Something in Microsoft Dynamics CRM.
                    tracingService.Trace("SamplePlugin: Entering Creating Note");

                    //Create a Note for this Account using the Name and User Id
                    if (entity.Attributes.Contains("name"))
                    {
                        Entity noteEntity = new Entity("annotation");
                        noteEntity.Attributes["subject"] = noteSubject;
                        noteEntity.Attributes["notetext"] = string.Format("Account Name: {0} Created by User with Id: {1}", entity.Attributes["name"], context.UserId);
                        noteEntity.Attributes["objectid"] = entity.ToEntityReference();

                        service.Create(noteEntity);

                        tracingService.Trace("SamplePlugin: Note Created");
                    }
                    tracingService.Trace("SamplePlugin: Exiting Create a Note");
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in the SamplePlugin plug-in.", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("SamplePlugin: {0}", ex.ToString());
                    throw;
                }
            }
        }
    }
}
