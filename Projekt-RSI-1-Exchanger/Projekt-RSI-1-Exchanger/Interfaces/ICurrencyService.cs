using CoreWCF;

namespace Projekt_RSI_1_Exchanger.Interfaces
{
    [ServiceContract]
    public interface ICurrencyService
    {
        [OperationContract]
        double GetMultiplier(string targetCurrency);
    }
}
