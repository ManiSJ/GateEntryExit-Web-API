using FluentValidation;
using GateEntryExit.Domain.Policy;
using GateEntryExit.Dtos.Gate;

namespace GateEntryExit.Validators
{
    public class GateNameValidator : AbstractValidator<string>
    {
        public GateNameValidator(IGateNameUniquePolicy _gatePolicy)
        {
            RuleFor(name => name)
                .Custom((name, context) =>
                {
                    var errorMessages = new List<string>();

                    if(name == null)
                    {
                        errorMessages.Add("Name is required");
                    }
                    else if(name != null)
                    {
                        if(name.ToLower() == "GateA".ToLower())
                        {
                            errorMessages.Add($"{name} is not a valid value");
                        }
                    }

                    if(errorMessages.Count() > 0)
                    {
                        context.AddFailure(string.Join(", ", errorMessages));
                    }
                });
        }
    }
}
