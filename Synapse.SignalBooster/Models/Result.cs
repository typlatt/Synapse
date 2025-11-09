using System;

namespace Synapse.SignalBooster.Models
{
    /// <summary>
    /// Represents the result of an operation that may succeed or fail with a string error message.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value</typeparam>
    public class Result<TValue>
    {
        private readonly TValue? _value;
        private readonly string? _error;

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        public TValue Value
        {
            get
            {
                if (IsFailure)
                    throw new InvalidOperationException($"Cannot access Value when result is a failure. Error: {_error}");
                return _value!;
            }
        }

        public string Error
        {
            get
            {
                if (IsSuccess)
                    throw new InvalidOperationException("Cannot access Error when result is a success");
                return _error!;
            }
        }

        private Result(TValue value)
        {
            _value = value;
            _error = null;
            IsSuccess = true;
        }

        private Result(string error)
        {
            _value = default;
            _error = error ?? throw new ArgumentNullException(nameof(error));
            IsSuccess = false;
        }

        public static Result<TValue> Success(TValue value)
            => new Result<TValue>(value);

        public static Result<TValue> Failure(string error)
            => new Result<TValue>(error);

        /// <summary>
        /// Executes one of two functions depending on success or failure.
        /// </summary>
        public TResult Match<TResult>(
            Func<TValue, TResult> onSuccess,
            Func<string, TResult> onFailure)
        {
            return IsSuccess ? onSuccess(Value) : onFailure(Error);
        }

        /// <summary>
        /// Executes one of two actions depending on success or failure.
        /// </summary>
        public void Match(
            Action<TValue> onSuccess,
            Action<string> onFailure)
        {
            if (IsSuccess)
                onSuccess(Value);
            else
                onFailure(Error);
        }

        /// <summary>
        /// Returns the value if success, otherwise returns the provided default value.
        /// </summary>
        public TValue GetValueOrDefault(TValue defaultValue)
        {
            return IsSuccess ? Value : defaultValue;
        }
    }
}
