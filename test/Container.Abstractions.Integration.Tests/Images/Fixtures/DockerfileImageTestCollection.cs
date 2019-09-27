using Xunit;

namespace Container.Abstractions.Integration.Tests.Images.Fixtures
{
    [CollectionDefinition(CollectionName)]
    public class DockerfileImageTestCollection : ICollectionFixture<DockerfileImageFixture>
    {
        public const string CollectionName = nameof(DockerfileImageTestCollection);
    }
}
