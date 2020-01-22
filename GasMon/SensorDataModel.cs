using System;
using System.Collections.Generic;
using System.Text;

namespace GasMon
{
    public class SensorDataModel
    {
        public string readingValue { get; set; }
        public double readingValueNumber { get; set; }
        public string readingId { get; set; }
        public string sensorId { get; set; }
        public string timeStamp { get; set; }
        public DateTime dateTime { get; set; }

        public SensorDataModel(string readingValue, string readingId, string sensorId, string timeStamp)
        {
            this.readingValue = readingValue;
            this.readingValueNumber = double.Parse(readingValue);
            this.readingId = readingId;
            this.sensorId = sensorId;
            this.timeStamp = timeStamp;
            this.dateTime = DateTimeConverter.UnixTimeStampToDateTime(Double.Parse(timeStamp));
        }
    }

}
