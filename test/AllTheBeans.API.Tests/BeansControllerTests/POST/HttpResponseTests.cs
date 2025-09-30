using AllTheBeans.API.IntegrationTests.Helpers;
using AllTheBeans.Domain.DataModels;
using AllTheBeans.Domain.Services;
using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.ExceptionExtensions;
using System.Net;

namespace AllTheBeans.API.IntegrationTests.BeansControllerTests.POST;
internal class HttpResponseTests
{
    private const string TestFilesLocation = "Beans\\POST";
    private const string Endpoint = "/beans";

    private readonly IBeansInitialisationService _beansInitialisationService =
        Substitute.For<IBeansInitialisationService>();
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
                    services.Replace(ServiceDescriptor.Scoped(_ => _beansInitialisationService));
                });
            });
    }

    [TearDown]
    public void TearDown()
    {
        _beansInitialisationService.ClearSubstitute();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _factory.DisposeAsync();
    }


    [Test]
    [Description("Empty body should return Unsupported Media Type status code")]
    public async Task EmptyBodyWithoutContentType_Should_ReturnUnsupportedMediaType()
    {
        using var content = new StringContent(string.Empty);

        using var httpClient = _factory.CreateClient();
        using var response = await httpClient.PostAsync(Endpoint, content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnsupportedMediaType));
    }


    [Test]
    [Description("Conflict status code should be returned when duplicated bean is created")]
    public async Task DuplicatedBean_Should_ReturnCoflict()
    {
        _beansInitialisationService
            .InitiliseAsync(Arg.Any<ICreateBeanDTO>(), Arg.Any<CancellationToken>())
            .ThrowsAsyncForAnyArgs(new UniqueConstraintException());
        var file = TestPayloadProvider.GetFirstValidPayloadFilePath(TestFilesLocation);
        using var content = await ContentBuilder.BuildJsonContentFromFile(file);

        using var httpClient = _factory.CreateClient();
        using var response = await httpClient.PostAsync(Endpoint, content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    [Description("Valid bean should return HTTP status code OK with the Id of the created entity")]
    public async Task ValidBean_Should_ReturnHttpStatusCodeOkWithIdOfTheCreatedEntity()
    {
        var beanId = Guid.NewGuid();
        _beansInitialisationService
            .InitiliseAsync(Arg.Any<ICreateBeanDTO>(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(Task.FromResult(beanId));
        var file = TestPayloadProvider.GetFirstValidPayloadFilePath(TestFilesLocation);
        using var content = await ContentBuilder.BuildJsonContentFromFile(file);

        using var httpClient = _factory.CreateClient();
        using var response = await httpClient.PostAsync(Endpoint, content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    [Description("Internal Server Error should be returned when an exception occurs")]
    public async Task InternalServerError_ShouldBe_Returned_When_AnExceptionOccurs()
    {
        var exception = new Exception("Something went wrong");
        _beansInitialisationService
            .InitiliseAsync(Arg.Any<ICreateBeanDTO>(), Arg.Any<CancellationToken>())
            .ThrowsAsyncForAnyArgs(exception);
        var file = TestPayloadProvider.GetFirstValidPayloadFilePath(TestFilesLocation);
        using var content = await ContentBuilder.BuildJsonContentFromFile(file);

        using var httpClient = _factory.CreateClient();
        using var response = await httpClient.PostAsync(Endpoint, content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.That(responseBody, Is.EqualTo(exception.Message));
    }
}
