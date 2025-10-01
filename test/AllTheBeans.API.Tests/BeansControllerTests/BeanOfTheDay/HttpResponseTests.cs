using AllTheBeans.API.Controllers;
using AllTheBeans.Domain.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.ExceptionExtensions;
using System.Net;

namespace AllTheBeans.API.IntegrationTests.BeansControllerTests.BeanOfTheDay;

[TestFixture(TestOf = typeof(BeansController))]
internal class HttpResponseTests
{
    private const string Endpoint = "/beans/of-the-day";

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

    [Test]
    [Description("Not Found status code should be returned when a bean of the day could not be determined")]
    public async Task NotFoundStatusCode_ShouldBe_Returned_When_ABeanOfTheDayCouldNotBeDetermined()
    {
        var exception = new KeyNotFoundException("Not found");
        _beansService
            .GetOrCreateBeanOfTheDayAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .ThrowsAsyncForAnyArgs(exception);

        using var httpClient = _factory.CreateClient();
        using var response = await httpClient.GetAsync(Endpoint);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.That(responseBody, Is.EqualTo(exception.Message));
    }

    [Test]
    [Description("Internal Server Error should be returned when an exception occurs")]
    public async Task InternalServerError_ShouldBe_Returned_When_AnExceptionOccurs()
    {
        var exception = new Exception("Something went wrong");
        _beansService
            .GetOrCreateBeanOfTheDayAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .ThrowsAsyncForAnyArgs(exception);

        using var httpClient = _factory.CreateClient();
        using var response = await httpClient.GetAsync(Endpoint);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.That(responseBody, Is.EqualTo(exception.Message));
    }
}
