using System.Xml.Schema;

namespace GateEntryExit.Dtos.Sensor
{
    public class GetAllSensorWithDetailsOutputDto
    {
        public int TotalCount { get; set; }

        public List<SensorDetailsDto> Items { get; set; }
    }
}
