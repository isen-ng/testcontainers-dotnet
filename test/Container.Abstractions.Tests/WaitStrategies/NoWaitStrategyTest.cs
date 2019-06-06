using TestContainers.Container.Abstractions.WaitStrategies;
using Xunit;

namespace Container.Abstractions.Tests.WaitStrategies
{
    public class NoWaitStrategyTest
    {
        private readonly IWaitStrategy _strategy;

        public NoWaitStrategyTest()
        {
            _strategy = new NoWaitStrategy();
        }

        [Fact]
        public void ShouldReturnSuccessImmediately()
        {
            // act
            var result = _strategy.WaitUntil(null);

            // assert
            Assert.True(result.IsCompletedSuccessfully);
        }
    }
}
