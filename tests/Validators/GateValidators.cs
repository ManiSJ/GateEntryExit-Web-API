using FluentValidation.Results;
using FluentValidation.TestHelper;
using GateEntryExit.Domain.Policy;
using GateEntryExit.Dtos.Gate;
using GateEntryExit.Validators;
using Moq;

namespace GateEntryExit.Test.Validators
{
    public class GateValidators
    {
        private readonly GateValidator _gateValidator;
        private readonly Mock<IGateNameUniquePolicy> _mockGatePolicy;

        public GateValidators()
        {
            _mockGatePolicy = new Mock<IGateNameUniquePolicy>();
            _gateValidator = new GateValidator(_mockGatePolicy.Object);
        }

        [Fact]
        public void Gate_Required()
        {
            var gate = new CreateGateDto();
            var result = _gateValidator.TestValidate(gate);
            List<ValidationFailure> resultErrors = result.Errors;
        }
    }
}
