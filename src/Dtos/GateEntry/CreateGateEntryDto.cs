namespace GateEntryExit.Dtos.GateEntry
{
    public class CreateGateEntryDto
    {
        public Guid GateId { get; set; }

        public DateTime TimeStamp { get; set; }

        public int NumberOfPeople { get; set; }
    }
}
