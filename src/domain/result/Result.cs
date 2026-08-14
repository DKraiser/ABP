namespace ABP.Domain.Result;

public class Result(bool IsSuccessful, Error? Error = null) {
    
    /// <summary>
    /// Gets a flag of whether an operation finished successfully.
    /// </summary>
    /// <value>Boolean flag representing whether an operation finished successfully.</value>
    public bool IsSuccessful { get; } = IsSuccessful;

    /// <summary>
    /// Gets an error object created if an operation finished unsuccessfully.
    /// </summary>
    /// <value>Error object containing a dictionary of problem occured.</value>
    public Error? Error { get; } = Error;

    /// <summary>
    /// Should be called if an operation finished successfully.
    /// </summary>
    /// <returns>`Result` object with with a value and no errors.</returns>
    public static Result Success() => new (true, null);

    /// <summary>
    /// Should be called if an operation finished unsuccessfully.
    /// </summary>
    /// <param name="error">An object of type derived from `Error`.</param>
    /// <returns>`Result` object contatining a set of errors occured during performing an operation.</returns>
    public static Result Failure(Error error) => new (false, error);
}

/// <summary>
/// Represents a result of some action performed by an application. 
/// If an action finished with success, it will contain a result,
/// else - an error object.
/// </summary> 
/// <typeparam name="T">Represents a type of result.</typeparam>
public class Result<T>(bool IsSuccessful, T? Value = null, Error? Error = null) : Result(IsSuccessful, Error) where T : class {
    
    /// <summary>
    /// Gets a value returned if an operation finished successfully.
    /// </summary>
    /// <value>An object of type `T` representing a result of successful finishing an operation.</value>
    public T? Value { get; } = Value;
    
    /// <summary>
    /// Should be called if an operation finished successfully.
    /// </summary>
    /// <param name="value">An expected result of an operation.</param>
    /// <returns>`Result` object with with a value and no errors.</returns>
    public static Result<T> Success(T value) => new (true, value, null);

    /// <summary>
    /// Should be called if an operation finished unsuccessfully.
    /// </summary>
    /// <param name="error">An object of type derived from `Error`.</param>
    /// <returns>`Result` object contatining a set of errors occured during performing an operation.</returns>
    public static new Result<T> Failure(Error error) => new (false, null, error);
}

/// <summary>
/// Represents a result of some action performed by an application. 
/// If an action finished with success, it will contain a result,
/// else - an error object.
/// </summary> 
/// <typeparam name="T">Represents a type of result.</typeparam>
public class ValueResult<T> (bool IsSuccessful, T? Value = null, Error? Error = null) : Result(IsSuccessful, Error) where T : struct {
 
    /// <summary>
    /// Gets a value returned if an operation finished successfully.
    /// </summary>
    /// <value>An object of type `T` representing a result of successful finishing an operation.</value>
    public T? Value { get; } = Value;
    
    /// <summary>
    /// Should be called if an operation finished successfully.
    /// </summary>
    /// <param name="value">An expected result of an operation.</param>
    /// <returns>`Result` object with with a value and no errors.</returns>
    public static ValueResult<T> Success(T value) => new (true, value, null);

    /// <summary>
    /// Should be called if an operation finished unsuccessfully.
    /// </summary>
    /// <param name="error">An object of type derived from `Error`.</param>
    /// <returns>`Result` object contatining a set of errors occured during performing an operation.</returns>
    public static new ValueResult<T> Failure(Error error) => new (false, null, error);
}