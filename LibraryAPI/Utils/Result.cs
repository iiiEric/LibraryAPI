namespace LibraryAPI.Utils
{
    public enum ResultType
    {
        Success,
        NotFound,
        ValidationError,
        Forbidden,
        BadRequest
    }

    public class Result
    {
        public ResultType Type { get; private set; }

        public static Result Success() => new()
        {
            Type = ResultType.Success
        };

        public static Result NotFound() => new()
        {
            Type = ResultType.NotFound
        };

        public static Result Forbidden() => new()
        {
            Type = ResultType.Forbidden
        };

        public static Result BadRequest() => new()
        {
            Type = ResultType.BadRequest
        };

        public static Result ValidationError() => new()
        {
            Type = ResultType.ValidationError
        };
    }
}
