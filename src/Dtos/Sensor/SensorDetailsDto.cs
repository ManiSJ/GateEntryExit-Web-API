using GateEntryExit.Dtos.Gate;

namespace GateEntryExit.Dtos.Sensor
{
    public class SensorDetailsDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public GateDetailsDto GateDetails { get; set; }
    }
}
