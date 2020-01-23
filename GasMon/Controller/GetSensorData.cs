using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Runtime;

namespace GasMon
{
    public class GetSensorData
    {
        private AmazonSimpleNotificationServiceClient sns;
        private AmazonSQSClient sqs;
        private string myQueueUrl;

        public GetSensorData(AWSCredentials credentials)
        {
            sns = new AmazonSimpleNotificationServiceClient(credentials, RegionEndpoint.EUWest1);
            sqs = new AmazonSQSClient(credentials, RegionEndpoint.EUWest1);
        }

        public async System.Threading.Tasks.Task CreateQueue()
        {
            var myTopicArn = "arn:aws:sns:eu-west-1:552908040772:EventProcessing-UCAS2-snsTopicSensorDataPart2-6SW9NJKW1LZ9";
            var myQueueName = "Paul-GasMon-" + Guid.NewGuid();
            myQueueUrl = (await sqs.CreateQueueAsync(myQueueName)).QueueUrl;
            await sns.SubscribeQueueAsync(myTopicArn, sqs, myQueueUrl);
        }

        public async System.Threading.Tasks.Task<List<SensorDataModel>> GetSensorDataAsync()
        {            
            List<SensorDataModel> sensorReadingsList = new List<SensorDataModel>();
            List<Message> messages = (await sqs.ReceiveMessageAsync(new ReceiveMessageRequest(myQueueUrl) { WaitTimeSeconds = 3, MaxNumberOfMessages = 10 })).Messages;            
            foreach (var message in messages)
            {                
                var snsMessage = Amazon.SimpleNotificationService.Util.Message.ParseMessage(message.Body);
                JObject reading = JObject.Parse(snsMessage.MessageText);                              
                var sensorReading = new SensorDataModel(
                            (string)reading["value"],
                            (string)reading["eventId"],
                            (string)reading["locationId"],
                            (string)reading["timestamp"]
                            );
                sensorReadingsList.Add(sensorReading); 
            }

            if (messages.Count > 0)
            {
                //Deleting messages we've processed from Queue
                await sqs.DeleteMessageBatchAsync(myQueueUrl, messages.Select(x => new DeleteMessageBatchRequestEntry(x.MessageId, x.ReceiptHandle)).ToList());
            }

            return sensorReadingsList;
        }   
    }  
}
