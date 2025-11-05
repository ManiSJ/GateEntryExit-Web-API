using GateEntryExit.Dtos.Gate;

namespace GateEntryExit.Dtos.GateEntry
{
    public class GetAllGateEntriesDto
    {
        public int TotalCount { get; set; }

        public List<GateEntryDto> Items { get; set; }
    }
}
