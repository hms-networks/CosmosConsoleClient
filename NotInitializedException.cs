using System;

namespace CosmosConsoleClient
{
    /// <summary>
    ///     Exception class for indication of a non-initialized command (internal program error).
    /// </summary>
    public class NotInitializedException : Exception
    {
        public NotInitializedException() : base() { }
        public NotInitializedException(string message) : base(message) { }
        public NotInitializedException(string message, Exception e) : base(message, e) { }
    }
}
