using System;

namespace CDP.Core.Extensions
{
    public static class ExceptionExtensions
    {
        public static string ToLogString(this Exception ex)
        {
            return ToLogString(ex, null);
        }

        public static string ToLogString(this Exception exception, string errorMessage)
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(errorMessage))
            {
                result = errorMessage + "\n\n";
            }

            if (exception != null)
            {
                Action<Exception> appendExceptionInfo = null;

                appendExceptionInfo = e =>
                {
                    if (e.InnerException != null)
                    {
                        appendExceptionInfo(e.InnerException);
                    }

                    result += e.Message + "\n\n" + "Inner Exception: " + (e != exception).ToString() + "\n\n" + "Source: " + e.Source + "\n\n" + "Type: " + e.GetType().ToString() + "\n\n" + e.StackTrace + "\n\n";
                };

                appendExceptionInfo(exception);
            }

            return result;
        }
    }
}
