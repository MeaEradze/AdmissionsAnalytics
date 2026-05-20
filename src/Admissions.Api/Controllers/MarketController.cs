using Admissions.Application.Dtos;
using Admissions.Application.Features.Market;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Admissions.Api.Controllers;

[ApiController]
[Route("api/market")]
public class MarketController : ControllerBase
{
    private readonly IMediator _mediator;

    public MarketController(IMediator mediator) => _mediator = mediator;

    [HttpGet("gaps")]
    public Task<List<FieldGapDto>> Gaps([FromQuery] int year, CancellationToken ct) =>
        _mediator.Send(new GetMarketGapsQuery(year), ct);

    [HttpGet("overview")]
    public Task<MarketOverviewDto> Overview([FromQuery] int year, CancellationToken ct) =>
        _mediator.Send(new GetMarketOverviewQuery(year), ct);
}
