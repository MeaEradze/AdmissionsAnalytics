using Admissions.Application.Dtos;
using Admissions.Application.Features.Competition;
using Admissions.Application.Features.Fields;
using Admissions.Application.Features.Trends;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Admissions.Api.Controllers;

[ApiController]
[Route("api/fields")]
public class FieldsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FieldsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public Task<List<FieldDto>> List(CancellationToken ct) =>
        _mediator.Send(new GetFieldsQuery(), ct);

    [HttpPost]
    public async Task<ActionResult<FieldDto>> Create([FromBody] CreateFieldRequest body, CancellationToken ct)
    {
        var created = await _mediator.Send(new CreateFieldCommand(body), ct);
        return Ok(created);
    }

    [HttpPut("{id:int}")]
    public Task<FieldDto> Update(int id, [FromBody] UpdateFieldRequest body, CancellationToken ct) =>
        _mediator.Send(new UpdateFieldCommand(id, body), ct);

    [HttpGet("{fieldId:int}/competition")]
    public Task<FieldCompetitionDto> Competition(int fieldId, [FromQuery] int year, CancellationToken ct) =>
        _mediator.Send(new GetFieldCompetitionQuery(fieldId, year), ct);

    [HttpGet("{fieldId:int}/trend")]
    public Task<TrendResultDto> Trend(int fieldId, CancellationToken ct) =>
        _mediator.Send(new GetFieldTrendQuery(fieldId), ct);
}
