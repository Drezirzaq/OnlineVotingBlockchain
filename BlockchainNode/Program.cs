

using System.Net.Sockets;
using System.Net;


namespace MainBlockchain
{
    public class Program
    {
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        public static void Main(string[] args)
        {
            var ip = GetLocalIPAddress();
            int port = args.Length > 0 ? int.Parse(args[0]) : 5000;

            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(IPAddress.Parse(ip), port, listenOptions =>
                {
                    listenOptions.UseHttps("Certs/192.168.1.87.pfx", "changeit");
                });
            });

            builder.WebHost.UseUrls($"https://{ip}:{port}");

            var loggerFactory = builder.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<PeerManager>();

            var blockchain = new Blockchain();
            builder.Services.AddSingleton(blockchain);
            var node = new Node(blockchain, $"https://{ip}:{port}", logger);
            builder.Services.AddSingleton(node);
            var wallet = new Wallet(blockchain);
            builder.Services.AddSingleton(wallet);
            var pollManager = new PollManager(wallet, blockchain);
            builder.Services.AddSingleton(pollManager);
            var validatorFactory = new ValidatorFactory(pollManager, wallet, blockchain);
            builder.Services.AddSingleton(validatorFactory);
            ValidationHandler.validatorFactory = validatorFactory;

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            builder.Services.AddControllers();

            var app = builder.Build();

            var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStopping.Register(() =>
            {
                Console.WriteLine("Disposing...");
                pollManager.Dispose();
                node.DisposeAsync().AsTask().Wait();
            });

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("AllowAll");
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.Run();
        }
    }
}
