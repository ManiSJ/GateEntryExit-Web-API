namespace GateEntryExit.Domain
{
    public class GateExit
    {
        public Guid Id { get; set; }

        public DateTime TimeStamp { get; set; }

        public int NumberOfPeople { get; set; }

        public Guid GateId { get; set; }

        public Gate Gate { get; set; }

        public GateExit()
        {

        }

        public GateExit(Guid id,
           Guid gateId,
           int numberOfPeople,
           DateTime timeStamp)
        {
            Id = id;
            GateId = gateId;
            NumberOfPeople = numberOfPeople;
            TimeStamp = timeStamp;
        }
    }
}
