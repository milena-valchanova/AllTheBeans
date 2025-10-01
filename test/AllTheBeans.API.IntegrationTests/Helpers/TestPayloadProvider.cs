using System.Reflection;

namespace AllTheBeans.API.IntegrationTests.Helpers;
internal static class TestPayloadProvider
{
    public static string GetFirstValidPayloadFilePath(string type)
        => EnumeratePayloadFilePaths("Valid", type, "*.json")
            .FirstOrDefault()
            ?? throw new InvalidOperationException("No valid data was found");

    public static IEnumerable<string> GetValidPayloadFilePaths(string type)
        => EnumeratePayloadFilePaths("Valid", type, "*.json");

    public static IEnumerable<string> GetInvalidPayloadFilePaths(string type)
        => EnumeratePayloadFilePaths("Invalid", type, "*-payload.json");

    private static IEnumerable<string> EnumeratePayloadFilePaths(string baseDir, string type, string searchPattern)
    {
        var directoryPath = Path.Combine(".", "TestData", baseDir, type);

        if (!Directory.Exists(directoryPath))
            yield break;

        foreach(var file in Directory.GetFiles(directoryPath, searchPattern))
            yield return file;
    }
}
