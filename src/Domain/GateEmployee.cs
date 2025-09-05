namespace GateEntryExit.Domain
{
    public class GateEmployee
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public GateEmployee(Guid id, string name) 
        {
            Id = id;
            Name = name;
        }
    }
}
