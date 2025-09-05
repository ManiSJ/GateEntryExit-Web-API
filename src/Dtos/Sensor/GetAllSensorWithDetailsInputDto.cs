using GateEntryExit.Dtos.Shared;

namespace GateEntryExit.Dtos.Sensor
{
    public class GetAllSensorWithDetailsInputDto : GetAllDto
    {
        public Guid[] GateIds { get; set; }

        public DateTime? FromDate { get; set; } 

        public DateTime? ToDate { get; set; }
    }
}
