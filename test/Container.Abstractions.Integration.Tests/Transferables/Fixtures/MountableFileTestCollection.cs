using Xunit;

namespace Container.Abstractions.Integration.Tests.Transferables.Fixtures
{
    [CollectionDefinition(CollectionName)]
    public class MountableFileTestCollection : ICollectionFixture<MountableFileFixture>
    {
        public const string CollectionName = nameof(MountableFileTestCollection);
    }
}
