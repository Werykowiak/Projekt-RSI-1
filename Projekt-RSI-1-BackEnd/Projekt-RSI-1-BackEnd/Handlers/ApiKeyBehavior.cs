using CoreWCF;
using CoreWCF.Channels;
using CoreWCF.Description;
using CoreWCF.Dispatcher;

namespace Projekt_RSI_1_BackEnd.Handlers
{
    public class ApiKeyBehavior : IServiceBehavior
    {
        private readonly string _apiKey;

        public ApiKeyBehavior(string apiKey)
        {
            _apiKey = apiKey;
        }
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) { }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (var cdb in serviceHostBase.ChannelDispatchers)
            {
                var cd = cdb as ChannelDispatcher;
                if (cd != null)
                {
                    foreach (var ed in cd.Endpoints)
                    {
                        // Dodajemy nasz inspektor do każdego punktu końcowego
                        ed.DispatchRuntime.MessageInspectors.Add(new ApiKeyInspector(_apiKey));
                    }
                }
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) { }
    }
}
