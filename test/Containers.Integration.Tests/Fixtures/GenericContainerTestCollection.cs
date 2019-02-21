using Xunit;

namespace Containers.Integration.Tests.Fixtures
{
    [CollectionDefinition(CollectionName)]
    public class GenericContainerTestCollection : ICollectionFixture<GenericContainerFixture>
    {
        public const string CollectionName = "GenericContainerTestCollection";
    }
}