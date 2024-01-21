
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SennheiserBackend.Database;
using SennheiserBackend.Database.Repositories;
using SennheiserBackend.Services;
using System.Reflection;

namespace SennheiserBackend
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSingleton<IReceiverService, ReceiverService>();
            builder.Services.AddSingleton<IReceiverRepository, ReceiverRepository>();
            builder.Services.AddSingleton<IDemoDbContextFactory, DemoDbContextFactory>();
            builder.Services.AddSingleton<IReceiverConnectionService, ReceiverConnectionService>();
            builder.Services.AddSingleton<IReceiverClientFactory, ReceiverClientFactory>();
            builder.Services.AddSingleton<IWebSocketFactory, WebSocketFactory>();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddHttpLogging(o => { });

            builder.Services.AddControllers().AddNewtonsoftJson();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

            builder.Logging.AddConsole();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpLogging();

            app.UseExceptionHandler("/error");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
