namespace TestContainers.Container.Abstractions.Models
{
    public class Bind
    {
        public string HostPath { get; set; }

        public string ContainerPath { get; set; }

        public AccessMode AccessMode { get; set; } = AccessMode.ReadOnly;
    }
}