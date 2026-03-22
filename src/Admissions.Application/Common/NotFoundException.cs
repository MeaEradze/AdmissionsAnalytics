namespace Admissions.Application.Common;

public class NotFoundException : Exception
{
    public NotFoundException(string entity, object key)
        : base($"{entity} ({key}) ვერ მოიძებნა.")
    {
    }
}
