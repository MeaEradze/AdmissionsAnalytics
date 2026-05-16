using Admissions.Application.Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Admissions.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            var problem = new ValidationProblemDetails(
                ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "ვალიდაციის შეცდომა",
                Status = StatusCodes.Status400BadRequest,
                Detail = "მოთხოვნა შეიცავს არასწორ მონაცემებს.",
                Instance = context.Request.Path,
            };
            await WriteProblem(context, problem);
        }
        catch (NotFoundException ex)
        {
            var problem = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                Title = "ვერ მოიძებნა",
                Status = StatusCodes.Status404NotFound,
                Detail = ex.Message,
                Instance = context.Request.Path,
            };
            await WriteProblem(context, problem);
        }
        catch (InvalidFileFormatException ex)
        {
            var problem = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "ფაილის ფორმატი არასწორია",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message,
                Instance = context.Request.Path,
            };
            await WriteProblem(context, problem);
        }
        catch (ConflictException ex)
        {
            var problem = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                Title = "კონფლიქტი",
                Status = StatusCodes.Status409Conflict,
                Detail = ex.Message,
                Instance = context.Request.Path,
            };
            await WriteProblem(context, problem);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {

            var problem = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                Title = "კონფლიქტი",
                Status = StatusCodes.Status409Conflict,
                Detail = "ჩანაწერი ასეთი კოდით უკვე არსებობს.",
                Instance = context.Request.Path,
            };
            await WriteProblem(context, problem);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Path}", context.Request.Path);
            var problem = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                Title = "სერვერის შეცდომა",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "დაფიქსირდა მოულოდნელი შეცდომა.",
                Instance = context.Request.Path,
            };
            await WriteProblem(context, problem);
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.GetBaseException() switch
        {
            Microsoft.Data.SqlClient.SqlException sql => sql.Number is 2601 or 2627,
            var inner => inner.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) ||
                         inner.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase),
        };

    private static async Task WriteProblem(HttpContext context, ProblemDetails problem)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(
            problem, problem.GetType(), options: null, contentType: "application/problem+json");
    }
}
