namespace DryIoc.WebApi.Owin.Sample.Services
{
    class NewsProvider : IGetNews
    {
        public string[] News()
        {
            return new[] { "Foo!", "and now a Bar!" };
        }
    }
}