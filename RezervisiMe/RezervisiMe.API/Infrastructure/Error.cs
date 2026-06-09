
namespace RezervisiMe.RezervisiMe.API.Infrastructure
{
    public class Error
    {
        public string Code { get; }
        public string Description { get; }

        public Error(string code, string description)
        {
            Code = code ?? string.Empty;
            Description = description ?? string.Empty;
        }
        public static Error NotFound(string description) => new Error("NotFound", description);
        public static Error Validation(string description) => new Error("Validation", description);
        public static Error Conflict(string description) => new Error("Conflict", description);
        public static Error Forbidden(string description) => new Error("Forbidden", description);
        public static Error Unauthorized(string description) => new Error("Unauthorized", description);
        public static Error Internal(string description) => new Error("Internal", description);
    }
}