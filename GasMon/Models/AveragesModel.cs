namespace GasMon
{
    public class AverageModel
    {
        string sensorId { get; set; }
        double sensorReading { get; set; }

        public AverageModel(string sensorId, double sensorReading)
        {
            this.sensorId = sensorId;
            this.sensorReading = sensorReading;
        }
    }
}
