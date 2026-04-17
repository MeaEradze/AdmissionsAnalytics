using Admissions.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Admissions.Application.Features.Meta;

public sealed record GetYearsQuery : IRequest<List<int>>;

public sealed class GetYearsHandler : IRequestHandler<GetYearsQuery, List<int>>
{
    private readonly IAppDbContext _db;

    public GetYearsHandler(IAppDbContext db) => _db = db;

    public async Task<List<int>> Handle(GetYearsQuery request, CancellationToken ct)
    {
        return await _db.ProgramYearStats
            .AsNoTracking()
            .Select(s => s.Year)
            .Distinct()
            .OrderByDescending(y => y)
            .ToListAsync(ct);
    }
}
