using Admissions.Application.Dtos;
using Admissions.Application.Features.Portfolio;
using Admissions.Application.Features.Trends;
using Admissions.Application.Features.Universities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Admissions.Api.Controllers;

[ApiController]
[Route("api/universities")]
public class UniversitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public UniversitiesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public Task<List<UniversityDto>> List(CancellationToken ct) =>
        _mediator.Send(new GetUniversitiesQuery(), ct);

    [HttpPost]
    public async Task<ActionResult<UniversityDto>> Create(
        [FromBody] CreateUniversityRequest body,
        CancellationToken ct)
    {
        var created = await _mediator.Send(new CreateUniversityCommand(body), ct);
        return Ok(created);
    }

    [HttpGet("{id:int}/trend")]
    public Task<TrendResultDto> Trend(int id, CancellationToken ct) =>
        _mediator.Send(new GetUniversityTrendQuery(id), ct);

    [HttpGet("{id:int}/portfolio")]
    public Task<List<PortfolioItemDto>> Portfolio(int id, [FromQuery] int year, CancellationToken ct) =>
        _mediator.Send(new GetUniversityPortfolioQuery(id, year), ct);
}
