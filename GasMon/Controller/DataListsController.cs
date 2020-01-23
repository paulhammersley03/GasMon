using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GasMon.Controller
{
    public class DataListsController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static async Task<List<SensorDataModel>> CreateDataList(List<SensorLocationModel> locations, GetSensorData sensorData, DateTime endTime, List<SensorDataModel> dataList, List<SensorDataModel> dupeList, List<SensorDataModel> unauthList, List<SensorDataModel> sensorDataList)
        {
            while (endTime > DateTime.Now)
            {
                sensorDataList = await sensorData.GetSensorDataAsync();
                try
                {
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
                            //checks for readings from sensors not in location list
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
                catch
                {
                    logger.Info("Corrupted Reading");
                    Console.WriteLine("Corrupted Reading");
                    continue;
                }
            }
            return sensorDataList;
        }
    }
}
