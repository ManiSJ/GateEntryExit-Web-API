namespace GateEntryExit.Dtos.GateExit
{
    public class GateExitDto
    {
        public Guid Id { get; set; }

        public Guid GateId { get; set; }

        public DateTime TimeStamp { get; set; }

        public int NumberOfPeople { get; set; }

        public string GateName { get; set; }
    }
}
