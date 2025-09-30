using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace AllTheBeans.API.IntegrationTests.Helpers;

internal static class ResponseAssertionHelper
{
    public static async Task VerifyBadRequest(HttpResponseMessage? responseMessage, string payloadFilePath)
    {
        Assert.That(responseMessage?.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var expectedErrorsFile = payloadFilePath.Replace("payload", "errors");

        if (!File.Exists(expectedErrorsFile))
            throw new FileNotFoundException($"The expected errors file {expectedErrorsFile} does not exist");

        var expectedErrors = await File.ReadAllTextAsync(expectedErrorsFile);
        if (string.IsNullOrWhiteSpace(expectedErrors))
            throw new ArgumentException($"The expected errors file {expectedErrorsFile} is empty");

        var responseErrors = await GetBadRequestErrorMessageAsync(responseMessage);
        Assert.That(MinifyJson(responseErrors), Is.EqualTo(MinifyJson(expectedErrors)));
    }

    private static async Task<string> GetBadRequestErrorMessageAsync(HttpResponseMessage? responseMessage)
    {
        Assert.That(responseMessage, Is.Not.Null, "Response message should not be null");

        var problemDetails = await responseMessage.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.That(problemDetails, Is.Not.Null, $"Response message is not of type {nameof(ProblemDetails)}");

        var errors = problemDetails.Extensions["errors"];
        Assert.That(errors, Is.Not.Null, "There are no errors found");
        
        return errors.ToString();
    }

    private static string MinifyJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(doc, jsonSerializerOptions);
    }

    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        WriteIndented = false
    };
}
