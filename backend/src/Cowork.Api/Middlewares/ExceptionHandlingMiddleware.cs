using Cowork.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Cowork.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problem = exception switch
        {
            NotFoundException => CreateProblem(
                StatusCodes.Status404NotFound,
                "Resource not found",
                exception.Message),

            BusinessRuleException => CreateProblem(
                StatusCodes.Status400BadRequest,
                "Business rule violation",
                exception.Message),

            ReservationConflictException => CreateProblem(
                StatusCodes.Status409Conflict,
                "Reservation conflict",
                exception.Message),

            ArgumentException => CreateProblem(
                StatusCodes.Status400BadRequest,
                "Invalid request",
                exception.Message),

            InvalidOperationException => CreateProblem(
                StatusCodes.Status400BadRequest,
                "Invalid operation",
                exception.Message),

            _ => CreateProblem(
                StatusCodes.Status500InternalServerError,
                "Unexpected error",
                "An unexpected error occurred.")
        };

        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problem);
    }

    private static ProblemDetails CreateProblem(int statusCode, string title, string detail)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };
    }
}