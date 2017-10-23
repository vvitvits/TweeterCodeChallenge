using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Net;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace Server
{
    class CorsConstants
    {
        internal const string Origin = "Origin";
        internal const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
        internal const string AccessControlRequestMethod = "Access-Control-Request-Method";
        internal const string AccessControlRequestHeaders = "Access-Control-Request-Headers";
        internal const string AccessControlAllowMethods = "Access-Control-Allow-Methods";
        internal const string AccessControlAllowHeaders = "Access-Control-Allow-Headers";

        internal const string PreflightSuffix = "_preflight_";
        static internal List<string> AllowedOrigins = new List<string> {
        "*"    };
    }

    public class CorsEnablingBehavior : BehaviorExtensionElement, IEndpointBehavior
    {

        public override Type BehaviorType
        {
            get { return typeof(CorsEnablingBehavior); }
        }

        protected override object CreateBehavior()
        {
            return new CorsEnablingBehavior();
        }

        private class CorsHeaderInjectingMessageInspector : IDispatchMessageInspector
        {

            public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
            {
                return (request.Properties["httpRequest"] as HttpRequestMessageProperty).Headers;
            }

            private static Dictionary<string, string> _headersToInject = new Dictionary<string, string> {
            {
                CorsConstants.AccessControlRequestMethod,
                "POST,GET,PUT,DELETE,OPTIONS"
            },
            {
                CorsConstants.AccessControlAllowHeaders,
                "X-Requested-With, Content-Type"
            }

        };
            public void BeforeSendReply(ref Message reply, object correlationState)
            {
                if (HttpContext.Current.Request.HttpMethod != "OPTIONS")
                {
                    dynamic httpHeader = reply.Properties["httpResponse"] as HttpResponseMessageProperty;
                    AllowOrigin("*", httpHeader);
                    //WebHeaderCollection requestHeaders = (WebHeaderCollection)correlationState;
                    //if (requestHeaders.AllKeys.Contains(CorsConstants.Origin, StringComparer.OrdinalIgnoreCase))
                    //{
                    //    dynamic origin = requestHeaders[CorsConstants.Origin];
                    //    AllowOrigin(origin, httpHeader);
                    //}
                }
            }

            private void AllowOrigin(string origin, HttpResponseMessageProperty httpHeader)
            {
                httpHeader.Headers.Add(CorsConstants.AccessControlAllowOrigin, origin);

                foreach (var aItem in _headersToInject)
                {
                    httpHeader.Headers.Add(aItem.Key, aItem.Value);
                }
            }
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new CorsHeaderInjectingMessageInspector());
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }
    }


    class CorsEnabledServiceHostFactory : ServiceHostFactory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            return new CorsEnabledServiceHost(serviceType, baseAddresses);
        }
    }

    class CorsEnabledUniversalFilterServiceHostFactory : ServiceHostFactory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            return new CorsEnabledServiceHost(serviceType, baseAddresses);
        }
    }

    class CorsEnabledServiceHost : ServiceHost
    {

        private List<Type> contractTypes;
        public CorsEnabledServiceHost(Type serviceType, Uri[] baseAddresses) : base(serviceType, baseAddresses)
        {
            this.contractTypes = GetContractType(serviceType);
        }

        protected override void OnOpening()
        {
            foreach (var aContract in this.ImplementedContracts)
            {
                AddPreflightOperations(this.Description.Endpoints[0], aContract.Value.Operations.ToList());
            }
            base.OnOpening();
        }

        private List<Type> GetContractType(Type serviceType)
        {
            if (HasServiceContract(serviceType))
            {
                return new List<Type> { serviceType };
            }

            dynamic possibleContractTypes = serviceType.GetInterfaces().Where(i => HasServiceContract(i)).ToList();

            switch (possibleContractTypes.Count)
            {
                case 0:
                    throw new InvalidOperationException("Service type " + serviceType.FullName + " does not implement any interface decorated with the ServiceContractAttribute.");
                default:
                    return possibleContractTypes;
            }
        }

        private static bool HasServiceContract(Type type)
        {
            return Attribute.IsDefined(type, typeof(ServiceContractAttribute), false);
        }

        private void AddPreflightOperations(ServiceEndpoint endpoint, List<OperationDescription> corsOperations)
        {
            Dictionary<string, PreflightOperationBehavior> uriTemplates = new Dictionary<string, PreflightOperationBehavior>(StringComparer.OrdinalIgnoreCase);

            foreach (var operation in corsOperations)
            {
                if (operation.Behaviors.Find<WebGetAttribute>() != null)
                {
                    // no need to add preflight operation for GET requests
                    continue;
                }

                if (operation.IsOneWay)
                {
                    // no support for 1-way messages
                    continue;
                }

                string originalUriTemplate = null;
                WebInvokeAttribute originalWia = operation.Behaviors.Find<WebInvokeAttribute>();

                //If originalWia IsNot Nothing AndAlso originalWia.UriTemplate IsNot Nothing Then
                //    originalUriTemplate = NormalizeTemplate(originalWia.UriTemplate)
                //Else
                originalUriTemplate = operation.Name;
                //End If

                string originalMethod = originalWia != null && originalWia.Method != null ? originalWia.Method : "POST";

                if (uriTemplates.ContainsKey(originalUriTemplate))
                {
                    // there is already an OPTIONS operation for this URI, we can reuse it
                    PreflightOperationBehavior operationBehavior = uriTemplates[originalUriTemplate];
                    operationBehavior.AddAllowedMethod(originalMethod);
                }
                else
                {
                    ContractDescription contract = operation.DeclaringContract;
                    OperationDescription preflightOperation = new OperationDescription(operation.Name + CorsConstants.PreflightSuffix, contract);
                    MessageDescription inputMessage = new MessageDescription(operation.Messages[0].Action + CorsConstants.PreflightSuffix, MessageDirection.Input);
                    inputMessage.Body.Parts.Add(new MessagePartDescription("input", contract.Namespace)
                    {
                        Index = 0,
                        Type = typeof(Message)
                    });
                    preflightOperation.Messages.Add(inputMessage);
                    MessageDescription outputMessage = new MessageDescription(operation.Messages[1].Action + CorsConstants.PreflightSuffix, MessageDirection.Output);
                    outputMessage.Body.ReturnValue = new MessagePartDescription(preflightOperation.Name + "Return", contract.Namespace) { Type = typeof(Message) };
                    preflightOperation.Messages.Add(outputMessage);

                    WebInvokeAttribute wia = new WebInvokeAttribute();
                    wia.UriTemplate = originalUriTemplate;
                    wia.Method = "OPTIONS";

                    preflightOperation.Behaviors.Add(wia);
                    preflightOperation.Behaviors.Add(new DataContractSerializerOperationBehavior(preflightOperation));
                    PreflightOperationBehavior preflightOperationBehavior = new PreflightOperationBehavior(preflightOperation);
                    preflightOperationBehavior.AddAllowedMethod(originalMethod);
                    preflightOperation.Behaviors.Add(preflightOperationBehavior);
                    uriTemplates.Add(originalUriTemplate, preflightOperationBehavior);

                    contract.Operations.Add(preflightOperation);
                }
            }
        }
    }

    class PreflightOperationBehavior : IOperationBehavior
    {
        private OperationDescription preflightOperation;

        private List<string> allowedMethods = new List<string>();
        public PreflightOperationBehavior(OperationDescription preflightOperation)
        {
            this.preflightOperation = preflightOperation;
            this.allowedMethods = new List<string>();
        }

        public void AddAllowedMethod(string httpMethod)
        {
            this.allowedMethods.Add(httpMethod);
        }

        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            dispatchOperation.Invoker = new PreflightOperationInvoker(operationDescription.Messages[1].Action, this.allowedMethods);
        }

        public void Validate(OperationDescription operationDescription)
        {
        }
    }

    class PreflightOperationInvoker : IOperationInvoker
    {

        private string replyAction;

        private List<string> allowedHttpMethods;
        public PreflightOperationInvoker(string replyAction, List<string> allowedHttpMethods)
        {
            this.replyAction = replyAction;
            this.allowedHttpMethods = allowedHttpMethods;
        }

        public object[] AllocateInputs()
        {
            return new object[1];
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            Message input = (Message)inputs[0];
            outputs = null;
            return HandlePreflight(input);
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            throw new NotSupportedException("Only synchronous invocation");
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            throw new NotSupportedException("Only synchronous invocation");
        }

        public bool IsSynchronous
        {
            get { return true; }
        }

        private Message HandlePreflight(Message input)
        {
            HttpRequestMessageProperty httpRequest = (HttpRequestMessageProperty)input.Properties[HttpRequestMessageProperty.Name];
            string origin = httpRequest.Headers[CorsConstants.Origin];


            Message reply = Message.CreateMessage(MessageVersion.None, replyAction);
            HttpResponseMessageProperty httpResponse = new HttpResponseMessageProperty();
            reply.Properties.Add(HttpResponseMessageProperty.Name, httpResponse);

            if (!string.IsNullOrEmpty(origin))
            {
                AllowOriginOption(origin, httpRequest, httpResponse);
            }
            return reply;
        }

        private void AllowOriginOption(string origin, HttpRequestMessageProperty httpRequest, HttpResponseMessageProperty httpResponse)
        {
            string requestMethod = httpRequest.Headers[CorsConstants.AccessControlRequestMethod];
            string requestHeaders = httpRequest.Headers[CorsConstants.AccessControlRequestHeaders];
            httpResponse.SuppressEntityBody = true;
            httpResponse.StatusCode = HttpStatusCode.OK;
            if (origin != null)
            {
                httpResponse.Headers.Add(CorsConstants.AccessControlAllowOrigin, origin);
            }

            if (requestMethod != null && this.allowedHttpMethods.Contains(requestMethod))
            {
                httpResponse.Headers.Add(CorsConstants.AccessControlAllowMethods, string.Join(",", this.allowedHttpMethods));
            }

            if (requestHeaders != null)
            {
                httpResponse.Headers.Add(CorsConstants.AccessControlAllowHeaders, requestHeaders);
            }
        }
    }
}