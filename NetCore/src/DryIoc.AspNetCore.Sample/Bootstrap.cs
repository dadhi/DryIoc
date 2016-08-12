using DryIoc.AspNetCore.Sample.Components;

namespace DryIoc.AspNetCore.Sample
{
    public class Bootstrap
    {
        public Bootstrap(IRegistrator r) // If you need the whole container then change parameter type rom IRegistrator to IContainer
        { 
            r.Register<IUserContext, UserContext>(Reuse.InWebRequest);
        }
    }
}