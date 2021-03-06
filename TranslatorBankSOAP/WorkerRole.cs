using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BankSOAP;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using TranslatorBankSOAP.Models;

namespace TranslatorBankSOAP
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("TranslatorBankSOAP is running");

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

            Trace.TraceInformation("TranslatorBankSOAP has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("TranslatorBankSOAP is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("TranslatorBankSOAP has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "datdb.cphbusiness.dk",
                UserName = "student",
                Password = "cph"
            };

            var bank = new BankService();

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "Databasserne_Test", type: "direct");
                var queName = channel.QueueDeclare().QueueName;

                channel.QueueBind(queName, "Databasserne_Test", "BankSOAP");
                
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var input = JsonConvert.DeserializeObject<Input>(Encoding.UTF8.GetString(ea.Body));

                    Trace.TraceInformation($"Got request: {input}");
                    Trace.TraceInformation("Proccesing request...");

                    var rate = bank.GetIntrestRate(input.CreditScore, input.Months, input.Amount);
                    Trace.TraceInformation($"Got rate: {rate}");

                    var output = new Output(input, rate);
                    Trace.TraceInformation($"Created output: {output}");
                    
                    var props = new BasicProperties();
                    props.CorrelationId = "BankSOAP";
                    channel.BasicPublish("", "Databasserne_Normalizer" , props, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(output)));

                    Trace.TraceInformation("Request sent");
                    Trace.TraceInformation("------------------------------");

                };
                channel.BasicConsume(queue: queName,
                    noAck: true,
                    consumer: consumer);


                Trace.TraceInformation("Waiting for requests...");

                while (!cancellationToken.IsCancellationRequested)
                {
                }
            }
        }
    }
}
