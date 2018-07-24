using System;
using System.Activities;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;

namespace Avanade.D365.Samples.Plugin.Tests
{
    [TestClass]
    public class SamplePluginTests
    {
        #region Class Level Members

        private IOrganizationService _mockedOrgService;

        #endregion

        #region Constants

        #endregion

        [TestMethod]
        public void SamplePluginTestMethod()
        {
            //ARRANGE
            var serviceMock = new Mock<IOrganizationService>();
            var factoryMock = new Mock<IOrganizationServiceFactory>();
            var tracingServiceMock = new Mock<ITracingService>();
            var notificationServiceMock = new Mock<IServiceEndpointNotificationService>();
            var pluginContextMock = new Mock<IPluginExecutionContext>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            //create a guid that we want our mock CRM organization service Create method to return when called
            Guid idToReturn = Guid.NewGuid();

            //next - create an entity object that will allow us to capture the entity record that is passed to the Create method
            Entity actualEntity = new Entity();

            //setup the CRM service mock
            serviceMock.Setup(t => t.Create(It.IsAny<Entity>()))

            //when Create is called with any entity as an invocation parameter
            .Returns(idToReturn) //return the idToReturn guid
            .Callback<Entity>(s => actualEntity = s); //store the Create method invocation parameter for inspection later
            IOrganizationService service = serviceMock.Object;

            //set up a mock servicefactory using the CRM service mock
            factoryMock.Setup(t => t.CreateOrganizationService(It.IsAny<Guid>())).Returns(service);
            var factory = factoryMock.Object;

            //set up a mock tracingservice - will write output to console
            tracingServiceMock.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object[]>())).Callback<string, object[]>((t1, t2) => Console.WriteLine(t1, t2));
            var tracingService = tracingServiceMock.Object;

            //set up mock notificationservice - not going to do anything with this
            var notificationService = notificationServiceMock.Object;

            //set up mock plugincontext with input/output parameters, etc.
            Entity targetEntity = new Entity("account");
            targetEntity.LogicalName = "account";

            //userid to be used inside the plug-in
            Guid userId = Guid.NewGuid();

            //"generated" account id
            Guid accountId = Guid.NewGuid();

            //get parameter collections ready
            ParameterCollection inputParameters = new ParameterCollection();
            inputParameters.Add("Target", targetEntity);
            ParameterCollection outputParameters = new ParameterCollection();
            outputParameters.Add("id", accountId);

            //finish preparing the plugincontextmock
            pluginContextMock.Setup(t => t.InputParameters).Returns(inputParameters);
            pluginContextMock.Setup(t => t.OutputParameters).Returns(outputParameters);
            pluginContextMock.Setup(t => t.UserId).Returns(userId);
            pluginContextMock.Setup(t => t.PrimaryEntityName).Returns("account");
            var pluginContext = pluginContextMock.Object;

            //set up a serviceprovidermock
            serviceProviderMock.Setup(t => t.GetService(It.Is<Type>(i => i == typeof(IServiceEndpointNotificationService)))).Returns(notificationService);
            serviceProviderMock.Setup(t => t.GetService(It.Is<Type>(i => i == typeof(ITracingService)))).Returns(tracingService);
            serviceProviderMock.Setup(t => t.GetService(It.Is<Type>(i => i == typeof(IOrganizationServiceFactory)))).Returns(factory);
            serviceProviderMock.Setup(t => t.GetService(It.Is<Type>(i => i == typeof(IPluginExecutionContext)))).Returns(pluginContext);
            var serviceProvider = serviceProviderMock.Object;

            //ACT
            //instantiate the plugin object and execute it with the testserviceprovider
            SamplePlugin samplePlugin = new SamplePlugin();
            samplePlugin.Execute(serviceProvider);


            #region Clean Up
            {

            }
            #endregion
        }
    }
}
