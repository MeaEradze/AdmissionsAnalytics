using Admissions.Application.Dtos;
using Admissions.Application.Features.Compare;
using Admissions.Application.Features.Competition;
using Admissions.Application.Features.Health;
using Admissions.Application.Features.ProgramAnalytics;
using Admissions.Application.Features.Programs;
using Admissions.Application.Features.Trends;
using Admissions.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Admissions.Api.Controllers;

[ApiController]
[Route("api/programs")]
public class ProgramsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProgramsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public Task<PagedResponse<ProgramListItemDto>> List(
        [FromQuery] int? universityId,
        [FromQuery] int? fieldId,
        [FromQuery] int? year,
        [FromQuery] decimal? minFee,
        [FromQuery] decimal? maxFee,
        [FromQuery] string? search,
        [FromQuery] HealthCategory? healthCategory,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        _mediator.Send(
            new GetProgramsQuery(universityId, fieldId, year, minFee, maxFee, search, healthCategory, page, pageSize),
            ct);

    [HttpGet("health")]
    public Task<HealthListResponse> HealthList(
        [FromQuery] HealthCategory? category,
        [FromQuery] int? fieldId,
        [FromQuery] int? universityId,
        [FromQuery] int? year,
        [FromQuery] decimal? minFee,
        [FromQuery] decimal? maxFee,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        _mediator.Send(
            new GetHealthListQuery(category, fieldId, universityId, year, minFee, maxFee, page, pageSize), ct);

    [HttpGet("compare")]
    public Task<List<ProgramComparisonItemDto>> Compare(
        [FromQuery] string ids,
        [FromQuery] int year,
        CancellationToken ct) =>
        _mediator.Send(new CompareProgramsQuery(ids, year), ct);

    [HttpGet("{id:int}")]
    public Task<ProgramDetailDto> Get(int id, CancellationToken ct) =>
        _mediator.Send(new GetProgramDetailQuery(id), ct);

    [HttpPut("{id:int}/year-stats/{year:int}")]
    public async Task<IActionResult> UpdateYearStats(
        int id,
        int year,
        [FromBody] UpdateProgramYearRequest body,
        CancellationToken ct)
    {
        await _mediator.Send(new UpdateProgramYearStatsCommand(id, year, body), ct);
        return NoContent();
    }

    [HttpPut("{id:int}/field")]
    public async Task<IActionResult> AssignField(
        int id,
        [FromBody] AssignProgramFieldRequest body,
        CancellationToken ct)
    {
        await _mediator.Send(new AssignProgramFieldCommand(id, body.FieldId), ct);
        return NoContent();
    }

    [HttpGet("{id:int}/competition")]
    public Task<ProgramCompetitionTrendDto> Competition(
        int id,
        [FromQuery] int fromYear,
        [FromQuery] int toYear,
        CancellationToken ct) =>
        _mediator.Send(new GetProgramCompetitionQuery(id, fromYear, toYear), ct);

    [HttpGet("{id:int}/health")]
    public Task<ProgramHealthDto> Health(int id, [FromQuery] int year, CancellationToken ct) =>
        _mediator.Send(new GetProgramHealthQuery(id, year), ct);

    [HttpGet("{id:int}/forecast")]
    public Task<ProgramForecastDto> Forecast(int id, CancellationToken ct) =>
        _mediator.Send(new GetProgramForecastQuery(id), ct);

    [HttpGet("{id:int}/trend")]
    public Task<TrendResultDto> Trend(int id, CancellationToken ct) =>
        _mediator.Send(new GetProgramTrendQuery(id), ct);

    [HttpGet("{id:int}/benchmark")]
    public Task<ProgramBenchmarkDto> Benchmark(int id, [FromQuery] int year, CancellationToken ct) =>
        _mediator.Send(new GetProgramBenchmarkQuery(id, year), ct);

    [HttpGet("{id:int}/priority-distribution")]
    public Task<PriorityDistributionDto> PriorityDistribution(
        int id,
        [FromQuery] int year,
        CancellationToken ct) =>
        _mediator.Send(new GetPriorityDistributionQuery(id, year), ct);

    [HttpGet("{id:int}/conversion")]
    public Task<ProgramConversionDto> Conversion(int id, CancellationToken ct) =>
        _mediator.Send(new GetProgramConversionQuery(id), ct);

    [HttpGet("{id:int}/fee-sensitivity")]
    public Task<FeeSensitivityDto> FeeSensitivity(int id, CancellationToken ct) =>
        _mediator.Send(new GetFeeSensitivityQuery(id), ct);
}
