namespace GateEntryExit.Helper
{
    public class GuidGenerator : IGuidGenerator
    {
        public Guid Create()
        {
            return Guid.NewGuid();
        }
    }
}
