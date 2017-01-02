using SimpleMessages.Extensions;
using SimpleMessages.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Web;

namespace SimpleMessages
{
    class Program
    {
        static void Main(/*string[] args*/)
        {
            //var publicMessageServiceUrl = ConfigurationManager.AppSettings["my:publicMessageServiceUrl"];
            //var publicSvcUri = new Uri(publicMessageServiceUrl);

            //var secureMessageServiceUrl = ConfigurationManager.AppSettings["my:secureMessageServiceUrl"];
            //var secureSvcUri = new Uri(secureMessageServiceUrl);

            var messageServiceUrl = ConfigurationManager.AppSettings["my:messageServiceUrl"];
            var svcUri = new Uri(messageServiceUrl);

            var logMessagesValue = ConfigurationManager.AppSettings["my:logMessages"];
            var logMessages = bool.Parse(logMessagesValue);

            var enableCorsValue = ConfigurationManager.AppSettings["my:enableCors"];
            var enableCors = bool.Parse(enableCorsValue);

            using (var host = new WebServiceHost(typeof(MessageService), svcUri))
            {
                /*
                // Authentication: set custom validator for username/pwd
                host.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom;
                host.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new CustomUserNameValidator();

                // Authorization: set custom policy
                var policies = new List<IAuthorizationPolicy>();
                policies.Add(new MessageServiceAuthorizationPolicy());
                host.Authorization.ExternalAuthorizationPolicies = policies.AsReadOnly();

                // endpoint #1 - no security
                var publicBinding = new WebHttpBinding();
                publicBinding.Security.Mode = WebHttpSecurityMode.None;
                var epNoSec = host.AddServiceEndpoint(typeof(IPublicMessages), publicBinding, publicSvcUri);

                // endpoint #2 - basic authentication security
                // TODO: we need SSL/TLS for this to be secure
                var secureBinding = new WebHttpBinding();
                secureBinding.Security.Mode = WebHttpSecurityMode.Transport;
                secureBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
                //binding.Security.Transport.Realm = "should I?";
                var epSec = host.AddServiceEndpoint(typeof(ISecureMessages), secureBinding, secureSvcUri);
                */

                // common endpoint - no security
                var publicBinding = new WebHttpBinding();
                publicBinding.Security.Mode = WebHttpSecurityMode.None;
                //var epSec = host.AddServiceEndpoint(typeof(IPublicMessages), publicBinding, svcUri);
                //var epSec = host.AddServiceEndpoint(typeof(ISecureMessages), publicBinding, svcUri);
                var ep = host.AddServiceEndpoint(typeof(IMessages), publicBinding, svcUri);

                // setup message interception
                if (logMessages)
                {
                    var msgLogger = new DispatchLogger();
                    //epNoSec.EndpointBehaviors.Add(msgIcpt);
                    //epSec.EndpointBehaviors.Add(msgIcpt);
                    ep.EndpointBehaviors.Add(msgLogger);
                }

                if (enableCors)
                {
                    var corsEnabler = new CorsEnabler();
                    ep.EndpointBehaviors.Add(corsEnabler);
                }

                try
                {
                    host.Open();
                    Console.WriteLine("*** Host is open ***");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: Open() failed on host!");
                    Console.WriteLine(ex.Message);
                }

                Console.WriteLine("[ENTER] to close");
                Console.WriteLine();
                Console.ReadLine();
                host.Close();
            }

            Console.WriteLine("Bye!");
        }
    }
}
