using Cowork.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Net;
using System.Text.Json;

namespace Cowork.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var response = exception switch
        {
            ReservationConflictException reservationConflictException => CreateResponse(
                HttpStatusCode.Conflict,
                "Reservation conflict",
                reservationConflictException.Message),

            NotFoundException notFoundException => CreateResponse(
                HttpStatusCode.NotFound,
                "Not found",
                notFoundException.Message),

            BusinessRuleException businessRuleException => CreateResponse(
                HttpStatusCode.BadRequest,
                "Business rule validation failed",
                businessRuleException.Message),

            DbUpdateException dbUpdateException
                when TryGetPostgresException(dbUpdateException, out var postgresException)
                => HandlePostgresException(postgresException),

            ArgumentException argumentException => CreateResponse(
                HttpStatusCode.BadRequest,
                "Invalid request",
                argumentException.Message),

            UnauthorizedAccessException unauthorizedAccessException => CreateResponse(
                HttpStatusCode.Unauthorized,
                "Unauthorized",
                unauthorizedAccessException.Message),

            _ => CreateResponse(
                HttpStatusCode.InternalServerError,
                "Unexpected error",
                "An unexpected error occurred while processing the request.")
        };

        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred.");
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception occurred.");
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)response.StatusCode;

        var payload = JsonSerializer.Serialize(new
        {
            status = context.Response.StatusCode,
            error = response.Title,
            message = response.Message
        });

        await context.Response.WriteAsync(payload);
    }

    private static bool TryGetPostgresException(
        DbUpdateException exception,
        out PostgresException postgresException)
    {
        if (exception.InnerException is PostgresException innerPostgresException)
        {
            postgresException = innerPostgresException;
            return true;
        }

        postgresException = null!;
        return false;
    }

    private static ErrorResponse HandlePostgresException(PostgresException exception)
    {
        return exception.SqlState switch
        {
            PostgresErrorCodes.ExclusionViolation => CreateResponse(
                HttpStatusCode.Conflict,
                "Reservation conflict",
                "The selected space is no longer available for the requested time range."),

            PostgresErrorCodes.UniqueViolation => CreateResponse(
                HttpStatusCode.Conflict,
                "Duplicated record",
                "A record with the same unique value already exists."),

            PostgresErrorCodes.ForeignKeyViolation => CreateResponse(
                HttpStatusCode.BadRequest,
                "Invalid reference",
                "One or more referenced records do not exist."),

            PostgresErrorCodes.CheckViolation => CreateResponse(
                HttpStatusCode.BadRequest,
                "Invalid data",
                "The submitted data does not satisfy database constraints."),

            _ => CreateResponse(
                HttpStatusCode.InternalServerError,
                "Database error",
                "A database error occurred while processing the request.")
        };
    }

    private static ErrorResponse CreateResponse(
        HttpStatusCode statusCode,
        string title,
        string message)
    {
        return new ErrorResponse(statusCode, title, message);
    }

    private sealed record ErrorResponse(
        HttpStatusCode StatusCode,
        string Title,
        string Message);
}