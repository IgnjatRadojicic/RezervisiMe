
namespace RezervisiMe.RezervisiMe.API.Infrastructure
{
    public class Result<T>
    {
        public T Value { get; }
        public Error Error { get; }
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        private Result(T Value)
        {
            Value = Value;
            IsSuccess = true;
        }
        private Result(Error error)
        {
            Error = error;
            IsSuccess = false;
        }

        public static Result<T> Success(T Value) => new Result<T>(Value);
        public static Result<T> Failure(Error error) => new Result<T>(error);

        public static implicit operator Result<T>(T value) => Success(value);
        public static implicit operator Result<T>(Error error) => Failure(error);
    }
}