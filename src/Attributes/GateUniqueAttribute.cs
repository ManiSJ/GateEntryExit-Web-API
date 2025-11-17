using GateEntryExit.Domain.Manager;
using GateEntryExit.Domain.Policy;
using System.ComponentModel.DataAnnotations;

namespace GateEntryExit.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GateUniqueAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var result = ValidationResult.Success;
            if(value == null) { return result; }

            var context = new ValidationContext(value);
            var name = (string?)context.ObjectType.GetProperty("Name")?.GetValue(value, null);

            try
            {
                var gatePolicy = (IGateNameUniquePolicy)validationContext.GetService(typeof(IGateNameUniquePolicy));
                if(gatePolicy == null)
                {
                    return new ValidationResult("Gate serive not found");
                }

                var isNameUnique = gatePolicy.IsNameUniqueAsync(name);
            }
            catch(Exception ex)
            {

            }

            return result;
        }
    }
}
