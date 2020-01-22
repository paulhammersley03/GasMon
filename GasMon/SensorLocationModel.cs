using System;
using System.Collections.Generic;
using System.Text;

namespace GasMon
{
    public class SensorLocationModel
    {
        public string xCoord { get; set; }
        public string yCoord { get; set; }
        public string sensorId { get; set; }

        public SensorLocationModel(string xCoord, string yCoord, string sensorId)
        {
            this.xCoord = xCoord;
            this.yCoord = yCoord;
            this.sensorId = sensorId;
        }
    }
}
