namespace IGrillLibrary
{
    public class TemperatureChangedEventArg
    {
        public int ProbeId { get; set; }
        public int Temperature { get; set; }

        public TemperatureChangedEventArg(int probeId, int temperature)
        {
            this.ProbeId = probeId;
            this.Temperature = temperature;
        }
    }
}