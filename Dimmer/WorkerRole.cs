using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dimmer.Models;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Dimmer
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private readonly string _ingoingChannelIn = "Databasserne_DimmerIn";
        private readonly string _ingoingChannelOut = "Databasserne_CreditScoreIn";
        private readonly string _outgoingChannel = "Databasserne_DimmerOut";

        private static Dictionary<string, List<string>> _stuff;

        public override void Run()
        {
            Trace.TraceInformation("Dimmer is running");

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

            Trace.TraceInformation("Dimmer has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("Dimmer is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("Dimmer has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "datdb.cphbusiness.dk",
                UserName = "student",
                Password = "cph"
            };

            _stuff = new Dictionary<string, List<string>>();

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: _ingoingChannelIn,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                channel.QueueDeclare(queue: _outgoingChannel,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var ingoingConsumer = new EventingBasicConsumer(channel);
                var outgoingConsumer = new EventingBasicConsumer(channel);

                ingoingConsumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var input = JsonConvert.DeserializeObject<Ingoing>(Encoding.UTF8.GetString(body));

                    if (_stuff.ContainsKey(input.SSN))
                    {
                        _stuff[input.SSN].Add(ea.BasicProperties.ReplyTo);
                    }
                    else
                    {
                        _stuff.Add(input.SSN, new List<string>{ea.BasicProperties.ReplyTo});
                        channel.BasicPublish("", _ingoingChannelOut, null, body);
                    }
                };


                outgoingConsumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var input = JsonConvert.DeserializeObject<OutgoingInput>(Encoding.UTF8.GetString(body));

                    if (_stuff.ContainsKey(input.SSN))
                    {
                        var replyQueues = _stuff[input.SSN];
                        
                        var output = new OutgoingOutput(input);

                        foreach (var replyQueue in replyQueues)
                        {
                            channel.BasicPublish("", replyQueue, null, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(output)));
                        }

                        _stuff.Remove(input.SSN);
                    }
                    else
                    {
                        Trace.TraceWarning("Got request without knowing the SSN");
                    }
                };


                channel.BasicConsume(queue: _ingoingChannelIn,
                    noAck: true,
                    consumer: ingoingConsumer);

                channel.BasicConsume(queue: _outgoingChannel,
                    noAck: true,
                    consumer: outgoingConsumer);

                Trace.TraceInformation("Waiting for requests...");

                while (!cancellationToken.IsCancellationRequested)
                {
                }
            }
        }
    }
}
