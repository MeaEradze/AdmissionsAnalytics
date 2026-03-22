namespace Admissions.Application.Common;

public class InvalidFileFormatException : Exception
{
    public InvalidFileFormatException(string message)
        : base(message)
    {
    }
}
