using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GasMon.Controller;

namespace GasMon
{
    public class GasMon
    {
        static AWSCredentials GetDefaultCredentials()
        {
            var chain = new Amazon.Runtime.CredentialManagement.CredentialProfileStoreChain();
            if (chain.TryGetAWSCredentials("default", out var credentials)) return credentials;
            throw new InvalidOperationException("Missing AWS profile default from ~/.aws/credentials");
        }
                
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
            DateTime endTime = DateTime.Now.AddMinutes(2);
            //actual readings list
            List<SensorDataModel> dataList = new List<SensorDataModel>();
            //duplicated list for error checking
            List<SensorDataModel> dupeList = new List<SensorDataModel>();
            //unauthorized sensor reading for checking
            List<SensorDataModel> unauthList = new List<SensorDataModel>();
            //only one item from queue will be on this list
            List<SensorDataModel> sensorDataList = new List<SensorDataModel>();
            //does the actual shizzle
            sensorDataList = await DataListsController.CreateDataList(locations, sensorData, endTime, dataList, dupeList, unauthList, sensorDataList);
            ReportController.ProgramEnd(locations, dataList, dupeList, unauthList, sensorDataList);
        }            
    }
}
