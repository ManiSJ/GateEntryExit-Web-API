namespace GateEntryExit.Dtos.Sensor
{
    public class GetAllSensorWithDetailsReportInputDto
    {
        public Guid[] GateIds { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}
