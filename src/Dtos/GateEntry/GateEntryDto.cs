namespace GateEntryExit.Dtos.GateEntry
{
    public class GateEntryDto
    {
        public Guid Id { get; set; }

        public Guid GateId { get; set; }

        public DateTime TimeStamp { get; set; }

        public int NumberOfPeople { get; set; }

        public string GateName { get; set; }
    }
}
