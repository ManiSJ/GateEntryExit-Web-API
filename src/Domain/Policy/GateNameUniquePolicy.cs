using GateEntryExit.Repositories.Interfaces;

namespace GateEntryExit.Domain.Policy
{
    public class GateNameUniquePolicy : IGateNameUniquePolicy
    {

        private readonly IGateRepository _gateRepository;

        public GateNameUniquePolicy(IGateRepository gateRepository)
        {
            _gateRepository = gateRepository;
        }

        public async Task<bool> IsNameUniqueAsync(string name)
        {
            return await _gateRepository.IsNameUniqueAsync(name);
        }
    }
}
