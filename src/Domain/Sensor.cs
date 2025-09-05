using System.ComponentModel.DataAnnotations;

namespace GateEntryExit.Domain
{
    public class Sensor
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid GateId { get; set; }

        public Gate Gate { get; set; }

        public Sensor()
        {

        }

        public Sensor(Guid id,
            Guid gateId,
            string name)
        {
            Id = id;
            GateId = gateId;
            Name = name;
        }
    }
}
