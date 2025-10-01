using AllTheBeans.API.Controllers;
using AllTheBeans.API.Mappers;
using AllTheBeans.Domain.DataModels;
using AllTheBeans.Domain.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using NSubstitute.ClearExtensions;
using System.Net;

namespace AllTheBeans.API.IntegrationTests.BeansControllerTests.GET_ById;

[TestFixture(TestOf = typeof(BeansController))]
internal class ValidationTests
{
    private readonly IBeansService _beansService = 
        Substitute.For<IBeansService>();
    private readonly IBeansMapper _beansMapper =
        Substitute.For<IBeansMapper>();
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
                    services.Replace(ServiceDescriptor.Singleton(_ => _beansMapper));
                });
            });
    }

    [TearDown]
    public void TearDown()
    {
        _beansService.ClearSubstitute();
        _beansMapper.ClearSubstitute();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _factory.DisposeAsync();
    }

    [Test]
    public async Task ValidId_Should_BeAccepted()
    {
        _beansService
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(Task.FromResult(Substitute.For<IBeanDTO>()));

        using var httpClient = _factory.CreateClient();
        var id = Guid.NewGuid();
        var endpoint = $"/beans/{id}";
        using var response = await httpClient.GetAsync(endpoint);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        await _beansService
            .Received(1)
            .GetByIdAsync(id, Arg.Any<CancellationToken>());
    }

    [TestCase("/beans/123")]
    [TestCase("/beans/invalid-value")]
    [Description("Invalid query parameters should return Bad Request status code")]
    public async Task InvalidRequests_Should_ReturnBadRequest(string endpoint)
    {
        using var httpClient = _factory.CreateClient();

        using var response = await httpClient.GetAsync(endpoint);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
