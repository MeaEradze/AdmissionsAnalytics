using Admissions.Application.Dtos;
using Admissions.Application.Features.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Admissions.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator) => _mediator = mediator;

    [HttpGet("summary")]
    public Task<DashboardSummaryDto> Summary([FromQuery] int year, CancellationToken ct) =>
        _mediator.Send(new GetDashboardSummaryQuery(year), ct);
}
