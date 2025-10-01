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

namespace AllTheBeans.API.IntegrationTests.BeansControllerTests.PATCH;

[TestFixture(TestOf = typeof(BeansController))]
internal class ValidationTests
{
    private const string TestFilesLocation = "Beans\\PATCH";
    private const string Endpoint = "/beans/0199a055-1540-77eb-8bdf-b2ff9cc37089";

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
    [TestCaseSource(typeof(TestPayloadProvider), nameof(TestPayloadProvider.GetValidPayloadFilePaths), new object[] { TestFilesLocation })]
    [Description("Valid requests should update the bean")]
    public async Task ValidRequests_Should_UpdateTheBean(string file)
    {
        using var content = await ContentBuilder.BuildJsonContentFromFile(file);
        using var httpClient = _factory.CreateClient();

        using var response = await httpClient.PatchAsync(Endpoint, content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        await _beansService
            .Received(1)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<IUpdateBeanDTO>(), Arg.Any<CancellationToken>());
    }

    [Test]
    [TestCaseSource(typeof(TestPayloadProvider), nameof(TestPayloadProvider.GetInvalidPayloadFilePaths), new object[] { TestFilesLocation })]
    [Description("Invalid requests should return Bad Request status code")]
    public async Task InvalidRequests_Should_ReturnBadRequest(string file)
    {
        using var content = await ContentBuilder.BuildJsonContentFromFile(file);

        using var httpClient = _factory.CreateClient();
        using var response = await httpClient.PatchAsync(Endpoint, content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        await _beansService
            .DidNotReceiveWithAnyArgs()
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<IUpdateBeanDTO>(), Arg.Any<CancellationToken>());

        await ResponseAssertionHelper.VerifyBadRequest(response, file);
    }
}
