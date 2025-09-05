using GateEntryExit.Domain.Policy;

namespace GateEntryExit.Domain.Manager
{
    public class GateManager : IGateManager
    {
        private readonly IGateNameUniquePolicy _gateNameUniquePolicy;

        public GateManager(IGateNameUniquePolicy gateNameUniquePolicy)
        {
            _gateNameUniquePolicy = gateNameUniquePolicy;
        }

        public async Task<Gate> CreateAsync(Guid id, string name)
        {
            var isNameUnique = await _gateNameUniquePolicy.IsNameUniqueAsync(name);

            if (!isNameUnique)
                throw new Exception("Name is not unique");

            return new Gate(id, name);
        }
    }
}
