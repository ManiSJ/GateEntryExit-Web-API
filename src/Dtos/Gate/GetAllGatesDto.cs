namespace GateEntryExit.Dtos.Gate
{
    public class GetAllGatesDto
    {
        public int TotalCount { get; set; }

        public List<GateDetailsDto> Items { get; set; }
    }
}
