using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BankRouter.Models;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BankRouter
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private const string InChannel = "Databasserne_BankRouterIn";
        private const string OutChannel = "Databasserne_BankRouterOut";


        public override void Run()
        {
            Trace.TraceInformation("BankRouter is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("BankRouter has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("BankRouter is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("BankRouter has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "datdb.cphbusiness.dk",
                UserName = "student",
                Password = "cph"
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: InChannel,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var input = JsonConvert.DeserializeObject<Input>(Encoding.UTF8.GetString(body));

                    Trace.TraceInformation($"Input: {input}");

                    var bodyOut = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Output(input)));

                    foreach (var bank in input.Banks)
                    {
                        channel.BasicPublish("", $"{OutChannel}_{bank}", null, bodyOut);
                    }
                };

                channel.BasicConsume(queue: InChannel,
                    noAck: true,
                    consumer: consumer);

                while (!cancellationToken.IsCancellationRequested) { }
            }
        }
    }
}
