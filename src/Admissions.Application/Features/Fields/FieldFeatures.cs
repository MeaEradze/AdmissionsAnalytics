using Admissions.Application.Common;
using Admissions.Application.Dtos;
using Admissions.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Admissions.Application.Features.Fields;

public sealed record GetFieldsQuery : IRequest<List<FieldDto>>;

public sealed class GetFieldsHandler : IRequestHandler<GetFieldsQuery, List<FieldDto>>
{
    private readonly IAppDbContext _db;

    public GetFieldsHandler(IAppDbContext db) => _db = db;

    public async Task<List<FieldDto>> Handle(GetFieldsQuery request, CancellationToken ct)
    {
        return await _db.Fields
            .AsNoTracking()
            .OrderBy(f => f.Id)
            .Select(f => new FieldDto { Id = f.Id, Name = f.Name, Code = f.Code })
            .ToListAsync(ct);
    }
}

public sealed record CreateFieldCommand(CreateFieldRequest Body) : IRequest<FieldDto>;

public sealed class CreateFieldHandler : IRequestHandler<CreateFieldCommand, FieldDto>
{
    private readonly IAppDbContext _db;

    public CreateFieldHandler(IAppDbContext db) => _db = db;

    public async Task<FieldDto> Handle(CreateFieldCommand request, CancellationToken ct)
    {
        var entity = new Field
        {
            Name = request.Body.Name.Trim(),
            Code = string.IsNullOrWhiteSpace(request.Body.Code) ? null : request.Body.Code.Trim(),
        };

        if (entity.Code is not null &&
            await _db.Fields.AsNoTracking().AnyAsync(f => f.Code == entity.Code, ct))
        {
            throw new ConflictException($"დარგი კოდით „{entity.Code}“ უკვე არსებობს.");
        }

        _db.Fields.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new FieldDto { Id = entity.Id, Name = entity.Name, Code = entity.Code };
    }
}

public sealed record UpdateFieldCommand(int Id, UpdateFieldRequest Body) : IRequest<FieldDto>;

public sealed class UpdateFieldHandler : IRequestHandler<UpdateFieldCommand, FieldDto>
{
    private readonly IAppDbContext _db;

    public UpdateFieldHandler(IAppDbContext db) => _db = db;

    public async Task<FieldDto> Handle(UpdateFieldCommand request, CancellationToken ct)
    {
        var entity = await _db.Fields.FirstOrDefaultAsync(f => f.Id == request.Id, ct)
            ?? throw new NotFoundException("დარგი", request.Id);

        string? newCode = string.IsNullOrWhiteSpace(request.Body.Code) ? null : request.Body.Code.Trim();
        if (newCode is not null &&
            await _db.Fields.AsNoTracking().AnyAsync(f => f.Id != request.Id && f.Code == newCode, ct))
        {
            throw new ConflictException($"დარგი კოდით „{newCode}“ უკვე არსებობს.");
        }

        entity.Name = request.Body.Name.Trim();
        entity.Code = newCode;
        await _db.SaveChangesAsync(ct);

        return new FieldDto { Id = entity.Id, Name = entity.Name, Code = entity.Code };
    }
}
