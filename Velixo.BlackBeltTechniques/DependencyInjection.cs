using Autofac;
using System;
using PX.Data;
using PX.Objects;
using PX.Objects.PO;
using System.Net;


namespace Velixo.BlackBeltTechniques
{
    public class CurrencyConversionServiceRegistration : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DummyCurrencyConversionService>().As<ICurrencyConversionService>();
            //builder.RegisterType<YahooCurrencyConversionService>().As<ICurrencyConversionService>();
        }
    }

    internal interface ICurrencyConversionService
    {
        decimal ConvertAmount(decimal amount, string fromCurrency, string toCurrency);
    }

    internal class DummyCurrencyConversionService : ICurrencyConversionService
    {
        public decimal ConvertAmount(decimal amount, string fromCurrency, string toCurrency)
        {
            return amount * 2;
        }
    }

    internal class YahooCurrencyConversionService : ICurrencyConversionService
    {
        public decimal ConvertAmount(decimal amount, string fromCurrency, string toCurrency)
        {
            string url = $"http://finance.yahoo.com/d/quotes.csv?s={fromCurrency}{toCurrency}=X&f=l1";

            using (var wc = new WebClient())
            {
                var response = wc.DownloadString(url);
                decimal exchangeRate = decimal.Parse(response, System.Globalization.CultureInfo.InvariantCulture);

                return amount * exchangeRate;
            }
        }
    }

    public class POOrderEntryExt : PXGraphExtension<POOrderEntry>
    {
        [InjectDependency]
        private ICurrencyConversionService CurrencyConversion { get; set; }

        public PXAction<POOrder> ConvertTotal;
        [PXButton]
        [PXUIField(DisplayName = "Convert total")]
        protected void convertTotal()
        {
            var amount = CurrencyConversion.ConvertAmount(Base.Document.Current.CuryOrderTotal.GetValueOrDefault(), "USD", "CAD");
            throw new PXException($"Converted amount: {amount}");
        }
    }
}
