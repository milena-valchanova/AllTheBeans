using AllTheBeans.API.Controllers;
using AllTheBeans.API.Mappers;
using AllTheBeans.Domain.DataModels;
using AllTheBeans.Domain.Repositories;
using AllTheBeans.Domain.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using NSubstitute.ClearExtensions;
using System.Net;

namespace AllTheBeans.API.IntegrationTests.BeansControllerTests.DELETE;

[TestFixture(TestOf = typeof(BeansController))]
internal class ValidationTests
{
    private readonly IBeansService _beansService =
        Substitute.For<IBeansService>();
    private WebApplicationFactory<Program> _factory;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");
                builder.ConfigureTestServices(services =>
                {
                    services.Replace(ServiceDescriptor.Scoped(_ => _beansService));
                });
            });
    }

    [TearDown]
    public void TearDown()
    {
        _beansService.ClearSubstitute();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _factory.DisposeAsync();
    }

    [TestCase("/beans/123")]
    [TestCase("/beans/invalid-value")]
    [Description("Invalid query parameters should return Bad Request status code")]
    public async Task InvalidRequests_Should_ReturnBadRequest(string endpoint)
    {
        using var httpClient = _factory.CreateClient();

        using var response = await httpClient.DeleteAsync(endpoint);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
