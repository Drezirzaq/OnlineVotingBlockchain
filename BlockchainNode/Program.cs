

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
            var builder = WebApplication.CreateBuilder(args);

            int port = args.Length > 0 ? int.Parse(args[0]) : 5000;
            string nodeAddress = $"http://{GetLocalIPAddress()}:{port}";

            var loggerFactory = builder.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<PeerManager>();


            var blockchain = new Blockchain();
            builder.Services.AddSingleton(blockchain);
            var node = new Node(blockchain, nodeAddress, logger);
            builder.Services.AddSingleton(node);
            var wallet = new Wallet(blockchain);
            builder.Services.AddSingleton(wallet);
            var validatorFactory = new ValidatorFactory(wallet, blockchain);
            builder.Services.AddSingleton(validatorFactory);
            var pollManager = new PollManager(wallet, blockchain);
            builder.Services.AddSingleton(pollManager);

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

            app.Run(nodeAddress);
        }


    }
}
