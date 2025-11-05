using GateEntryExit.Dtos.Gate;

namespace GateEntryExit.Dtos.Sensor
{
    public class GetAllSensorsDto
    {
        public int TotalCount { get; set; }

        public List<SensorDetailsDto> Items { get; set; }
    }
}
