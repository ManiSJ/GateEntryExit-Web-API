using System.ComponentModel.DataAnnotations;

namespace GateEntryExit.Attributes
{
    // How to use this converter? In any property like below
    // [DateTimeValidator("Bla")]
    // public string? DateTime { get; set; } 
    #region
    // Used string type above not dateTime because datetime in json will be a string if used dateTime it will 
    // convert to local time or UTC something like, the point is we need data as in json so string used.
    #endregion

    public class DateTimeValidator : ValidationAttribute
    {
        public DateTimeValidator(string wayToPassParameter)
        {
            
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var validationResult = ValidationResult.Success;

            if (!IsValidDate(value.ToString().Trim()))
            {
                validationResult = new ValidationResult($"{validationContext.MemberName} is not a valid date time");
            }

            if (IsInFuture(value.ToString().Trim()))
            {
                validationResult = new ValidationResult($"{validationContext.MemberName} is in future");
            }

            if (Is45DaysOlder(value.ToString().Trim()))
            {
                validationResult = new ValidationResult($"{validationContext.MemberName} is older than 30 days");
            }

            return validationResult;
        }

        private bool IsValidDate(string value)
        {
            DateTimeOffset parsedDateTimeOffset;
            return DateTimeOffset.TryParse(value, out parsedDateTimeOffset);
        }

        private bool IsInFuture(string value)
        {
            bool isInFuture = false;

            DateTimeOffset parsedDateTimeOffset;
            DateTimeOffset currentDateTime = DateTimeOffset.Now;

            DateTimeOffset.TryParse(value, out parsedDateTimeOffset);

            if(parsedDateTimeOffset > currentDateTime)
            {
                isInFuture = true;
            }

            return isInFuture;
        }

        private bool Is45DaysOlder(string value)
        {
            bool is45DaysOlder = false;

            DateTimeOffset parsedDateTimeOffset;
            DateTimeOffset currentDateTime = DateTimeOffset.Now;

            DateTimeOffset.TryParse(value, out parsedDateTimeOffset);

            if (parsedDateTimeOffset < currentDateTime.AddDays(-45))
            {
                is45DaysOlder = true;
            }

            return is45DaysOlder;
        }
    }
}
