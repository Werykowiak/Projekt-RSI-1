using CoreWCF;
using CoreWCF.Channels;
using CoreWCF.Dispatcher;

namespace Projekt_RSI_1_BackEnd.Handlers
{
    public class ApiKeyInspector : IDispatchMessageInspector
    {
        private const string ApiKeyHeaderName = "X-Api-Key";
        private const string ApiKeyHeaderNamespace = "http://projektrsi.security";
        private readonly string _expectedApiKey; // Możesz to pobrać z appsettings.json

        public ApiKeyInspector(string apiKey)
        {
            _expectedApiKey = apiKey;
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            // Szukamy nagłówka w kopercie SOAP
            int headerIndex = request.Headers.FindHeader(ApiKeyHeaderName, ApiKeyHeaderNamespace);
            string action = request.Headers.Action;

            if (!action.Contains("AddTrainRoute") || !action.Contains("DeleteTrainRoute") || !action.Contains("EditTrainRoute"))
            {
                if (headerIndex < 0)
                {
                    throw new FaultException("Brak klucza API (X-Api-Key) w nagłówku SOAP.");
                }

                string providedKey = request.Headers.GetHeader<string>(headerIndex);

                if (providedKey != _expectedApiKey)
                {
                    throw new FaultException("Nieprawidłowy klucz API.");
                }
            }

            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState) { }
    }
}
