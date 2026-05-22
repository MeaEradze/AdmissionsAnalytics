using Admissions.Application.Common;
using Admissions.Application.Dtos;
using Admissions.Application.Imports;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Admissions.Api.Controllers;

[ApiController]
[Route("api/import")]
public class ImportController : ControllerBase
{

    private const long MaxUploadBytes = 50_000_000;

    private static readonly string[] XlsxContentTypes =
    [
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/octet-stream",
    ];

    private static readonly string[] PdfContentTypes =
    [
        "application/pdf",
        "application/octet-stream",
    ];

    private readonly IMediator _mediator;

    public ImportController(IMediator mediator) => _mediator = mediator;

    [HttpPost("enrollments")]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<ActionResult<ImportResultDto>> Enrollments(
        IFormFile file,
        [FromQuery] int year,
        CancellationToken ct)
    {
        ValidateUpload(file, ".xlsx", XlsxContentTypes, "საჭიროა .xlsx Excel ფაილი");

        await using var stream = await BufferAsync(file, ct);
        return Ok(await _mediator.Send(new ImportEnrollmentsCommand(stream, year), ct));
    }

    [HttpPost("priorities")]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<ActionResult<ImportResultDto>> Priorities(
        IFormFile file,
        [FromQuery] int year,
        CancellationToken ct)
    {
        ValidateUpload(file, ".xlsx", XlsxContentTypes, "საჭიროა .xlsx Excel ფაილი");

        await using var stream = await BufferAsync(file, ct);
        return Ok(await _mediator.Send(new ImportPrioritiesCommand(stream, year), ct));
    }

    [HttpPost("handbook")]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<ActionResult<ImportResultDto>> Handbook(
        IFormFile file,
        [FromQuery] int year,
        CancellationToken ct)
    {
        ValidateUpload(file, ".pdf", PdfContentTypes, "საჭიროა PDF ფაილი");

        await using var stream = await BufferAsync(file, ct);
        return Ok(await _mediator.Send(new ImportHandbookCommand(stream, year), ct));
    }

    private static void ValidateUpload(
        IFormFile? file, string extension, string[] allowedContentTypes, string expected)
    {
        if (file is null || file.Length == 0)
        {
            throw new InvalidFileFormatException($"ფაილი ცარიელია — {expected}.");
        }

        if (!Path.GetExtension(file.FileName).Equals(extension, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidFileFormatException($"ფაილის ფორმატი არასწორია — {expected}.");
        }

        if (!string.IsNullOrEmpty(file.ContentType) &&
            !allowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidFileFormatException($"ფაილის ფორმატი არასწორია — {expected}.");
        }
    }

    private static async Task<FileStream> BufferAsync(IFormFile file, CancellationToken ct)
    {
        var temp = new FileStream(
            Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()),
            FileMode.CreateNew,
            FileAccess.ReadWrite,
            FileShare.None,
            bufferSize: 81920,
            FileOptions.DeleteOnClose);
        try
        {
            await file.CopyToAsync(temp, ct);
            temp.Position = 0;
            return temp;
        }
        catch
        {
            await temp.DisposeAsync();
            throw;
        }
    }
}
