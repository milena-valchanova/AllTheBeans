using System.Text;

namespace AllTheBeans.API.IntegrationTests.Helpers;

internal static class ContentBuilder
{
    public static async Task<HttpContent> BuildJsonContentFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File {filePath} does not exist");

        var json = await File.ReadAllTextAsync(filePath);

        return BuildJsonContent(json);
    }

    private static StringContent BuildJsonContent(string content)
        => new(content, Encoding.UTF8, "application/json");
}
