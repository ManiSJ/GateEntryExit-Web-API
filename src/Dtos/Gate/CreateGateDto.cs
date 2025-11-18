using GateEntryExit.Attributes;

namespace GateEntryExit.Dtos.Gate
{
    [GateUnique]
    public class CreateGateDto
    {
        public string Name { get; set; }

        public List<string> Names { get; set; }
    }
}
