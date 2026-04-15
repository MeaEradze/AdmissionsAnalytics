using Admissions.Application.Common;
using Admissions.Application.Dtos;
using Admissions.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Admissions.Application.Features.Universities;

public sealed record GetUniversitiesQuery : IRequest<List<UniversityDto>>;

public sealed class GetUniversitiesHandler : IRequestHandler<GetUniversitiesQuery, List<UniversityDto>>
{
    private readonly IAppDbContext _db;

    public GetUniversitiesHandler(IAppDbContext db) => _db = db;

    public async Task<List<UniversityDto>> Handle(GetUniversitiesQuery request, CancellationToken ct)
    {
        return await _db.Universities
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .Select(u => new UniversityDto
            {
                Id = u.Id,
                Name = u.Name,
                ShortName = u.ShortName,
                Code = u.Code,
            })
            .ToListAsync(ct);
    }
}

public sealed record CreateUniversityCommand(CreateUniversityRequest Body) : IRequest<UniversityDto>;

public sealed class CreateUniversityHandler : IRequestHandler<CreateUniversityCommand, UniversityDto>
{
    private readonly IAppDbContext _db;

    public CreateUniversityHandler(IAppDbContext db) => _db = db;

    public async Task<UniversityDto> Handle(CreateUniversityCommand request, CancellationToken ct)
    {
        var entity = new University
        {
            Name = request.Body.Name.Trim(),
            ShortName = string.IsNullOrWhiteSpace(request.Body.ShortName) ? null : request.Body.ShortName.Trim(),
            Code = string.IsNullOrWhiteSpace(request.Body.Code) ? null : request.Body.Code.Trim(),
        };

        if (entity.Code is not null &&
            await _db.Universities.AsNoTracking().AnyAsync(u => u.Code == entity.Code, ct))
        {
            throw new ConflictException($"უნივერსიტეტი კოდით „{entity.Code}“ უკვე არსებობს.");
        }

        _db.Universities.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new UniversityDto
        {
            Id = entity.Id,
            Name = entity.Name,
            ShortName = entity.ShortName,
            Code = entity.Code,
        };
    }
}
