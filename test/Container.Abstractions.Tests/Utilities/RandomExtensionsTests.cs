using System;
using TestContainers.Container.Abstractions.Utilities;
using Xunit;

namespace Container.Abstractions.Tests.Utilities
{
    public class RandomExtensionsTests
    {
        private readonly Random _random;

        protected RandomExtensionsTests()
        {
            _random = new Random();
        }

        public class NextAlphaNumeric : RandomExtensionsTests
        {
            [Fact]
            public void ShouldReturnCorrectNumberOfCharacters()
            {
                // arrange
                var length = _random.Next(0, int.MaxValue / 16);

                // act
                var result = _random.NextAlphaNumeric(length);

                // assert
                Assert.Equal(length, result.Length);
            }
        }
    }
}
