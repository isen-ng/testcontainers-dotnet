using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace TestContainers.Containers.Exceptions
{
    public class ContainerLaunchException : Exception
    {
        public ContainerLaunchException()
        {
        }

        protected ContainerLaunchException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ContainerLaunchException(string message) : base(message)
        {
        }

        public ContainerLaunchException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}