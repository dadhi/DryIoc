namespace CurrencyRounding
{
    public class CurrencyRoundingFactory
        : ICurrencyRoundingFactory
    {
        public CurrencyRoundingFactory(
        )
        {
        }
    }


    public class CurrencyRoundingType
    {
    }


    public class DefaultCurrencyRounding
        : ICurrencyRounding
    {
        public DefaultCurrencyRounding(
        )
        {
        }
    }


    public interface ICurrencyRounding
    {
    }


    public interface ICurrencyRoundingFactory
    {
    }


    public class RoundedTotalGrossCurrencyRounding : DefaultCurrencyRounding
    {
        public RoundedTotalGrossCurrencyRounding(
        )
        {
        }
    }


    public class SwedishCurrencyRounding
        : ICurrencyRounding
    {
        public SwedishCurrencyRounding(
        )
        {
        }
    }
}