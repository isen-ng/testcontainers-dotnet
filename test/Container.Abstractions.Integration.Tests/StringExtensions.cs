using System;

namespace Container.Abstractions.Integration.Tests
{
    public static class StringExtensions
    {
        public static string TrimEndNewLine(this string input)
        {
            return input.TrimEnd(Environment.NewLine.ToCharArray());
        }
    }
}