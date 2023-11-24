using DryIoc;
using DryIoc.Microsoft.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var container = new Container(DryIocAdapter.MicrosoftDependencyInjectionRules);
var dependencyInjectionFactory = new DryIocServiceProviderFactory(container);
builder.Host.UseServiceProviderFactory(dependencyInjectionFactory);
builder.Services.AddControllersWithViews();
builder.Services.AddWebOptimizer(options =>
{
    options.AddLessBundle("/css/test.css", "wwwroot/css/test.less").UseContentRoot();
});

var app = builder.Build();
app.UseDeveloperExceptionPage();
app.UseWebOptimizer();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();