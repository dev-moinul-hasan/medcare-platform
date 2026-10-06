namespace MedCare.Application.Exceptions;

public class ApplicationExceptions
{
    /// <summary>
    /// Thrown when an entity is not found by ID or key.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
        public NotFoundException(string entityName, object key) : base($"{entityName} with key '{key}' was not found.") { }
    }

    /// <summary>
    /// Thrown when business invariants or domain rules are violated.
    /// </summary>
    public class BusinessRuleException(string message) : Exception(message)
    {
    }

    /// <summary>
    /// Thrown when incoming request fields fail validation rules.
    /// </summary>
    public class ValidationException(IDictionary<string, string[]> errors, string message = "One or more validation errors occurred.") : Exception(message)
    {
        public IDictionary<string, string[]> Errors { get; } = errors;
    }


}
