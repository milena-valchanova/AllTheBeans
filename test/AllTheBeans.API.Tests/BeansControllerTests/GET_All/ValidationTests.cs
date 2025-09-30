using AllTheBeans.API.Controllers;
using AllTheBeans.API.Mappers;
using AllTheBeans.Domain.DataModels;
using AllTheBeans.Domain.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using NSubstitute.ClearExtensions;
using System.Net;

namespace AllTheBeans.API.IntegrationTests.BeansControllerTests.GET_All;

[TestFixture(TestOf = typeof(BeansController))]
internal class ValidationTests
{
    private readonly IBeansRepository _beansRepository = 
        Substitute.For<IBeansRepository>();
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
                    services.Replace(ServiceDescriptor.Scoped(_ => _beansRepository));
                    services.Replace(ServiceDescriptor.Singleton(_ => _beansMapper));
                });
            });
    }

    [TearDown]
    public void TearDown()
    {
        _beansRepository.ClearSubstitute();
        _beansMapper.ClearSubstitute();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _factory.DisposeAsync();
    }

    [TestCase("/beans", 1, 10)]
    [TestCase("/beans?PageNumber=5&PageSize=15", 5, 15)]
    [TestCase("/beans?pageNumber=5&pageSize=15", 5, 15)]
    [TestCase("/beans?pageSize=5&pageNumber=15", 15, 5)]
    [Description("Valid query parameters are accepted")]
    public async Task ValidQueryParameters_Should_BeAccepted(string endpoint, int expectedPageNumber, int expectedPageSize)
    {
        _beansRepository
            .GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(Task.FromResult(new List<IBeanDTO>()));

        using var httpClient = _factory.CreateClient();

        using var response = await httpClient.GetAsync(endpoint);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        await _beansRepository
            .Received(1)
            .GetAllAsync(expectedPageNumber, expectedPageSize, Arg.Any<CancellationToken>());
    }

    [TestCase("/beans", 1, 10)]
    [TestCase("/beans?PageNumber=5", 5, 10)]
    [TestCase("/beans?pageSize=15", 1, 15)]
    [TestCase("/beans?anotherParameter=5", 1, 10)]
    [Description("Default query parameters are applied when they are not provided")]
    public async Task DefaultQueryParameters_Should_BeApplied_When_NotProvided(string endpoint, int expectedPageNumber, int expectedPageSize)
    {
        _beansRepository
            .GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(Task.FromResult(new List<IBeanDTO>()));

        using var httpClient = _factory.CreateClient();

        using var response = await httpClient.GetAsync(endpoint);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        await _beansRepository
            .Received(1)
            .GetAllAsync(expectedPageNumber, expectedPageSize, Arg.Any<CancellationToken>());
    }

    [TestCase("/beans?PageNumber=0&PageSize=0")]
    [TestCase("/beans?pageNumber=-1&pageSize=-1")]
    [TestCase("/beans?pageNumber=-1")]
    [TestCase("/beans?pageSize=0")]
    [Description("Invalid query parameters should return Bad Request status code")]
    public async Task InvalidRequests_Should_ReturnBadRequest(string endpoint)
    {
        using var httpClient = _factory.CreateClient();

        using var response = await httpClient.GetAsync(endpoint);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
