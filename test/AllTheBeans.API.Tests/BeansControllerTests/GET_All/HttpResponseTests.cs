using AllTheBeans.API.Controllers;
using AllTheBeans.API.DataModels;
using AllTheBeans.API.Mappers;
using AllTheBeans.Domain.DataModels;
using AllTheBeans.Domain.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.ExceptionExtensions;
using System.Net;
using System.Net.Http.Json;

namespace AllTheBeans.API.IntegrationTests.BeansControllerTests.GET_All;

[TestFixture(TestOf = typeof(BeansController))]
internal class HttpResponseTests
{
    private const string Endpoint = "/beans";

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

    [Test]
    [Description("GetAll should return HTTP status code OK with correct response")]
    public async Task GetAll_Should_ReturnHttpStatusCodeOkWithCorrectResponse()
    {
        _beansRepository
            .GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(Task.FromResult(new List<IBeanDTO>()));
        var totalNumberOfBeans = 5;
        _beansRepository
            .CountAllAsync(Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(totalNumberOfBeans);

        using var httpClient = _factory.CreateClient();
        using var response = await httpClient.GetAsync(Endpoint);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await response.Content.ReadFromJsonAsync<BeansResponse>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Total, Is.EqualTo(totalNumberOfBeans));
        Assert.That(result.Beans, Is.Empty);
    }

    [Test]
    [Description("Internal Server Error should be returned when an exception occurs")]
    public async Task InternalServerError_ShouldBe_Returned_When_AnExceptionOccurs()
    {
        var exception = new Exception("Something went wrong");
        _beansRepository
            .GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ThrowsAsyncForAnyArgs(exception);

        using var httpClient = _factory.CreateClient();
        using var response = await httpClient.GetAsync(Endpoint);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.That(responseBody, Is.EqualTo(exception.Message));
    }
}
