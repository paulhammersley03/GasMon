using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NLog;
using System;

namespace GasMon
{
    public class GetSensorLocation
    {
        private const string bucketName = "eventprocessing-ucas2-locationss3bucket-1dfub0iyuq3av";
        private const string keyName = "locations-part2.json";
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.EUWest1;
        private static IAmazonS3 client;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public GetSensorLocation(AWSCredentials credentials)
        {
            client = new AmazonS3Client(credentials, bucketRegion);
        }

        public async Task<List<SensorLocationModel>> ReadObjectDataAsync()
        {
            string responseBody = "";
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName
                };
                using (GetObjectResponse response = await client.GetObjectAsync(request))
                using (Stream responseStream = response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    responseBody = reader.ReadToEnd();
                    return ParseLocations(responseBody);
                }
            }
        }

        internal static List<SensorLocationModel> ParseLocations(string responseBody)
        {
            List<SensorLocationModel> sensorList = new List<SensorLocationModel>();
            JArray locations = JArray.Parse(responseBody);

            foreach (var sensor in locations.Cast<JObject>())
            {
                var sensorLocation = new SensorLocationModel(
                    (string)sensor["x"],
                    (string)sensor["y"],
                    (string)sensor["id"]
                    );
                sensorList.Add(sensorLocation);
            }
            logger.Info("Json data parsed into c# objects successfully!");
            return sensorList;
        }
    }
}




