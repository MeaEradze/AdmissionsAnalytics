using System.Collections.Concurrent;

namespace Admissions.Application.Analytics;

public sealed class HealthCache
{
    private readonly ConcurrentDictionary<int, Dictionary<int, HealthResult>> _byYear = new();

    public bool TryGet(int year, out Dictionary<int, HealthResult>? result) =>
        _byYear.TryGetValue(year, out result);

    public void Set(int year, Dictionary<int, HealthResult> result) => _byYear[year] = result;

    public void Clear() => _byYear.Clear();
}
