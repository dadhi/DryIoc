using Web.IocDi;

namespace Web.Models.Home
{
    [ResolveAsSelf]
    public sealed class IndexModel
    {
        public string Title { get { return "Static title"; } }
        public int Singleton1Id { get; set; }
        public int Singleton2Id { get; set; }
        public int Transient1Id { get; set; }
        public int Transient2Id { get; set; }
        public int PerRequest1Id { get; set; }
        public int PerRequest2Id { get; set; }
        public int FooScopeInstance1Id { get; set; }
        public int FooScopeInstance2Id { get; set; }
        public int BarScopeInstance1Id { get; set; }
        public int BarScopeInstance2Id { get; set; }
    }
}
