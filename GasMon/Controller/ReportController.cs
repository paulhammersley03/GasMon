using ClosedXML.Excel;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GasMon.Controller
{
    public class ReportController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static void ProgramEnd(List<SensorLocationModel> locations, List<SensorDataModel> dataList, List<SensorDataModel> dupeList, List<SensorDataModel> unauthList, List<SensorDataModel> sensorDataList)
        {
            Console.WriteLine("Total Sensors = " + locations.Count);
            Console.WriteLine("Total Readings = " + dataList.Count);
            Console.WriteLine("Total Average Value = " + sensorDataList.Average(x => x.readingValueNumber));            
            logger.Info("Unauthorized Readings: " + unauthList.Count);
            logger.Info("Duplicated Readings: " + dupeList.Count);

            var averageList = new List<(string id, double average)>();

            XLWorkbook sensorReadings = new XLWorkbook();
            var counter = 0;
            var fileName = "Sensor Readings Report";

            double bestAverageSoFar = -1;
            string bestAverageIdSoFar = null;

            foreach (var id in dataList.GroupBy(x => x.sensorId))
            {
                counter += 1;
                var ws = sensorReadings.Worksheets.Add("Sensor" + counter);                             
                ws.Cell(1, 1).Value = "Event ID";
                ws.Cell(1, 2).Value = "Sensor ID";
                ws.Cell(1, 3).Value = "Reading Value";
                ws.Cell(1, 4).Value = "Timestamp";
                ws.Cell(1, 5).Value = "Date Time";

                double average = id.Average(l => l.readingValueNumber);
                averageList.Add( (id.Key, average) );

                if (average > bestAverageSoFar)
                {
                    bestAverageSoFar = average;
                    bestAverageIdSoFar = id.Key;
                }

                int row = 2;
                foreach (var reading in id)
                {
                    ws.Cell(row, 1).Value = reading.readingId;
                    ws.Cell(row, 2).Value = reading.sensorId;
                    ws.Cell(row, 3).Value = reading.readingValue;
                    ws.Cell(row, 4).Value = reading.timeStamp;
                    ws.Cell(row, 5).Value = reading.dateTime;
                    row++;
                }
            }

            Console.WriteLine($"Highest average Sensor Reading is {bestAverageSoFar}, SensorID = {bestAverageIdSoFar}");
            var location = locations.FirstOrDefault(l => l.sensorId == bestAverageIdSoFar);
            if (location != null)
            {
                Console.WriteLine($"Found location, x = {location.xCoord} y = {location.yCoord}");
            }
            else
            {
                Console.WriteLine("Did not find location");
            }

            if (fileName.Contains("."))
            {
                int IndexOfLastFullStop = fileName.LastIndexOf('.');
                fileName = fileName.Substring(0, IndexOfLastFullStop) + ".xlsx";
            }
            fileName = fileName + ".xlsx";
            sensorReadings.SaveAs("C:/Work/Training/GasMon/GasMon/" + fileName);
        }
    }
}
