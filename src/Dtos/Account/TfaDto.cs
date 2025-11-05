namespace GateEntryExit.Dtos.Account
{
    public class TfaDto
    {
        public string? Email { get; set; }

        public string? Code { get; set; }

        public string RefreshToken { get; set; }
    }
}
