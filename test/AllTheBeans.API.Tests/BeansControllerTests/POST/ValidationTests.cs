using AllTheBeans.API.Controllers;
using AllTheBeans.API.IntegrationTests.Helpers;
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

namespace AllTheBeans.API.IntegrationTests.BeansControllerTests.POST;

[TestFixture(TestOf = typeof(BeansController))]
internal class ValidationTests
{
    private const string TestFilesLocation = "Beans\\POST";
    private const string Endpoint = "/beans";

    private readonly IBeansService _beansInitialisationService = 
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
    [TestCaseSource(typeof(TestPayloadProvider), nameof(TestPayloadProvider.GetValidPayloadFilePaths), new object[] { TestFilesLocation })]
    [Description("Valid requests should create a new bean")]
    public async Task ValidRequests_Should_CreateANewBean(string file)
    {
        using var content = await ContentBuilder.BuildJsonContentFromFile(file);
        using var httpClient = _factory.CreateClient();

        using var response = await httpClient.PostAsync(Endpoint, content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        await _beansInitialisationService
            .Received(1)
            .CreateAsync(Arg.Any<ICreateBeanDTO>(), Arg.Any<CancellationToken>());
    }

    [Test]
    [TestCaseSource(typeof(TestPayloadProvider), nameof(TestPayloadProvider.GetInvalidPayloadFilePaths), new object[] { TestFilesLocation })]
    [Description("Invalid requests should return Bad Request status code")]
    public async Task InvalidRequests_Should_ReturnBadRequest(string file)
    {
        using var content = await ContentBuilder.BuildJsonContentFromFile(file);

        using var httpClient = _factory.CreateClient();
        using var response = await httpClient.PostAsync(Endpoint, content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        await _beansInitialisationService
            .DidNotReceiveWithAnyArgs()
            .CreateAsync(Arg.Any<ICreateBeanDTO>(), Arg.Any<CancellationToken>());

        await ResponseAssertionHelper.VerifyBadRequest(response, file);
    }
}
