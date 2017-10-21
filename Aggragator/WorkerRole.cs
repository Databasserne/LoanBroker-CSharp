using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aggragator.Models;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Aggragator
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private const string InChannel = "Databasserne_Aggregator";
        private const string OutChannel = "Databasserne_DimmerOut";

        private readonly int _timeout = 10; // in seconds
        private readonly BlockingCollection<Tuple<DateTime, List<Input>>> _timeoutController = new BlockingCollection<Tuple<DateTime, List<Input>>>();

        public override void Run()
        {
            Trace.TraceInformation("Aggragator is running");

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

            Trace.TraceInformation("Aggragator has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("Aggragator is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("Aggragator has stopped");
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
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var input = JsonConvert.DeserializeObject<Input>(Encoding.UTF8.GetString(body));
                    Trace.TraceInformation($"Input: {input}");

                    var foundSSN = false;

                    foreach (var timecon in _timeoutController)
                    {
                        if (timecon.Item2.First().SSN.Equals(input.SSN))
                        {
                            timecon.Item2.Add(input);
                            foundSSN = true;
                            break;
                        }
                    }

                    if (!foundSSN)
                    {
                        _timeoutController.Add(Tuple.Create(DateTime.Now, new List<Input>{input}), cancellationToken);
                    }

                };
                channel.BasicConsume(queue: InChannel,
                    noAck: true,
                    consumer: consumer);

                while (!cancellationToken.IsCancellationRequested)
                {
                    if(_timeoutController.Count == 0) continue;

                    foreach (var timeoutCon in _timeoutController)
                    {
                        var compTime = timeoutCon.Item1.Add(TimeSpan.FromSeconds(_timeout));
                        var nowTime = DateTime.Now;
                        if (compTime < nowTime)
                        {
                            Trace.TraceInformation($"Time to comp: {compTime} - Now Time: {nowTime}");
                            Input min = null;
                            foreach (var input in timeoutCon.Item2)
                            {
                                if (min == null)
                                {
                                    min = input;
                                    continue;
                                }
                                if (min.InterestRate > input.InterestRate)
                                {
                                    min = input;
                                }
                            }

                            Trace.TraceInformation($"Found lowest interest rate: {min}");
                            _timeoutController.Take();

                            var bodyOut = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(min));

                            channel.QueueDeclare(OutChannel, false, false, false, null);
                            channel.BasicPublish("", OutChannel, null, bodyOut);
                        }
                    }
                }
            }
        }
    }
}
