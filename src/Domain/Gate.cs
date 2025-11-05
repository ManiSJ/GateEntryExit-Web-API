using System.ComponentModel.DataAnnotations;

namespace GateEntryExit.Domain
{
    public class Gate
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public List<GateEntry> GateEntries { get ; set;}

        public  List<GateExit>  GateExits { get; set; }

        public Sensor Sensor { get; set; }

        public Gate()
        {
            GateEntries = new List<GateEntry>();
            GateExits = new List<GateExit>();
        }

        public Gate(Guid id,
            string name) : this()
        {
            Id = id;
            Name = name;
        }
    }
}
