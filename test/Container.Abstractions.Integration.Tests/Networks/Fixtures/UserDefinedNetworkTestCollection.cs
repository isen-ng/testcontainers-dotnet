using Xunit;

namespace Container.Abstractions.Integration.Tests.Networks.Fixtures
{
    [CollectionDefinition(CollectionName)]
    public class UserDefinedNetworkTestCollection : ICollectionFixture<UserDefinedNetworkFixture>
    {
        public const string CollectionName = nameof(UserDefinedNetworkTestCollection);
    }
}
