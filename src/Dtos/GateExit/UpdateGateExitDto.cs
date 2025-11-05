namespace GateEntryExit.Dtos.GateExit
{
    public class UpdateGateExitDto
    {
        public Guid Id { get; set; }

        public DateTime TimeStamp { get; set; }

        public int NumberOfPeople { get; set; }
    }
}
