namespace Infrastructure.Validation
{
    public class Validation
    {
        public ValidationResult Result;
        public string           Message;

        public Validation(ValidationResult result, string message)
        {
            Result  = result;
            Message = message;
        }
    }

    public enum ValidationResult
    {
        Undefined,
        Invalid,
        Warning,
        Valid
    }
}