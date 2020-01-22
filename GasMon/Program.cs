using Amazon.Runtime;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GasMon
{
    class Program
    {
        static AWSCredentials GetDefaultCredentials()
        {
            var chain = new Amazon.Runtime.CredentialManagement.CredentialProfileStoreChain();
            if (chain.TryGetAWSCredentials("default", out var credentials)) return credentials;
            throw new InvalidOperationException("Missing AWS profile default from ~/.aws/credentials");
        }
        //sets up the logging
        private static Logger logger = LogManager.GetCurrentClassLogger();
        static async Task Main(string[] args)
        {
            //setting up queue
            var credentials = GetDefaultCredentials();
            var sensorLocation = new GetSensorLocation(credentials);
            var locations = await sensorLocation.ReadObjectDataAsync();

            //gets data from queue
            var sensorData = new GetSensorData(credentials);
            await sensorData.CreateQueue();
            //sets up timer
            DateTime endTime = DateTime.Now.AddMinutes(.5);
            //actual readings list
            List<SensorDataModel> dataList = new List<SensorDataModel>();
            //duplicated list for error checking
            List<SensorDataModel> dupeList = new List<SensorDataModel>();
            //unauthorized sensor reading for checking
            List<SensorDataModel> unauthList = new List<SensorDataModel>();
            //only one item from queue will be on this list
            List<SensorDataModel> sensorDataList = new List<SensorDataModel>();

            //does the actual shizzle
            while (endTime > DateTime.Now)
            {
                sensorDataList = await sensorData.GetSensorDataAsync();

                foreach (var reading in sensorDataList)
                {
                    if (dataList.Any(p => p.readingId == reading.readingId))
                    {
                        //checks for dupes, adds to dupe list if true
                        dupeList.Add(reading);
                        logger.Info("Event Already Exists!");
                        Console.WriteLine("Event Already Exists! - Duplicated Readings: " + dupeList.Count);
                    }
                    else if (!locations.Any(q => q.sensorId == reading.sensorId))
                    {
                        //checks for dupes, adds to dupe list if true
                        unauthList.Add(reading);
                        logger.Info("Unauthorized Sensor Reading!");
                        Console.WriteLine("Unauthorized Sensor Reading! - Unauthorized Readings: " + unauthList.Count);
                    }
                    else
                    {
                        //adds to individual list if not
                        dataList.Add(reading);
                        Console.WriteLine("Event ID: " + reading.readingId + "| Sensor ID: " + "\t" + reading.sensorId + "| Value: " + "\t" + reading.readingValueNumber + "| Time: " + "\t" + reading.dateTime);
                    }
                }
            }
            Console.WriteLine("Total Sensors = " + locations.Count);
            Console.WriteLine("Total Readings = " + dataList.Count);
            Console.WriteLine("Average Value for First Minute = " + sensorDataList.Average(x => x.readingValueNumber));
            logger.Info("Unauthorized Readings: " + unauthList.Count);
            logger.Info("Duplicated Readings: " + dupeList.Count);

            var counter = 0;

            foreach (var id in dataList.GroupBy(x => x.sensorId))
            {
                counter += 1;

                File.WriteAllLines("C:/Work/Training/GasMon/GasMon/readings" + counter + ".csv", id.Select(x =>
                $"{x.readingId},{x.sensorId},{x.readingValue},{x.timeStamp},{x.dateTime}"));
            }            
        }
    }
}
