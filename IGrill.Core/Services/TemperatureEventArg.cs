namespace IGrill.Core
{
    public class TemperatureChangedEventArg
    {
        /// <summary>
        /// The index of the probe. For iGrill2 = 0 to 4
        /// </summary>
        public int ProbeIndex { get; set; }

        /// <summary>
        /// The temperature of the probe
        /// </summary>
        public int? Temperature { get; set; }

        public TemperatureChangedEventArg(int probeIndex, int? temperature)
        {
            this.ProbeIndex = probeIndex;
            this.Temperature = temperature;
        }
    }
}