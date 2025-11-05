namespace GateEntryExit.Dtos.Account
{
    public class TfaSetupDto
    {
        public string Email { get; set; }
        public string Code { get; set; }
        public bool IsTfaEnabled { get; set; }
        public string? AuthenticatorKey { get; set; }
        public string? FormattedKey { get; set; }

        public string? Error { get; set; }
    }
}
