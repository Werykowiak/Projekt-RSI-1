using Projekt_RSI_1_Exchanger.Interfaces;

namespace Projekt_RSI_1_Exchanger.Services
{
    public class CurrencyService : ICurrencyService
    {
        public double GetMultiplier(string targetCurrency)
        {
            if (string.IsNullOrEmpty(targetCurrency))
                return 1.0;

            return targetCurrency.ToUpper() switch
            {
                "EUR" => 0.23, 
                "USD" => 0.25,
                "GBP" => 0.20,
                "PLN" => 1.0,
                _ => 1.0
            };
        }
    }
}
