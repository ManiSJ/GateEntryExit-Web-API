using FluentValidation;
using GateEntryExit.Domain.Policy;
using GateEntryExit.Dtos.Gate;

namespace GateEntryExit.Validators
{
    public class GateValidator : AbstractValidator<CreateGateDto>
    {
        private readonly IGateNameUniquePolicy _gatePolicy;

        public GateValidator(IGateNameUniquePolicy gatePolicy)
        {
            _gatePolicy = gatePolicy;

            RuleFor(x => x.Name)
                  .NotEmpty()
                  .WithMessage("Name is required");

            When(x => x.Name != null, () => {
                RuleFor(x => x.Name)
                    .SetValidator((x, context) => new GateNameValidator(_gatePolicy));                   
            });

            When(x => x.Name != null, () => {
                RuleFor(x => x.Name)
                    .Custom((name, context) =>
                    {
                        if (name == "Bla")
                        {
                            context.AddFailure($"{name} is not valid");
                        }
                    });
            });
        }
    }
}
