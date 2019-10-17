using System;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Container.Abstractions.Integration.Tests.Images.Fixtures;
using Container.Test.Utility;
using Container.Test.Utility.Platforms;
using Docker.DotNet;
using TestContainers.Container.Abstractions;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Abstractions.Images;
using Xunit;

namespace Container.Abstractions.Integration.Tests.Images
{
    [Collection(DockerfileImageTestCollection.CollectionName)]
    public class DockerfileImageTests
    {
        private readonly DockerfileImageFixture _fixture;

        private IImage Image => _fixture.Image;

        private IDockerClient DockerClient => _fixture.DockerClient;

        private IPlatformSpecific PlatformSpecific => _fixture.PlatformSpecific;

        public DockerfileImageTests(DockerfileImageFixture fixture)
        {
            _fixture = fixture;
        }

        public class ResolveTests : DockerfileImageTests
        {
            public ResolveTests(DockerfileImageFixture fixture) : base(fixture)
            {
            }

            [Fact]
            public async Task ShouldResolveImageCorrectly()
            {
                // act
                var actualImageId = await Image.Resolve();

                // assert
                var actualImage = await DockerClient.Images.InspectImageAsync(Image.ImageName);
                Assert.Equal(actualImage.ID, actualImageId);
            }
        }

        public class WithContainer : DockerfileImageTests
        {
            public WithContainer(DockerfileImageFixture fixture) : base(fixture)
            {
            }

            [Fact]
            public async Task ShouldCreateAndStartContainerSuccessfully()
            {
                // arrange
                var container = new ContainerBuilder<GenericContainer>()
                    .ConfigureDockerImage(Image)
                    .ConfigureContainer((h, c) =>
                    {
                        c.ExposedPorts.Add(80);
                    })
                    .Build();

                // act
                await container.StartAsync();

                // assert
                var mappedPort = container.GetMappedPort(80);
                var host = $"http://localhost:{mappedPort}";

                AssertFileExists($"{host}/dummy.txt", DockerfileImageFixture.DockerfileImageContext + "/dummy.txt");
                AssertFileExists($"{host}/file1.txt", DockerfileImageFixture.DockerfileImageTransferableFile);
                AssertFileExists($"{host}/folder1/file1.txt", DockerfileImageFixture.DockerfileImageTransferableFolder + "/file1.txt");

                // ignored by .dockerignore
                AssertFileDoesNotExists($"{host}/dummy2.txt");
            }

            private static void AssertFileExists(string httpPath, string localPath)
            {
                var actual = HttpClientHelper.MakeGetRequest(httpPath);
                var expected = File.ReadAllText(localPath);

                Assert.Equal(expected, actual);
            }

            private static void AssertFileDoesNotExists(string httpPath)
            {
                try
                {
                    HttpClientHelper.MakeGetRequest(httpPath);
                    Assert.True(false);
                }
                catch (WebException e)
                {
                    if (e.Status != WebExceptionStatus.ProtocolError)
                    {
                        throw;
                    }

                    var status = (e.Response as HttpWebResponse)?.StatusCode;
                    Assert.Equal(HttpStatusCode.NotFound, status);
                }
            }
        }
    }
}
