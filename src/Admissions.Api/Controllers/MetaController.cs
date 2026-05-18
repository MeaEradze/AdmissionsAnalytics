using Admissions.Application.Features.Meta;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Admissions.Api.Controllers;

[ApiController]
[Route("api/meta")]
public class MetaController : ControllerBase
{
    private readonly IMediator _mediator;

    public MetaController(IMediator mediator) => _mediator = mediator;

    [HttpGet("years")]
    public Task<List<int>> Years(CancellationToken ct) =>
        _mediator.Send(new GetYearsQuery(), ct);
}
