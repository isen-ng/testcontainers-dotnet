using Xunit;

namespace Container.Abstractions.Integration.Tests.Images.Fixtures
{
    [CollectionDefinition(CollectionName)]
    public class GenericImageTestCollection : ICollectionFixture<GenericImageFixture>
    {
        public const string CollectionName = nameof(GenericImageTestCollection);
    }
}
