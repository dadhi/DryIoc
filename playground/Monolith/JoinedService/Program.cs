var builder = WebApplication.CreateBuilder(args);

// public static IHostBuilder CreateHostBuilder(string[] args) =>
//             Host.CreateDefaultBuilder(args)
//                 .ConfigureServices((context, services) =>
//                 {
//                     var configuration = context.Configuration;
//                     services.ConfigureDaemonBackgroundServices(
//                         configuration,
//                         DaemonNames,
//                         args);

//                     services.AddHostedService<DaemonBackgroundService<First.Program>>();
//                     services.AddHostedService<DaemonBackgroundService<Second.Program>>();
//                 })
//                 .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
