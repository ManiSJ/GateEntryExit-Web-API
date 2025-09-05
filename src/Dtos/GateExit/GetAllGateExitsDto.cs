using GateEntryExit.Dtos.Gate;

namespace GateEntryExit.Dtos.GateExit
{
    public class GetAllGateExitsDto
    {
        public int TotalCount { get; set; }

        public List<GateExitDto> Items { get; set; }
    }
}
