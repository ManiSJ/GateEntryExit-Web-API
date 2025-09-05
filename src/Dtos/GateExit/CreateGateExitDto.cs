namespace GateEntryExit.Dtos.GateExit
{
    public class CreateGateExitDto
    {

        public Guid GateId { get; set; }

        public DateTime TimeStamp { get; set; }

        public int NumberOfPeople { get; set; }
    }
}
