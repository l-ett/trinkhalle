using System.Net;
using FluentResults;
using Microsoft.Azure.Functions.Worker.Http;

namespace Trinkhalle.Shared.Extensions;

public static class HttpRequestDataExtensions
{
    public static async Task<HttpResponseData> CreateResponseAsync<T>(this HttpRequestData request,
        HttpStatusCode statusCode,
        Result<T> result)
    {
        var response = request.CreateResponse();
        response.StatusCode = statusCode;

        if (result.IsFailed)
        {
            await response.WriteStringAsync(result.Errors.FirstOrDefault()?.Message ?? string.Empty);
        }

        if (result.IsSuccess)
        {
            if (result.Value is Guid)
            {
                await response.WriteStringAsync(result.ValueOrDefault?.ToString() ?? string.Empty);
            }
            else
            {
                await response.WriteAsJsonAsync(result.ValueOrDefault);
            }
        }

        return response;
    }

    public static async Task<HttpResponseData> CreateResponseAsync(this HttpRequestData request,
        HttpStatusCode statusCode,
        Result result)
    {
        var response = request.CreateResponse();
        response.StatusCode = statusCode;

        if (result.IsFailed)
        {
            await response.WriteStringAsync(result.Errors.FirstOrDefault()?.Message ?? string.Empty);
        }

        return response;
    }
}